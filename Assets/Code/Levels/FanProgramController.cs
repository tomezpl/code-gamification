using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanProgramController : ProgramController
{
    // Fan in the scene
    public Fan fan;

    // Declare puzzle-specific functions for the user program
    protected override Dictionary<string, Delegate> ControllerFunctions()
    {
        return new Dictionary<string, Delegate> { { "speedUp", new Action<string>(SpeedUp) }, { "speedDown", new Action<string>(SpeedDown) } };
    }

    // Add acceleration to the fan's torque
    private void SpeedUp(string acceleration)
    {
        double accel = 0.0;
        if (!double.TryParse(acceleration, out accel))
        {

        }

        fan.speed += accel;
        fan.speedMult = 1.0f;
    }

    // Slow down the fan torque
    private void SpeedDown(string deceleration)
    {
        double decel = 0.0;
        if(!double.TryParse(deceleration, out decel))
        {

        }

        fan.speed = Mathf.Max(0.0f, (float)(fan.speed - decel));
    }

    // Handle puzzle-specific function calls if ProgramController.ExecuteNode set the handover flag
    public override ExecutionStatus ExecuteNode(NodeBase node)
    {
        ExecutionStatus baseStatus = base.ExecuteNode(node);
        if (!baseStatus.handover)
            return baseStatus;

        if (CheckNodeType(node) == NodeType.FunctionCallBase)
        {
            FunctionCallBase funcCall = node.GetComponent<FunctionCallBase>();

            if (functions.ContainsKey(funcCall.functionName))
            {
                functions[funcCall.functionName].DynamicInvoke(funcCall.GetRawParameters(symbolTable));
                return new ExecutionStatus { success = true, handover = false };
            }
        }

        return new ExecutionStatus { success = true, handover = true };
    }
}
