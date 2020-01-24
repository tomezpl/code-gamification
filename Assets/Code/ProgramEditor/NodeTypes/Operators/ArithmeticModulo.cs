﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArithmeticModulo : ArithmeticOperationBase
{
    public override void InitialiseNode()
    {
        operatorStr = "%";
        operatorName = "modulo";

        base.InitialiseNode();
    }
}
