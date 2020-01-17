﻿using System.Collections;
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

    protected bool isInitialised = false;

    public abstract string Serialize();

    public void Start()
    {
        // Unless overriden and not null, assign nextNode as node interface of the NextNodeObject
        if(nextNode == null)
        {
            NodeBase nextNodeBase = NextNodeObject.GetComponent<NodeBase>();
            nextNode = (IProgramNode)nextNodeBase;
        }

        InitialiseNode();

        // TODO: Perhaps search for ProgramEnd as nextNode fallback?
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

        return lineTabs;
    }
}
