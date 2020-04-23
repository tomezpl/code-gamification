using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherits from ArithmeticOperationBase because it's easier to program the UI this way. 
// Be careful about order of types with regards to inheritance in ProgramController.CheckNodeTypes() though - this needs to appear before the parent class.
public class AssignValue : ArithmeticOperationBase
{
    public override void InitialiseNode()
    {
        operatorStr = "=";
        operatorName = "set variable";

        base.InitialiseNode();
        functionNamePrefix = "";
        UpdateFunctionProperties();
    }

    public override void Update()
    {
        base.Update();

        if (prevArithmetic)
            rightHand.Value = prevArithmetic.Serialize();
    }

    public override string Serialize()
    {
        return $"{leftHand.Value} = {(prevArithmetic ? prevArithmetic.Serialize() : rightHand.Value)}";
    }
}
