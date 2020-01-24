using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArithmeticAdd : ArithmeticOperationBase
{
    public override void InitialiseNode()
    {
        operatorStr = "+";
        operatorName = "add";

        base.InitialiseNode();
    }
}
