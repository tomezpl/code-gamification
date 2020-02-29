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
        string leftValue = leftHand.IsReference ? (symbolTable.ContainsKey(leftHand.Value) ? symbolTable[leftHand.Value].Value : null) : leftHand.Value;
        string rightValue = rightHand.IsReference ? (symbolTable.ContainsKey(rightHand.Value) ? symbolTable[rightHand.Value].Value : null) : rightHand.Value;

        // failsafe for missing/invalid values
        if(string.IsNullOrWhiteSpace(leftValue) || string.IsNullOrWhiteSpace(rightValue))
        {
            return false;
        }

        double numL, numR = numL = 0;
        if (comparison == "==")
        {
            return leftValue == rightValue;
        }
        else if (comparison == "!=")
        {
            return leftValue != rightValue;
        }
        else if(comparison == ">=")
        {
            if(double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                return numL >= numR;
            }
        }
        else if (comparison == "<=")
        {
            if (double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                return numL <= numR;
            }
        }
        else if (comparison == ">")
        {
            if (double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                return numL > numR;
            }
        }
        else if (comparison == "<")
        {
            if (double.TryParse(leftValue, out numL) && double.TryParse(rightValue, out numR))
            {
                return numL < numR;
            }
        }
        return false;
    }
}