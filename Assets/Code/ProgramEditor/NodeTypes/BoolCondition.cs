using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class BoolCondition
{
    [SerializeField]
    public FunctionParameter leftHand;
    [SerializeField]
    public FunctionParameter rightHand;

    [SerializeField]
    // Comparison operator, like equality, greater, greater-or-equal, etc.
    public string comparison;

    // Returns the result of the conditional at this frame
    public bool Evaluate(ref Dictionary<string, FunctionParameter> symbolTable)
    {
        bool ret = false;

        bool isLeftString = leftHand.IsReference ? (symbolTable.ContainsKey(leftHand.Value) && symbolTable[leftHand.Value].Value.Trim().StartsWith("\"") && symbolTable[leftHand.Value].Value.Trim().EndsWith("\"")) : (leftHand.Value.Trim().StartsWith("\"") && leftHand.Value.Trim().EndsWith("\""));
        bool isLeftReference = leftHand.IsReference ? true : !isLeftString && symbolTable.ContainsKey(leftHand.Value);

        bool isRightString = rightHand.IsReference ? (symbolTable.ContainsKey(rightHand.Value) && symbolTable[rightHand.Value].Value.Trim().StartsWith("\"") && symbolTable[rightHand.Value].Value.Trim().EndsWith("\"")) : (rightHand.Value.Trim().StartsWith("\"") && rightHand.Value.Trim().EndsWith("\""));
        bool isRightReference = rightHand.IsReference ? true : !isRightString && symbolTable.ContainsKey(rightHand.Value);

        string leftValue = isLeftReference ? symbolTable[leftHand.Value].Value : (isLeftString ? leftHand.Value.Trim().Substring(1, leftHand.Value.Trim().Length-2) : leftHand.Value.Trim());
        string rightValue = isRightReference ? symbolTable[rightHand.Value].Value : (isRightString ? rightHand.Value.Trim().Substring(1, rightHand.Value.Trim().Length - 2) : rightHand.Value.Trim());

        // failsafe for missing/invalid values
        if (string.IsNullOrWhiteSpace(leftValue) || string.IsNullOrWhiteSpace(rightValue))
        {
            Debug.LogWarning("BoolCondition.Evaluate: invalid values!");
            return false;
        }

        double numL, numR = numL = 0;
        if (comparison == "==")
        {
            ret = leftValue == rightValue;
        }
        else if (comparison == "!=")
        {
            ret = leftValue != rightValue;
        }
        else if(comparison == ">=")
        {
            if(double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                ret = numL >= numR;
            }
        }
        else if (comparison == "<=")
        {
            if (double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                ret = numL <= numR;
            }
        }
        else if (comparison == ">")
        {
            if (double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                ret = numL > numR;
            }
        }
        else if (comparison == "<")
        {
            if (double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                ret = numL < numR;
            }
        }

        Debug.Log($"BoolCondition.Evaluate: Finished with result {ret.ToString().ToUpper()}. \nEvaluated '{leftHand.Value} {comparison} {rightHand.Value}' as '{leftValue} {comparison} {rightValue}'. \nNumerical: {numL} {comparison} {numR}");

        return ret;
    }
}