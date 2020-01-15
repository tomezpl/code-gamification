using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FunctionCallBase : NodeBase
{
    public string functionName;
    public ushort paramCount; // number of parameters
    [SerializeField]
    public List<FunctionParameter> parameters;

    // UI stuff:
    public GameObject functionNameText;

    // Start is called before the first frame update
    void Start()
    {
        functionNameText.GetComponent<Text>().text = functionName;
        for(ushort i = 0; i < paramCount; i++)
        {
            // TODO: type checking, pointing references to scene objects, etc.
            transform.Find($"Parameter{i + 1}").GetComponentInChildren<Text>().text = parameters[i].Value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual string GetFunctionName()
    {
        if (functionName != null)
        {
            return functionName;
        }
        else
        {
            Debug.LogError("Missing function name!");
            return "";
        }
    }

    // Returns list of comma-separated parameters to be passed in the function call
    public virtual List<string> GetParameterList()
    {
        return parameters.ConvertAll<string>(p => parameters[parameters.Count - 1] == p ? $"{p.Value}" : $"{p.Value}, ");
    }

    // Get results from GetParameterList and parse them as a single string to be added between parentheses of a function call.
    public virtual string GetParameterListString()
    {
        List<string> paramList = GetParameterList();
        string paramListStr = "";

        foreach(string p in paramList)
        {
            paramListStr += p;
        }
        
        return paramListStr;
    }

    public override string Serialize()
    {
        return $"{GetFunctionName()}({GetParameterListString()})";
    }
}
