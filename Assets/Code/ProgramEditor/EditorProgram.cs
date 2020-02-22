﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorProgram : MonoBehaviour
{
    private Canvas lineCanvas;
    private static Material lineMaterial = null;

    public GameObject elementContainer;

    public ProgramStart programStart;
    public ProgramController programController;

    public int framesToDisable = 2;

    int frameCounter = 0;
    protected static Dictionary<string, KeyValuePair<string, GameObject>> nodePrefabs = null;

    public GUISkin guiSkin;
    
    public bool EditorActive {
        get { return editorActive; }
        set {
            if (!elementContainer)
            {
                elementContainer = transform.Find("Canvas").Find("Elements").gameObject;
            }
            if (!lineCanvas)
            {
                lineCanvas = transform.Find("LineCanvas").GetComponent<Canvas>();
            }
            if (value == true && editorActive != true)
            {
                EnableEditor();
            }
            else if(value == false && editorActive != false)
            {
                DisableEditor();
            }
        }
    }

    private bool editorActive = false;

    public bool enableEditorOnStartup = false;

    // Editor state
    private bool choosingNode = false;
    public bool linkingNodes = false;
    public GameObject[] linkingNodesObjects = new GameObject[2];
    public bool editingNodeProperty = false;
    public string editingNodeValue = ""; // original value of the string that is edited - this will be returned if Esc is pressed
    public string editedNodeValue = ""; // final edited value to return if Enter is pressed
    public bool editingNodeInPlace = false;
    public Rect editingNodeInPlaceRect = new Rect();
    public System.Action<string> editingNodeFinishedClb = null;

    void DisableEditor()
    {
        foreach (EditorDraggableNode draggable in elementContainer.GetComponentsInChildren<EditorDraggableNode>())
        {
            draggable.enabled = false;
        }
        lineCanvas.worldCamera.enabled = false;
        lineCanvas.GetComponent<Canvas>().enabled = false;
        foreach (LineRenderer line in lineCanvas.GetComponentsInChildren<LineRenderer>())
        {
            line.enabled = false;
        }
        if (GameObject.Find("Player"))
            GameObject.Find("Player").GetComponentInChildren<Camera>().enabled = true;
        transform.Find("Canvas").GetComponent<Canvas>().enabled = false;
        editorActive = false;
    }

    void EnableEditor()
    {
        foreach (EditorDraggableNode draggable in elementContainer.GetComponentsInChildren<EditorDraggableNode>())
        {
            draggable.enabled = true;
        }
        lineCanvas.worldCamera.enabled = true;
        lineCanvas.GetComponent<Canvas>().enabled = true;
        foreach(LineRenderer line in lineCanvas.GetComponentsInChildren<LineRenderer>())
        {
            line.enabled = true;
        }
        if (GameObject.Find("Player"))
            GameObject.Find("Player").GetComponentInChildren<Camera>().enabled = false;
        transform.Find("Canvas").GetComponent<Canvas>().enabled = true;
        editorActive = true;
    }

    GameObject AddNode(string type = "NodeBase", float x = 0.0f, float y = 0.0f)
    {
        GameObject nodeObject = Instantiate(nodePrefabs[type].Value, elementContainer.transform);

        nodeObject.transform.localPosition = new Vector3(x, y, 0.0f);

        return nodeObject;
    }

    public void LinkCurrentlySelectedObjects()
    {
        linkingNodes = false;
        linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject = linkingNodesObjects[1];
        if (linkingNodesObjects[1].GetComponent<NodeBase>())
        {
            linkingNodesObjects[0].GetComponent<NodeBase>().nextNode = linkingNodesObjects[1].GetComponent<NodeBase>();
        }
        linkingNodesObjects[0] = linkingNodesObjects[1] = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Load editor node prefabs from Resources
        if(nodePrefabs == null)
        {
            nodePrefabs = new Dictionary<string, KeyValuePair<string, GameObject>>();
            nodePrefabs.Add("AssignValue", new KeyValuePair<string, GameObject>("Set variable: Assigns a value to a variable. The variable is created if it doesn't exist.", Resources.Load("Prefabs/ProgramEditor/Nodes/Operations/AssignValue") as GameObject));
            nodePrefabs.Add("FunctionCallBase", new KeyValuePair<string, GameObject>("Function call: Triggers the specified function. Can pass parameters.", Resources.Load("Prefabs/ProgramEditor/Nodes/Operations/FunctionCall") as GameObject));
        }

        if(!lineCanvas)
            lineCanvas = transform.Find("LineCanvas").GetComponent<Canvas>();

        if(!lineMaterial)
            lineMaterial = Resources.Load("Materials/LineMaterial") as Material;

        if (!programStart)
            programStart = elementContainer.GetComponentInChildren<ProgramStart>();

        if (!enableEditorOnStartup)
        {
            DisableEditor();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Lock player movement if in editor
        GameObject.Find("Player").GetComponent<FPPControl>().allowMove = !EditorActive;

        DrawNodeLinks();

        // Editor creation occurs at startup so we may need a couple of frames to disable it for good measure?
        if (frameCounter < framesToDisable)
        {
            if (!enableEditorOnStartup)
            {
                DisableEditor();
            }
            frameCounter++;
        }

        if(Input.GetKeyUp(KeyCode.Tab) && EditorActive)
        {
            choosingNode = !choosingNode;
        }

        if (editingNodeProperty)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                editingNodeFinishedClb.DynamicInvoke(editingNodeValue);
                editingNodeProperty = false;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                editingNodeFinishedClb.DynamicInvoke(editedNodeValue);
                editingNodeProperty = false;
            }
        }
    }

    private Vector2 ndcToScreen(Vector2 ndc)
    {
        return new Vector2(ndc.x * Screen.width, ndc.y * Screen.height);
    }

    private Vector2 ndcToScreen(float x, float y)
    {
        return ndcToScreen(new Vector2(x, y));
    }

    private void OnGUI()
    {
        GUI.skin = guiSkin;

        if (choosingNode)
        {
            GUI.Box(new Rect(ndcToScreen(0.5f - 0.5f / 2.0f, 0.25f), ndcToScreen(0.5f, 0.5f)), "Choose a node type to add:");

            List<string> nodeTypeStrings = new List<string>();
            foreach(string nodeTypeString in nodePrefabs.Keys)
            {
                nodeTypeStrings.Add(nodeTypeString);
            }

            for (int i = 0; i < nodeTypeStrings.Count; i++)
            {
                float buttonHeight = 0.05f;
                float buttonWidth = 0.475f;
                bool pressed = GUI.Button(new Rect(ndcToScreen(0.5f - buttonWidth/2.0f, 0.3f + buttonHeight * (float)i + 0.01f * (float)i), ndcToScreen(buttonWidth, buttonHeight)), nodePrefabs[nodeTypeStrings[i]].Key);
                if(pressed)
                {
                    AddNode(nodeTypeStrings[i]);
                    choosingNode = false;
                    break;
                }
            }
        }

        if (editingNodeProperty)
        {
            if (editingNodeInPlace)
            {
                float x = editingNodeInPlaceRect.x;
                float y = editingNodeInPlaceRect.y;
                float width = editingNodeInPlaceRect.width;
                float height = editingNodeInPlaceRect.height;
                float cvsHeight = elementContainer.GetComponentInParent<RectTransform>().rect.height;
                editedNodeValue = GUI.TextField(new Rect(x, Screen.height - (y + height), width, height), editedNodeValue);
            }
            else
            {
                editedNodeValue = GUI.TextField(new Rect(ndcToScreen(0.5f - 0.375f / 2.0f, 0.5f - 0.05f / 2.0f), ndcToScreen(0.375f, 0.05f)), editedNodeValue);
            }
        }
    }

    void DrawNodeLinks()
    {
        List<GameObject> elements = new List<GameObject>();
        for(int childIndex = 0; childIndex < transform.Find("Canvas").Find("Elements").childCount; childIndex++)
        {
            elements.Add(transform.Find("Canvas").Find("Elements").GetChild(childIndex).gameObject);
        }
        /*LineRenderer renderer = lineCanvas.transform.Find("NextNodeLines").GetComponent<LineRenderer>();
        renderer.SetPositions(new List<Vector3>().ToArray());
        renderer.positionCount = 0;
        if (elements.Count > 2)
        {
            GameObject currentObject = elements[0];
            {
                while (currentObject.GetComponent<NodeBase>() != null && currentObject.GetComponent<NodeBase>().NextNodeObject != null)
                {
                    renderer.positionCount += 2;
                    renderer.SetPosition(renderer.positionCount - 2, currentObject.GetComponent<RectTransform>().localPosition);
                    renderer.SetPosition(renderer.positionCount - 1, currentObject.GetComponent<NodeBase>().NextNodeObject.GetComponent<RectTransform>().localPosition);

                    currentObject = currentObject.GetComponent<NodeBase>().NextNodeObject;
                }
            }
        }*/

        // Names of valid link GameObjects
        List<string> validLinks = new List<string>();

        // First, render the next-node links
        foreach (GameObject element in elements)
        {
            NodeBase potentialNode = element.GetComponent<NodeBase>();
            if (potentialNode && potentialNode.NextNodeObject)
            {
                GameObject currentLink = null;
                LineRenderer currentRenderer = null;
                Transform cachedLinkTransform = lineCanvas.transform.Find($"nextNode:{potentialNode.name}->{potentialNode.NextNodeObject.name}");
                if (cachedLinkTransform)
                {
                    currentLink = cachedLinkTransform.gameObject;
                    currentRenderer = currentLink.GetComponent<LineRenderer>();

                    currentLink.GetComponent<LinkDescriptor>().prev = potentialNode.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialNode.NextNodeObject;
                }
                else
                {
                    currentLink = new GameObject($"nextNode:{potentialNode.name}->{potentialNode.NextNodeObject.name}");
                    currentLink.transform.SetParent(lineCanvas.transform, false);
                    currentRenderer = currentLink.AddComponent<LineRenderer>();
                    currentRenderer.material = lineMaterial;
                    currentRenderer.useWorldSpace = false;

                    currentLink.AddComponent<LinkDescriptor>();
                    currentLink.GetComponent<LinkDescriptor>().prev = potentialNode.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialNode.NextNodeObject;
                }
                // This is to prevent culling from other 3D elements in the scene
                currentRenderer.gameObject.layer = LayerMask.NameToLayer("CodeEditor");
                Rect    thisRect = potentialNode.GetComponent<RectTransform>().rect,
                        nextRect = potentialNode.NextNodeObject.GetComponent<RectTransform>().rect;
                Vector2 thisPos = new Vector2(potentialNode.GetComponent<RectTransform>().localPosition.x + thisRect.width / 2, potentialNode.GetComponent<RectTransform>().localPosition.y);
                Vector2 nextPos = new Vector2(potentialNode.NextNodeObject.GetComponent<RectTransform>().localPosition.x - nextRect.width / 2, potentialNode.NextNodeObject.GetComponent<RectTransform>().localPosition.y);
                currentRenderer.positionCount = 2;
                currentRenderer.SetPositions(new Vector3[] {
                    thisPos,
                    nextPos
                });
                validLinks.Add(currentLink.name);
            }
        }

        // Now create separate LineRenderers for FirstBodyNodeObjects
        foreach (GameObject element in elements)
        {
            CodeBlock potentialCodeBlock = element.GetComponent<CodeBlock>();
            if (potentialCodeBlock && potentialCodeBlock.FirstBodyNodeObject)
            {
                GameObject currentLink = null;
                LineRenderer currentRenderer = null;
                Transform cachedLinkTransform = lineCanvas.transform.Find($"firstBody:{potentialCodeBlock.name}->{potentialCodeBlock.FirstBodyNodeObject.name}");
                if (cachedLinkTransform)
                {
                    currentLink = cachedLinkTransform.gameObject;
                    currentRenderer = currentLink.GetComponent<LineRenderer>();

                    currentLink.GetComponent<LinkDescriptor>().prev = potentialCodeBlock.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialCodeBlock.FirstBodyNodeObject;
                }
                else
                {
                    currentLink = new GameObject($"firstBody:{potentialCodeBlock.name}->{potentialCodeBlock.FirstBodyNodeObject.name}");
                    currentLink.transform.SetParent(lineCanvas.transform, false);
                    currentRenderer = currentLink.AddComponent<LineRenderer>();
                    currentRenderer.material = lineMaterial;
                    currentRenderer.useWorldSpace = false;

                    currentLink.AddComponent<LinkDescriptor>();
                    currentLink.GetComponent<LinkDescriptor>().prev = potentialCodeBlock.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialCodeBlock.FirstBodyNodeObject;
                }

                currentRenderer.positionCount = 2;
                currentRenderer.SetPositions(new Vector3[] {
                    potentialCodeBlock.GetComponent<RectTransform>().localPosition + new Vector3(.0f, potentialCodeBlock.GetComponent<RectTransform>().rect.height / -2.0f),
                    potentialCodeBlock.FirstBodyNodeObject.GetComponent<RectTransform>().localPosition + new Vector3(potentialCodeBlock.FirstBodyNodeObject.GetComponent<RectTransform>().rect.width / -2.0f, 0.0f)
                });
                // This is to prevent culling from other 3D elements in the scene
                currentRenderer.gameObject.layer = LayerMask.NameToLayer("CodeEditor");
                validLinks.Add(currentLink.name);

                /*GameObject currentObject = potentialCodeBlock.FirstBodyNodeObject;
                {
                    while (currentObject.GetComponent<NodeBase>() != null && currentObject.GetComponent<NodeBase>().NextNodeObject != null)
                    {
                        currentRenderer.positionCount += 2;
                        currentRenderer.SetPosition(renderer.positionCount - 2, currentObject.GetComponent<RectTransform>().localPosition);
                        currentRenderer.SetPosition(renderer.positionCount - 1, currentObject.GetComponent<NodeBase>().NextNodeObject.GetComponent<RectTransform>().localPosition);

                        currentObject = currentObject.GetComponent<NodeBase>().NextNodeObject;
                    }
                }*/
            }
        }

        // Lastly, remove any lines that are not needed (e.g after deleting a node)
        Transform[] allLines = lineCanvas.GetComponentsInChildren<Transform>();
        foreach (Transform line in allLines)
        {
            if(!validLinks.Contains(line.name) && line.name != "LineCanvas")
            {
                Destroy(line.gameObject);
            }
        }

        FixLines();
    }

    // Introduce 90-degree angle vertices to the lines if they're not fully straight
    void FixLines()
    {
        // Straight line tolerance expressed as maximum vertical offset between two vertices of the line
        const float verticalTolerance = 10.0f;

        const float connectorPadding = 20.0f;

        LineRenderer[] lines = lineCanvas.GetComponentsInChildren<LineRenderer>();
        foreach(LineRenderer line in lines)
        {
            LinkDescriptor linkDesc = line.GetComponent<LinkDescriptor>();
            Vector2 finalPoint = line.GetPosition(1);

            if (Mathf.Abs(line.GetPosition(0).y - finalPoint.y) > verticalTolerance)
            {
                RectTransform prevTransform = linkDesc.prev.GetComponent<RectTransform>(), nextTransform = linkDesc.next.GetComponent<RectTransform>();
                Rect prevRect = prevTransform.rect, nextRect = nextTransform.rect;
                if(prevTransform.localPosition.x + prevRect.width + connectorPadding < nextTransform.localPosition.x)
                {
                    line.positionCount = 4;
                    line.SetPosition(1, new Vector2((nextTransform.localPosition.x - nextRect.width/2.0f) - connectorPadding, line.GetPosition(0).y));
                    line.SetPosition(2, new Vector2((nextTransform.localPosition.x - nextRect.width/2.0f) - connectorPadding, finalPoint.y));
                    line.SetPosition(3, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f), finalPoint.y));
                }
                else
                {
                    line.positionCount = 6;
                    line.SetPosition(1, new Vector2((prevTransform.localPosition.x + prevRect.width / 2.0f) + connectorPadding, line.GetPosition(0).y));
                    line.SetPosition(2, new Vector2((prevTransform.localPosition.x + prevRect.width / 2.0f) + connectorPadding, (prevTransform.localPosition.y + nextTransform.localPosition.y) / 2.0f));
                    line.SetPosition(3, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f) - connectorPadding, (prevTransform.localPosition.y + nextTransform.localPosition.y) / 2.0f));
                    line.SetPosition(4, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f) - connectorPadding, finalPoint.y));
                    line.SetPosition(5, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f), finalPoint.y));
                }
            }
        }
    }
}
