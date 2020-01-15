﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NodeBase : MonoBehaviour, IProgramNode
{
    // Points to the next node
    public IProgramNode nextNode;

    // GameObject that holds the nextNode
    public GameObject NextNodeObject;


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

    public void Reset()
    {
        InitialiseNode();
    }

    public virtual void InitialiseNode()
    {

    }
}
