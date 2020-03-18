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

        string lh = ArithmeticOperationBase.GetResult(leftHand.Value, ref symbolTable);
        string rh = ArithmeticOperationBase.GetResult(rightHand.Value, ref symbolTable);

        bool isLeftString = leftHand.IsReference ? (symbolTable.ContainsKey(rh) && symbolTable[lh].Value.Trim().StartsWith("\"") && symbolTable[lh].Value.Trim().EndsWith("\"")) : (lh.Trim().StartsWith("\"") && lh.Trim().EndsWith("\""));
        bool isLeftReference = leftHand.IsReference ? true : !isLeftString && symbolTable.ContainsKey(lh);

        bool isRightString = rightHand.IsReference ? (symbolTable.ContainsKey(rh) && symbolTable[rh].Value.Trim().StartsWith("\"") && symbolTable[rh].Value.Trim().EndsWith("\"")) : (rh.Trim().StartsWith("\"") && rh.Trim().EndsWith("\""));
        bool isRightReference = rightHand.IsReference ? true : !isRightString && symbolTable.ContainsKey(rh);

        string leftValue = isLeftReference ? symbolTable[lh].Value : (isLeftString ? lh.Trim().Substring(1, lh.Trim().Length-2) : lh.Trim());
        string rightValue = isRightReference ? symbolTable[rh].Value : (isRightString ? rh.Trim().Substring(1, rh.Trim().Length - 2) : rh.Trim());

        // failsafe for missing/invalid values
        if (string.IsNullOrWhiteSpace(leftValue) || string.IsNullOrWhiteSpace(rightValue))
        {
            Logger.LogWarning("BoolCondition.Evaluate: invalid values!");
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

        Logger.Log($"BoolCondition.Evaluate: Finished with result {ret.ToString().ToUpper()}. \nEvaluated '{leftHand.Value} {comparison} {rightHand.Value}' as '{leftValue} {comparison} {rightValue}'. \nNumerical: {numL} {comparison} {numR}");

        return ret;
    }
}