using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllocateArray : FunctionCallBase
{
    public override void InitialiseNode()
    {
        paramCount = 2;
        parameters = new List<FunctionParameter> { new FunctionParameter { Name = "size", Type = "Int" }, new FunctionParameter { Name = "name", Type = "String" } };

        functionName = "create list";

        base.InitialiseNode();
    }

    public override string Serialize()
    {
        return $"{parameters[1].Value} = [None] * {parameters[0].Value}";
    }
}
