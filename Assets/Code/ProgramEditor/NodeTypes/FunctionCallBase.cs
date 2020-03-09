using System;
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

    // Used to instantiate parameters
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
            //Debug.Log($"{name}.initHeight : {initHeight}");
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

        //if (!isInitialised)
            //InitialiseNode();
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
        // TODO: maybe enable a prompt that asks the user if they want to use the arithmetic chain as a default value for param0 or type their own?
        /*if(paramCount == 1 && prevArithmetic != null)
        {
            if(parameters.Count < 1)
            {
                parameters.Capacity = 1;
            }
            if(parameters[0] == null)
            {
                parameters[0] = new FunctionParameter();
            }
            ArithmeticOperationBase arithmetic = prevArithmetic;
            parameters[0].Value = result.ToString();
            parameters[0].Expression = arithmetic.Serialize();
            parameters[0].Type = "Int";
            parameters[0].IsReference = false;
        }*/

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
        if (parameters.Count < paramsToDestroy.Count)
        {
            foreach (Transform param in paramsToDestroy)
            {
                if (Convert.ToInt32(param.name.Replace("Parameter", "")) > parameters.Count)
                {
                    Destroy(param.gameObject);
                    GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (initHeight == 0.0f ? GetComponent<RectTransform>().rect.height : initHeight) - firstParamHeight * (parameters.Count > 2 ? (float)parameters.Count : 1.5f) - margin);
                }
            }
        }
        /*if (needResize)
        {*/
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (initHeight == 0.0f ? GetComponent<RectTransform>().rect.height : initHeight) + firstParamHeight * (parameters.Count > 2 ? (float)parameters.Count : 1.5f) + margin);
            //needResize = false;
        //}

        for (ushort i = 0; i < paramCount && parameters != null && i < parameters.Count; i++)
        {
            // TODO: type checking, pointing references to scene objects, etc.
            Transform param = transform.Find($"Parameter{i + 1}");
            if (param == null)
            {
                GameObject paramObject = Instantiate(ParameterTemplate, transform);
                paramObject.SetActive(true);

                //paramObject.transform.localPosition = new Vector3(0.0f, (functionNameText.GetComponentInParent<RectTransform>().rect.yMin * -i + margin));
                // some really weird maths?
                float height = paramObject.GetComponent<RectTransform>().rect.height;
                float origin = parameters.Count > 0 ? ((parameters.Count / 2.0f - parameters.Count) + i) * height : 0.0f;
                paramObject.transform.localPosition = new Vector3(0.0f, origin);

                paramObject.name = $"Parameter{i+1}";

                param = paramObject.transform;

                if (computer != null && computer.symbolTable != null)
                {
                    ref Dictionary<string, FunctionParameter> symTable = ref computer.symbolTable;
                    string resultStr = ArithmeticOperationBase.GetResult(parameters[i].Value, ref symTable);
                    //double result = string.IsNullOrWhiteSpace(resultStr) ? 0 : double.Parse(resultStr);
                }
            }
            // Name is the parameter name as defined by the function. Not a variable name. If we haven't defined the parameter name, don't show the = character.
            param.GetComponentInChildren<Text>().text = $"{parameters[i].Name}{(!string.IsNullOrWhiteSpace(parameters[i].Name) ? "=" : "")}{(string.IsNullOrWhiteSpace(parameters[i].Expression) ? parameters[i].Value : parameters[i].Expression)}";
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
            Debug.LogError("Missing function name!");
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
        Debug.Log(GetFunctionName());
        return $"{GetFunctionName()}({GetParameterListString()})";
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

            if (!string.IsNullOrWhiteSpace(indexName))
                value += $"[{indexName}]";

            bool isString = parameter.IsReference ? (symbolTable.ContainsKey(value) && symbolTable[value].Value.Trim().StartsWith("\"") && symbolTable[value].Value.Trim().EndsWith("\"")) : (value.Trim().StartsWith("\"") && value.Trim().EndsWith("\""));
            bool isReference = parameter.IsReference ? true : !isString && symbolTable.ContainsKey(value);

            Debug.Log($"{functionName}.GetRawParameters: {parameter.Value}");

            string mathResult = ArithmeticOperationBase.GetResult(value, ref symbolTable);
            Debug.Log($"mathResult: {mathResult}, paramVal: {value}");
            bool isMath = mathResult != value;

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
            if(isString)
            {
                Debug.Log("Passing string!");
                ret.Add(isReference ? symbolTable[value].Value : value.Trim().Substring(1, value.Trim().Length - 2));
            }
            else if(isMath)
            {
                Debug.Log("Passing arithmetic expression!");
                ret.Add(ArithmeticOperationBase.GetResult(value, ref symbolTable));
            }
            else
            {
                if(isReference)
                {
                    Debug.Log("Passing variable!");
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
                    Debug.Log("Passing bool/number literal!");
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
            Debug.Log($"Passed {(ret.Count > 0 ? ret[ret.Count - 1] : "nothing")}");
        }

        return ret.ToArray();
    }
}
