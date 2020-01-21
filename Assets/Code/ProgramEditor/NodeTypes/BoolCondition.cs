using System;
using UnityEngine;

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
    public bool Evaluate(GameObject caller)
    {
        // TODO
        return true;
    }
}