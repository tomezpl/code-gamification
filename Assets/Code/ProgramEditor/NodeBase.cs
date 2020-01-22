using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NodeBase : MonoBehaviour, IProgramNode
{
    // Points to the next node
    public IProgramNode nextNode;

    // GameObject that holds the nextNode
    public GameObject NextNodeObject;

    // Python syntax-specific: indentation level (default: 0, increases with nested CodeBlocks)
    public int indentLevel = 0;

    public bool isInitialised = false;

    public abstract string Serialize();

    public bool inLoop = false;

    public virtual void Start()
    {
        // Unless overriden, not null and not in a loop, assign nextNode as node interface of the NextNodeObject
        if(nextNode == null && !inLoop && NextNodeObject != null)
        {
            NodeBase nextNodeBase = NextNodeObject.GetComponent<NodeBase>();
            nextNode = (IProgramNode)nextNodeBase;
        }
        if(NextNodeObject == null)
        {
            Debug.LogWarning($"NextNodeObject on {gameObject}::{this} is set to null! If this is intended (e.g. last node in a loop), this should be corrected on startup.");
        }

        // TODO: Perhaps search for ProgramEnd as nextNode fallback?

        // TODO: only initialise if !isInitialised?
        InitialiseNode();
    }

    public void Awake()
    {
        Start();
    }

    public virtual void Reset()
    {
        isInitialised = false;
        InitialiseNode();
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

        Debug.Log($"{this} returned {indentLevel} lineTabs.");

        return lineTabs;
    }
}
