using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarProgramController : ProgramController
{
    public Car car;
    public GameObject grid;
    public GridCell[] cells;
    public UnlockableDoorWithLock doorToUnlock;
    GridCell endCell = null;

    protected override Dictionary<string, Delegate> ControllerFunctions()
    {
        return new Dictionary<string, Delegate> { { "moveForward", new Action(MoveForward) }, { "turnLeft", new Action(TurnLeft) }, { "turnRight", new Action(TurnRight) } };
    }

    private void TurnRight()
    {
        car.TurnRight();
    }

    private void TurnLeft()
    {
        car.TurnLeft();
    }

    private void MoveForward()
    {
        bool reachedEnd = false;
        Vector2 nextCoords = car.CurrentCoord + new Vector2(car.XDir, car.YDir);
        if (nextCoords.x >= 0 && nextCoords.y >= 0 && nextCoords.x <= 4 && nextCoords.y <= 9)
        {
            foreach(GridCell cell in cells)
            {
                if(cell.blocked && cell.Coords == nextCoords)
                {
                    return; // prevent from going into blocked cells/out of bounds
                }
                else if(!cell.blocked && cell.Coords == nextCoords && cell.name == "End")
                {
                    reachedEnd = true;
                    endCell = cell;
                }
            }
            car.Move();

            if(reachedEnd)
            {
                endCell.GetComponent<BooleanSwitchTest>().isEnabled = false;
                endCell.GetComponent<BooleanSwitchTest>().ToggleBoolean();
                doorToUnlock.SetLock(false);
            }
            else if(endCell != null)
            {
                endCell.GetComponent<BooleanSwitchTest>().isEnabled = true;
                endCell.GetComponent<BooleanSwitchTest>().ToggleBoolean();
                doorToUnlock.SetLock(true);
            }
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        CombineControllerFunctions(base.ControllerFunctions());
        CombineControllerFunctions(ControllerFunctions());

        cells = grid.GetComponentsInChildren<GridCell>();
    }

    public override ExecutionStatus ExecuteNode(NodeBase node)
    {
        ExecutionStatus baseStatus = base.ExecuteNode(node);
        if (!baseStatus.handover)
            return baseStatus;

        if(CheckNodeType(node) == NodeType.FunctionCallBase)
        {
            FunctionCallBase funcCall = node.GetComponent<FunctionCallBase>();

            if(functions.ContainsKey(funcCall.functionName))
            {
                functions[funcCall.functionName].DynamicInvoke();
                return new ExecutionStatus { success = true, handover = false };
            }
        }

        return new ExecutionStatus { success = true, handover = true };
    }
}
