using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorProgramController : ProgramController
{
    // Controlled door
    public GameObject doorObject;

    float timer = 0.0f;
    bool timerActive = false;

    public UnlockableDoorWithLock.LockType lockType;
    public int fizzBuzzN = 30;

    protected virtual void SetLock(string state)
    {
        Debug.Log($"Called setLock({state})");
        bool bState = true;
        if(!bool.TryParse(state, out bState))
        {
            // TODO: error handling?
        }

        doorObject.GetComponent<UnlockableDoorWithLock>().SetLock(bState);
        timer = doorObject.GetComponent<UnlockableDoorWithLock>().doorOpenDuration + doorObject.GetComponent<UnlockableDoorWithLock>().doorOpeningTime;
        timerActive = true;
    }

    protected override void ProgramEndCallback()
    {
        base.ProgramEndCallback();

        if(lockType == UnlockableDoorWithLock.LockType.FizzBuzz)
        {
            // Check FizzBuzz output
            if(outputBuffer.ToLower().Contains(expectedOutput))
            {
                SetLock("false");
            }
            else
            {
                SetLock("true");
            }
        }
    }

    public override ExecutionStatus ExecuteNode(NodeBase node)
    {
        ExecutionStatus baseStatus = base.ExecuteNode(node);
        if (!baseStatus.handover)
        {
            return baseStatus;
        }

        switch(CheckNodeType(node))
        {
            case NodeType.FunctionCallBase:
                FunctionCallBase functionCall = currentNode.GetComponent<FunctionCallBase>();
                // TODO: rewrite so we have a Dictionary of function names and function delegates, along with an array of types describing each parameter's type
                Delegate func = null;
                try
                {
                    func = functions[functionCall.functionName];
                }
                catch(Exception)
                {
                    Debug.Log($"Unknown function {functionCall.functionName}.");
                    return new ExecutionStatus { success = false, handover = false };
                }
                if (func != null)
                {
                    if (lockType != UnlockableDoorWithLock.LockType.FizzBuzz)
                    {
                        bool state = false;

                        string val = (string)functionCall.GetRawParameters(symbolTable)[0];

                        // Check literal & symbol table
                        // TODO: make symbol lookup more robust/universal? A template function maybe?
                        if (bool.TryParse(val, out state) || bool.TryParse(symbolTable[val].Value, out state))
                        {
                            // we need a custom amount of time for processing this node
                            processingDone = false;
                            func.DynamicInvoke(state.ToString());
                            return new ExecutionStatus { success = true, handover = false };
                        }
                        else
                        {
                            Debug.LogWarning($"Can't convert function parameter for function {functionCall.functionName}. ({gameObject.name}).");
                            return new ExecutionStatus { success = false, handover = false };
                        }
                    }
                    else
                    {
                        processingDone = true;
                        func.DynamicInvoke();
                        return new ExecutionStatus { success = true, handover = false };
                    }
                }
                break;
        }

        return new ExecutionStatus { success = true, handover = false };
    }

    public override bool ExecuteFrame()
    {
        if (!base.ExecuteFrame())
            return false;

        timer -= Time.deltaTime;

        if(timerActive)
        {
            if (timer <= 0.0f)
            {
                timer = 0.0f;
                timerActive = false;

                processingDone = true;
            }
        }

        return true;
    }

    protected override Dictionary<string, Delegate> ControllerFunctions()
    {
        if (lockType == UnlockableDoorWithLock.LockType.FizzBuzz)
        {
            return new Dictionary<string, Delegate> { { "fizz", new Action(Fizz) }, { "buzz", new Action(Buzz) }, { "fizzBuzz", new Action(FizzBuzz) } };
        }
        else
        {
            return new Dictionary<string, Delegate> { { "setLock", new Action<string>(SetLock) } };
        }
    }

    private void Fizz()
    {
        OutPrint("fizz");
        OutPrintNewline();
    }

    private void Buzz()
    {
        OutPrint("buzz");
        OutPrintNewline();
    }

    private void FizzBuzz()
    {
        OutPrint("fizzbuzz");
        OutPrintNewline();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //functions.Add("setLock", new System.Action<string>(SetLock));
        CombineControllerFunctions(base.ControllerFunctions());
        CombineControllerFunctions(ControllerFunctions());

        // Prepare expected output for fizz buzz puzzle
        if(lockType == UnlockableDoorWithLock.LockType.FizzBuzz)
        {
            for(int i = 1; i <= fizzBuzzN; i++)
            {
                bool printNumber = true;
                if(i % 3 == 0)
                {
                    expectedOutput += "fizz";
                    printNumber = false;
                }
                if(i % 5 == 0)
                {
                    expectedOutput += "buzz";
                    printNumber = false;
                }
                if(printNumber)
                {
                    expectedOutput += i.ToString();
                }
                if(i != fizzBuzzN)
                {
                    expectedOutput += "\n";
                }
            }
        }
    }
}
