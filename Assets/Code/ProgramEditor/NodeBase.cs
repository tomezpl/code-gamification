using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class NodeBase : MonoBehaviour, IProgramNode
{
    // Points to the next node
    public IProgramNode nextNode;
    public IProgramNode prevNode;

    // GameObject that holds the nextNode
    public GameObject NextNodeObject;

    public GameObject PrevNodeObject;

    // Python syntax-specific: indentation level (default: 0, increases with nested CodeBlocks)
    public int indentLevel = 0;

    public bool isInitialised = false;

    public abstract string Serialize();

    public bool inLoop = false;

    protected static GameObject nodeLinkerPrefab;
    protected static Material lineMaterial;

    protected GameObject nodeLinker;

    public ProgramController computer;

    // a conditional statement, like an if statement or a loop, inside which this Node is nested
    public LogicalBlock ownerLoop;

    // Finds previous nodes for all nodes in the program this node belongs to
    void FindPrevNode()
    {
        NodeBase[] nodes = computer.editorUi.GetComponent<EditorProgram>().elementContainer.GetComponentsInChildren<NodeBase>();
        foreach(NodeBase node in nodes)
        {
            if(node.NextNodeObject != null && node.NextNodeObject.GetComponent<NodeBase>().PrevNodeObject == null)
            {
                node.NextNodeObject.GetComponent<NodeBase>().PrevNodeObject = node.gameObject;
            }
            if (node.GetComponent<CodeBlock>() != null && node.GetComponent<CodeBlock>().FirstBodyNodeObject != null && node.GetComponent<CodeBlock>().FirstBodyNodeObject.GetComponent<NodeBase>().PrevNodeObject == null)
            {
                node.GetComponent<CodeBlock>().FirstBodyNodeObject.GetComponent<NodeBase>().PrevNodeObject = node.gameObject;
            }
        }
    }

    public virtual void Start()
    {
        // Find owner computer
        {
            Transform computerTransform = transform.parent;
            while (computerTransform)
            {
                if (computerTransform.GetComponent<EditorProgram>())
                {
                    computer = computerTransform.GetComponent<EditorProgram>().programController;
                    break;
                }
                else
                {
                    computerTransform = computerTransform.parent;
                }
            }
        }

        FindPrevNode();

        // Unless overriden, not null and not in a loop, assign nextNode as node interface of the NextNodeObject
        if(nextNode == null && /*!inLoop &&*/ NextNodeObject != null)
        {
            NodeBase nextNodeBase = NextNodeObject.GetComponent<NodeBase>();
            nextNode = (IProgramNode)nextNodeBase;
        }
        if (prevNode == null && /*!inLoop &&*/ PrevNodeObject != null)
        {
            NodeBase prevNodeBase = PrevNodeObject.GetComponent<NodeBase>();
            prevNode = (IProgramNode)prevNodeBase;
        }
        if (prevNode == null && /*!inLoop &&*/ ownerLoop != null)
        {
            PrevNodeObject = ownerLoop.gameObject;
            NodeBase prevNodeBase = PrevNodeObject.GetComponent<NodeBase>();
            prevNode = (IProgramNode)prevNodeBase;
        }
        if (NextNodeObject == null)
        {
            Logger.LogWarning($"NextNodeObject on {gameObject}::{this} is set to null! If this is intended (e.g. last node in a loop), this should be corrected on startup.");
        }
        if (PrevNodeObject == null)
        {
            Logger.LogWarning($"PrevNodeObject on {gameObject}::{this} is set to null! This is usually not intended.");
        }

        // TODO: Perhaps search for ProgramEnd as nextNode fallback?

        // TODO: only initialise if !isInitialised?
        InitialiseNode();

    }

    // Not used anymore
    public virtual void RenderNodeLinker()
    {
        nodeLinker.transform.SetParent(null, false);

        Rect thisRect = GetComponent<RectTransform>().rect;
        Rect nodeLinkerRect = nodeLinker.GetComponent<RectTransform>().rect;
        Vector2 linkerOrigin = new Vector2(thisRect.xMax + nodeLinkerRect.width / 2, thisRect.yMin + thisRect.height / 2);
        float rightSideAngle = 270.0f; // -90 degrees points to the right side of the node; we use this as a reference.

        float angleToNextNode = 0.0f;
        if (NextNodeObject != null)
        {
            Rect nextNodeRect = NextNodeObject.GetComponent<RectTransform>().rect;
            angleToNextNode = Vector3.SignedAngle(new Vector3(thisRect.xMax, thisRect.y), new Vector3(nextNodeRect.xMin, nextNodeRect.y), Vector3.forward);
        }
        nodeLinker.GetComponent<RectTransform>().SetPositionAndRotation(linkerOrigin, Quaternion.Euler(0.0f, 0.0f, rightSideAngle + angleToNextNode));

        if (transform.Find("NodeLinker") == null)
            nodeLinker.transform.SetParent(transform, false); // add as child (affects transforms!)
    }

    public virtual void Update()
    {
        //RenderNodeLinker();

        // Propagate ownerLoop
        if(NextNodeObject != null)
        {
            NextNodeObject.GetComponent<NodeBase>().ownerLoop = ownerLoop;
        }
    }

    public void Awake()
    {
        //Start();
    }

    public virtual void Reset()
    {
        //isInitialised = false;
        //InitialiseNode();
    }

    public virtual void InitialiseNode()
    {
        isInitialised = true;
    }

    // Returns tabulation for the current line according to indentLevel.
    public virtual string GetLineTabs()
    {
        string lineTabs = "";

        for(int i = 0; i < indentLevel; i++)
        {
            lineTabs += "\t";
        }

        Logger.Log($"{this} returned {indentLevel} lineTabs.");

        return lineTabs;
    }

    // Finds the "Elements" object which is a root container for all nodes in the program diagram.
    public Transform FindElementContainer()
    {
        Transform elements = transform.parent;
        while(elements.name != "Elements" && elements.parent != null)
        {
            elements = elements.parent;
        }

        return elements;
    }

    // Delete this node from the program
    public void DeleteNode()
    {
        NodeBase[] nodes = FindElementContainer().GetComponentsInChildren<NodeBase>();
        foreach(NodeBase node in nodes)
        {
            if(node.NextNodeObject == gameObject)
            {
                node.NextNodeObject = NextNodeObject;
                node.nextNode = nextNode;

                if(node.GetComponent<CodeBlock>() != null)
                {
                    ((NodeBase)(node.GetComponent<CodeBlock>().firstBodyNode)).ownerLoop = null;
                    ((NodeBase)(node.GetComponent<CodeBlock>().firstBodyNode)).PropagateOwnershipChanges();
                }

                break;
            }
        }
        Destroy(gameObject);
    }

    // If the node is inside an if-statement/loop, set all its nextNodes to belong to the same code block (ownerLoop)
    public void PropagateOwnershipChanges()
    {
        NodeBase currentNode = (NodeBase)nextNode;

        while(currentNode != null)
        {
            currentNode.ownerLoop = ownerLoop;
            currentNode = (NodeBase)currentNode.nextNode;
        }
    }
}
