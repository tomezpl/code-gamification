using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Represents a piece of data - either a variable name or a literal
public class Data : NodeBase
{
    public FunctionParameter descriptor;

    public override void InitialiseNode()
    {
        base.InitialiseNode();

        if (descriptor == null)
            descriptor = new FunctionParameter();

        transform.Find("Text").GetComponent<Text>().text = descriptor.IsReference ? descriptor.Name : descriptor.Value;
    }

    public override string Serialize()
    {
        return descriptor.IsReference ? descriptor.Name : descriptor.Value;
    }
}
