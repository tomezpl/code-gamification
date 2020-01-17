using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArithmeticAdd : ArithmeticOperationBase
{
    // Update is called once per frame
    void Update()
    {
        
    }

    public override void InitialiseNode()
    {
        operatorStr = "+";
        operatorName = "add";

        base.InitialiseNode();
    }
}
