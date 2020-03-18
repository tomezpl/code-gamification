using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArithmeticOperationBase : FunctionCallBase
{
    public FunctionParameter leftHand, rightHand;

    public string operatorStr;
    public string operatorName;

    public GameObject operatorText;

    public bool wrap = false;

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
                Logger.LogWarning($"{this}: {functionName} has invalid param count!");
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

        if (prevArithmetic)
        {
            // is this NOT an assignvalue? (e.g. if this is part of the arithmetic chain)
            // if so, put the values from the chain on the left side
            if (!GetComponent<AssignValue>())
            {
                leftHand.Value = prevArithmetic.Serialize();
            }
            // otherwise, put it on the right (left hand would be the symbol name in the assignvalue)
            else
            {
                rightHand.Value = prevArithmetic.Serialize();
            }
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
        return $"{(wrap ? "(" : "")}{(prevArithmetic ? prevArithmetic.Serialize() : leftHand.Value)} {operatorStr} {rightHand.Value}{(wrap ? ")" : "")}";
    }

    public static string GetResult(string expr, ref Dictionary<string, FunctionParameter> symbolTable)
    {
        // Check if there are even arithmetic operations present in the expressions
        // TODO: this might pick up strings as well
        if(string.IsNullOrWhiteSpace(expr))
        {
            return "";
        }

        if (!(expr.Contains("+") || expr.Contains("-") || expr.Contains("%") || expr.Contains("/") || expr.Contains("*")))
            return expr;

        foreach(string symbol in symbolTable.Keys)
        {
            if(expr.Contains(symbol))
            {
                expr = expr.Replace(symbol, symbolTable[symbol].Value);
            }
        }

        try
        {
            return System.Convert.ToDouble(new System.Data.DataTable().Compute(expr, null)).ToString();
        } catch(System.Exception)
        {
            return "";
        }
    }
}
