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
        int count = -1;
        if (!int.TryParse(parameters[0].Value, out count) || count < 0)
        {
            return $"{parameters[1].Value} = []";
        }
        else
        {
            return $"{parameters[1].Value} = [None] * {count}";
        }
    }
}
