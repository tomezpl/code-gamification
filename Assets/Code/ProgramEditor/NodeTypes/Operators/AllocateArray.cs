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

    public override void UpdateFunctionProperties()
    {
        base.UpdateFunctionProperties();

        int listSize = parameters.Count - 2;
        int newListSize;

        if(!int.TryParse(parameters[0].Value, out newListSize))
        {
            newListSize = -1;
        }

        Logger.Log($"listSize={listSize}, newListSize={newListSize}");

        if (newListSize != listSize && newListSize > 0)
        {
            while(parameters.Count > newListSize + 2)
            {
                parameters.RemoveAt(parameters.Count - 1);
            }

            for (int i = listSize; i < newListSize; i++)
            {
                parameters.Add(new FunctionParameter
                {
                    Name = "",
                    Value = "",
                    Expression = ""
                });
            }

            paramCount = (ushort)parameters.Count;
        }
        else if(newListSize == 0)
        {
            while(parameters.Count > 2)
            {
                parameters.RemoveAt(parameters.Count - 1);
            }

            paramCount = (ushort)parameters.Count;
        }

        for(int i = 2; i < parameters.Count; i++)
        {
            parameters[i].Name = $"{(string.IsNullOrWhiteSpace(parameters[1].Value) ? "" : parameters[1].Value)}[{i-2}]";
        }

        base.UpdateFunctionProperties();
    }
}
