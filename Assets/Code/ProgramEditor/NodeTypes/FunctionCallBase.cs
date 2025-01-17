﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FunctionCallBase : NodeBase
{
    public string functionName;
    public string functionNamePrefix;
    public ushort paramCount; // number of parameters
    [SerializeField]
    public List<FunctionParameter> parameters;

    // UI stuff:
    public GameObject functionNameText;
    protected Vector2 firstParamOrigin;
    protected Rect firstParamRect;
    protected float firstParamWidth, firstParamHeight;

    // Used to instantiate parameters in function call UI on runtime
    static GameObject ParameterTemplate;

    // Used for arithmetic operation chain, to feed arithmetic expressions & evaluated results into assignValue righthands/functionCall params
    public ArithmeticOperationBase prevArithmetic;

    // Should only be set to true when changing function
    public bool needResize = true;

    private float initHeight = 0.0f;

    // Start is called before the first frame update
    public override void InitialiseNode()
    {
        bool wasInitialised = isInitialised;
        base.InitialiseNode();

        // Create a ParameterTemplate, which can then be instantiated for each parameter added to the function call in the UI.
        // TODO: performance fixes, this can be optimised by storing Parameter in a variable
        if (transform.Find("Parameter"))
        {
            firstParamOrigin = transform.Find("Parameter").localPosition;
            firstParamRect = transform.Find("Parameter").GetComponent<RectTransform>().rect;
            firstParamWidth = transform.Find("Parameter").GetComponent<RectTransform>().sizeDelta.x;
            firstParamHeight = transform.Find("Parameter").GetComponent<RectTransform>().sizeDelta.y;
            transform.Find("Parameter").gameObject.SetActive(false);
            if(ParameterTemplate == null)
            {
                ParameterTemplate = transform.Find("Parameter").gameObject;
                ParameterTemplate.transform.SetParent(FindElementContainer());
            }
            else
            {
                Destroy(transform.Find("Parameter").gameObject);
            }
            if (initHeight == 0.0f)
            {
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetComponent<RectTransform>().rect.height - firstParamRect.height);
                initHeight = GetComponent<RectTransform>().rect.height;
            }
            //Logger.Log($"{name}.initHeight : {initHeight}");
        }

        if (parameters == null)
        {
            parameters = new List<FunctionParameter>();
            for(int i = 0; i < paramCount; i++)
            {
                parameters.Add(new FunctionParameter());
            }
        }
        //UpdateFunctionProperties();
    }

    public override void Start()
    {
        base.Start();

        UpdateFunctionProperties();
    }

    public void OnValidate()
    {
        //UpdateFunctionProperties();
    }

    public override void Reset()
    {
        base.Reset();
        //UpdateFunctionProperties();
    }

    public virtual void UpdateFunctionProperties()
    {

        if (functionNameText == null)
        {
            functionNameText = transform.Find("FuncName").transform.Find("Text").gameObject;
        }
        functionNameText.GetComponent<Text>().text = functionName;

        float margin = Mathf.Abs(firstParamOrigin.y - functionNameText.GetComponentInParent<RectTransform>().localPosition.y);

        // Remove any parameter UI objects
        List<Transform> paramsToDestroy = new List<Transform>();
        foreach(Transform child in GetComponentsInChildren<Transform>())
        {
            if(child.name.StartsWith("Parameter"))
            {
                paramsToDestroy.Add(child);
            }
        }

        foreach (Transform param in paramsToDestroy)
        {
            if (Convert.ToInt32(param.name.Replace("Parameter", "")) > parameters.Count)
            {
                Destroy(param.gameObject);
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (initHeight == 0.0f ? GetComponent<RectTransform>().rect.height : initHeight) - firstParamHeight * (parameters.Count > 2 ? (float)parameters.Count : 1.5f) - margin);
            }
        }
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (initHeight == 0.0f ? GetComponent<RectTransform>().rect.height : initHeight) + firstParamHeight * (parameters.Count > 2 ? (float)parameters.Count : 1.5f) + margin);

        for (ushort i = 0; i < paramCount && parameters != null && i < parameters.Count; i++)
        {
            int paramIdx = (parameters.Count - 1) - i;

            // TODO: type checking, pointing references to scene objects, etc.
            Transform param = transform.Find($"Parameter{paramIdx + 1}");

            if (param == null)
            {
                GameObject paramObject = Instantiate(ParameterTemplate, transform);
                paramObject.SetActive(true);

                // some really weird maths? it works for arranging the parameter objects in the UI though...
                float height = paramObject.GetComponent<RectTransform>().rect.height;
                float origin = parameters.Count > 0 ? ((parameters.Count / 2.0f - parameters.Count) + i) * height : 0.0f;
                paramObject.transform.localPosition = new Vector3(0.0f, origin);

                paramObject.name = $"Parameter{paramIdx+1}";

                if(paramObject.GetComponent<EditorDraggableNode>())
                {
                    paramObject.GetComponent<EditorDraggableNode>().enabled = true;
                }

                param = paramObject.transform;

                if (computer != null && computer.symbolTable != null)
                {
                    ref Dictionary<string, FunctionParameter> symTable = ref computer.symbolTable;
                    string resultStr = ArithmeticOperationBase.GetResult(parameters[paramIdx].Value, ref symTable);
                    //double result = string.IsNullOrWhiteSpace(resultStr) ? 0 : double.Parse(resultStr);
                }
            }

            // Name is the parameter name as defined by the function. Not a variable name. If we haven't defined the parameter name, don't show the = character.
            param.GetComponentInChildren<Text>().text = $"{parameters[paramIdx].Name}{(!string.IsNullOrWhiteSpace(parameters[paramIdx].Name) ? "=" : "")}{(string.IsNullOrWhiteSpace(parameters[paramIdx].Expression) ? parameters[paramIdx].Value : parameters[paramIdx].Expression)}";
        }
    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if(prevArithmetic != null && prevArithmetic.NextNodeObject != gameObject)
        {
            prevArithmetic = null;
        }
    }

    public virtual string GetFunctionName()
    {
        if (functionName != null)
        {
            return functionName;
        }
        else
        {
            Logger.LogError("Missing function name!");
            return "";
        }
    }

    // Returns list of comma-separated parameters to be passed in the function call
    public virtual List<string> GetParameterList()
    {
        return parameters.ConvertAll<string>(p => parameters[parameters.Count - 1] == p ? $"{(string.IsNullOrWhiteSpace(p.Expression) ? p.Value : p.Expression)}" : $"{(string.IsNullOrWhiteSpace(p.Expression) ? p.Value : p.Expression)}, ");
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
        //Logger.Log(GetFunctionName());
        // Special case for printNewline. That function doesn't actually exist in Python but is here for convenience's sake. 
        // We want it to be serialized to print("\n")
        if (functionName != "printNewline")
        {
            return $"{GetFunctionName()}({GetParameterListString()})";
        }
        else if(functionName == "sleep")
        {
            double tickTime = 1.0;
            if(computer != null)
            {
                tickTime = computer.tickTime;
            }
            return $"time.sleep({tickTime})";
        }
        else
        {
            return "print(\"\\n\")";
        }
    }

    public object[] GetRawParameters(Dictionary<string, FunctionParameter> symbolTable)
    {
        List<object> ret = new List<object>();

        foreach(FunctionParameter parameter in parameters)
        {
            string type = parameter.Type == null ? "" : parameter.Type.ToLower();

            string value = parameter.Value;

            // Is it an array index?
            string indexName = "";
            string[] indexSplit = parameter.Value.Split('[');
            if (indexSplit != null && indexSplit.Length == 2)
            {
                value = indexSplit[0];
                indexName = indexSplit[1].Substring(0, indexSplit[1].Length - 1);
                indexName = ArithmeticOperationBase.GetResult(indexName, ref symbolTable);
            }
            Logger.Log($"indexName = {indexName}");

            if (!string.IsNullOrWhiteSpace(indexName))
                value += $"[{indexName}]";

            bool isString = parameter.IsReference ? (symbolTable.ContainsKey(value) && symbolTable[value].Value.Trim().StartsWith("\"") && symbolTable[value].Value.Trim().EndsWith("\"")) : (value.Trim().StartsWith("\"") && value.Trim().EndsWith("\""));
            bool isReference = parameter.IsReference ? true : !isString && symbolTable.ContainsKey(value);

            Logger.Log($"isString = {isString}");
            Logger.Log($"isReference = {isReference}");

            Logger.Log($"{functionName}.GetRawParameters: {parameter.Value}");

            string mathResult = ArithmeticOperationBase.GetResult(value, ref symbolTable);
            Logger.Log($"mathResult: {mathResult}, paramVal: {value}");
            bool isMath = mathResult != value;
            Logger.Log($"isMath = {isMath}");

            // Types are usually declared on literals...
            /*if (type.Contains("num") || type.Contains("int") || type.Contains("double"))
            {
                ret.Add(Convert.ToInt32(parameter.Value));
            }
            else if(type.Contains("str"))
            {
                ret.Add(parameter.Value);
            }
            else if(type.Contains("bool"))
            {
                ret.Add(Convert.ToBoolean(parameter.Value.ToLower()));
            }*/
            if (isString)
            {
                Logger.Log("Passing string!");
                ret.Add(isReference ? symbolTable[value].Value : value.Trim().Substring(1, value.Trim().Length - 2));
            }
            else if(isMath)
            {
                Logger.Log("Passing arithmetic expression!");
                ret.Add(ArithmeticOperationBase.GetResult(value, ref symbolTable));
            }
            else
            {
                if(isReference)
                {
                    Logger.Log("Passing variable!");
                    if (value.Trim() == "None")
                    {
                        ret.Add(null);
                    }
                    else if(symbolTable[value.Trim()].Type.ToLower().Contains("bool"))
                    {
                        ret.Add(symbolTable[value.Trim()].Value.ToLower());
                    }
                    else if(symbolTable[value.Trim()].Type.ToLower().Contains("int") || symbolTable[value.Trim()].Type.ToLower().Contains("num"))
                    {
                        ret.Add(symbolTable[value.Trim()].Value);
                    }
                    else
                    {
                        ret.Add(symbolTable[value.Trim()].Value);
                    }
                }
                else
                {
                    Logger.Log("Passing bool/number literal!");
                    double number = 0.0;
                    if(value.Trim() == "True" || value.Trim() == "False")
                    {
                        ret.Add(value.Trim().ToLower());
                    }
                    else if(double.TryParse(value.Trim(), out number))
                    {
                        ret.Add(value.Trim());
                    }
                    else
                    {
                        ret.Add("");
                    }
                }
            }
            Logger.Log($"Passed {(ret.Count > 0 ? ret[ret.Count - 1] : "nothing")}");
        }

        return ret.ToArray();
    }
}
