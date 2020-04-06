using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for blocks that wrap/nest code (if statements, loops, function definitions)
public abstract class CodeBlock : NodeBase
{
    // Only set to true before InitialiseNode gets called. 
    // Any consequent calls to InitialiseNode should proceed with this set to false,
    // to prevent incorrect indentation levels.
    public bool needToIndent = true;

    public GameObject FirstBodyNodeObject;

    public IProgramNode firstBodyNode;

    public override void InitialiseNode()
    {
        base.InitialiseNode();

        if (FirstBodyNodeObject != null)
            firstBodyNode = FirstBodyNodeObject.GetComponent<NodeBase>();
    }

    public virtual string GetBodyLineTabs()
    {
        return GetLineTabs() + "\t";
    }

    public virtual string SerializeBlockBody()
    {
        string fullBody = "";
        IProgramNode currentNode = firstBodyNode;
        while(currentNode != null)
        {
            fullBody += $"{currentNode.Serialize()}\n";
            // TODO: could this fail potentially?
            currentNode = ((NodeBase)currentNode).nextNode;
        }
        return fullBody;
    }

    public virtual string SerializeBlockHeader()
    {
        return null;
    }

    public override string Serialize()
    {
        string header = SerializeBlockHeader();
        string[] bodyLines = SerializeBlockBody().Split('\n');
        string fullCode = $"{(string.IsNullOrWhiteSpace(header) ? "" : GetLineTabs() + header + "\n")}";

        string bodyLineTabs = GetBodyLineTabs();

        foreach(string line in bodyLines)
        {
            fullCode += $"{bodyLineTabs}{line}\n";
        }

        return fullCode.Substring(0, fullCode.Length-1);
    }
}
