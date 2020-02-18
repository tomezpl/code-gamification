using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArithmeticOperationBase : FunctionCallBase
{
    public FunctionParameter leftHand, rightHand;

    public string operatorStr;
    public string operatorName;

    public GameObject operatorText;

    public override void InitialiseNode()
    {

        bool wasInitialised = isInitialised;

        functionNamePrefix = "arithmetic";

        base.InitialiseNode();

        if (parameters == null || parameters.Count < 2)
        {
            if (parameters != null && parameters.Count == 1)
                parameters = new List<FunctionParameter> { parameters[0], new FunctionParameter() };
            else if (parameters == null || parameters.Count == 0)
                parameters = new List<FunctionParameter> { new FunctionParameter(), new FunctionParameter() };
            else
            {
                Debug.LogWarning($"{this}: {functionName} has invalid param count!");
            }
            paramCount = 2;
        }

        leftHand = parameters[0];
        rightHand = parameters[1];

        UpdateFunctionProperties();
    }

    public override void UpdateFunctionProperties()
    {
        if (!string.IsNullOrWhiteSpace(functionNamePrefix))
        {
            functionName = $"{functionNamePrefix}{(string.IsNullOrWhiteSpace(operatorName) ? "" : $": {operatorName}")}";
        }
        else
        {
            functionName = operatorName;
        }

        base.UpdateFunctionProperties();

        if (operatorText == null)
        {
            operatorText = transform.Find("Operator").transform.Find("Text").gameObject;
        }
        operatorText.GetComponent<Text>().text = operatorStr;
    }

    public override string Serialize()
    {
        return $"{leftHand.Value} {operatorStr} {rightHand.Value}";
    }
}
