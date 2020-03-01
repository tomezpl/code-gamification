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

    // Start is called before the first frame update
    public override void InitialiseNode()
    {
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

            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetComponent<RectTransform>().rect.height - firstParamRect.height);
        }

        if(parameters == null)
        {
            parameters = new List<FunctionParameter>();
            for(int i = 0; i < paramCount; i++)
            {
                parameters.Add(new FunctionParameter());
            }
        }
        UpdateFunctionProperties();
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
        // TODO: maybe enable a prompt that asks the user if they want to use the arithmetic chain as a default value for param0 or type their own?
        if(paramCount == 1 && prevArithmetic != null)
        {
            if(parameters.Count < 1)
            {
                parameters.Capacity = 1;
            }
            if(parameters[0] == null)
            {
                parameters[0] = new FunctionParameter();
            }
            // TODO: use variable instead of getcomponent
            parameters[0].Value = prevArithmetic.GetComponent<ArithmeticOperationBase>().GetResult(ref computer.symbolTable).ToString();
            parameters[0].Expression = prevArithmetic.GetComponent<ArithmeticOperationBase>().Serialize();
            parameters[0].Type = "Int";
            parameters[0].IsReference = false;
        }

        if (functionNameText == null)
        {
            functionNameText = transform.Find("FuncName").transform.Find("Text").gameObject;
        }
        functionNameText.GetComponent<Text>().text = functionName;

        float margin = Mathf.Abs(firstParamOrigin.y - functionNameText.GetComponentInParent<RectTransform>().localPosition.y);
        if (needResize)
        {
            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetComponent<RectTransform>().rect.height + firstParamHeight * (parameters.Count > 2 ? (float)parameters.Count : 1.5f) + margin);
            needResize = false;
        }

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
            }
            // Name is the parameter name as defined by the function. Not a variable name. If we haven't defined the parameter name, don't show the = character.
            param.GetComponentInChildren<Text>().text = $"{parameters[i].Name}{(!string.IsNullOrWhiteSpace(parameters[i].Name) ? "=" : "")}{parameters[i].Value}";
        }
    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();
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
        Debug.Log(GetFunctionName());
        return $"{GetFunctionName()}({GetParameterListString()})";
    }

    public object[] GetRawParameters(Dictionary<string, FunctionParameter> symbolTable)
    {
        List<object> ret = new List<object>();

        foreach(FunctionParameter parameter in parameters)
        {
            string type = parameter.Type == null ? "" : parameter.Type.ToLower();

            bool isString = parameter.IsReference ? (symbolTable.ContainsKey(parameter.Value) && symbolTable[parameter.Value].Value.Trim().StartsWith("\"") && symbolTable[parameter.Value].Value.Trim().EndsWith("\"")) : (parameter.Value.Trim().StartsWith("\"") && parameter.Value.Trim().EndsWith("\""));
            bool isReference = parameter.IsReference ? true : !isString && symbolTable.ContainsKey(parameter.Value);

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
                ret.Add(isReference ? symbolTable[parameter.Value].Value : parameter.Value.Trim().Substring(1, parameter.Value.Trim().Length - 2));
            }
            else
            {
                if(isReference)
                {
                    if(parameter.Value.Trim() == "None")
                    {
                        ret.Add(null);
                    }
                    else if(symbolTable[parameter.Value.Trim()].Type.ToLower().Contains("bool"))
                    {
                        ret.Add(bool.Parse(symbolTable[parameter.Value.Trim()].Value.ToLower()));
                    }
                    else if(symbolTable[parameter.Value.Trim()].Type.ToLower().Contains("int") || symbolTable[parameter.Value.Trim()].Type.ToLower().Contains("num"))
                    {
                        ret.Add(double.Parse(symbolTable[parameter.Value.Trim()].Value));
                    }
                }
                else
                {
                    if(parameter.Value.Trim() == "True" || parameter.Value.Trim() == "False")
                    {
                        ret.Add(bool.Parse(parameter.Value.Trim().ToLower()));
                    }
                }
            }
        }

        return ret.ToArray();
    }
}
