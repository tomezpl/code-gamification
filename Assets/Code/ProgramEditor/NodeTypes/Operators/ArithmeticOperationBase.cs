﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This class used to be for the arithmetic chain concept, which is now deleted.
// However, it provides the GetResult method, which is still used for arithmetic and symbol table lookups.
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

    // Evaluate a mathematical expression (or just replace the symbols with their current values)
    public static string GetResult(string expr, ref Dictionary<string, FunctionParameter> symbolTable)
    {
        Logger.Log($"Arithmetic.GetResult evaluating {expr}");

        // Check if there are even arithmetic operations present in the expressions
        // TODO: this might pick up strings as well
        if(string.IsNullOrWhiteSpace(expr))
        {
            return "";
        }

        // If no arithmetic needs to be done, just return the value
        if (!(expr.Contains("+") || expr.Contains("-") || expr.Contains("%") || expr.Contains("/") || expr.Contains("*")) || (expr.Trim().StartsWith("\"") && expr.Trim().EndsWith("\"")))
        {
            if (symbolTable.ContainsKey(expr))
            {
                return symbolTable[expr].Value;
            }
            else
            {
                return expr;
            }
        }

        // Separate each operator with spaces
        foreach (char op in new char[]{ '+', '-', '%', '/', '*'})
        {
            expr = expr.Replace($"{op}", $" {op} ");
        }

        // Look up symbol value if expr is a name
        foreach (string symbol in symbolTable.Keys)
        {
            List<string> exprSplit = new List<string>(expr.Split(new char[] { ' ', '+', '-', '%', '/', '*' }));
            if (exprSplit != null && exprSplit.Contains(symbol))
            {
                string oldExpr = expr;
                expr = expr.Replace($" {symbol} ", $" {symbolTable[symbol].Value} ");
                if(expr == oldExpr)
                {
                    oldExpr = expr;
                    expr = expr.Replace($"{symbol} ", $"{symbolTable[symbol].Value} ");
                }
                if (expr == oldExpr)
                {
                    oldExpr = expr;
                    expr = expr.Replace($" {symbol}", $" {symbolTable[symbol].Value}");
                }
            }
            else if(expr.Trim() == symbol)
            {
                expr = symbolTable[symbol].Value;
            }
        }

        Logger.Log($"Arithmetic.GetResult expr reformatted as {expr}");

        if (expr.Trim() == "None")
        {
            return "None";
        }

        try
        {
            return Convert.ToDouble(new System.Data.DataTable().Compute(expr.Trim(), null)).ToString();
        } catch(Exception ex)
        {
            Logger.LogError($"Arithmetic.GetResult threw an exception: {ex.ToString()}");
            return "";
        }
    }
}
