using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorProgramController : ProgramController
{
    // Controlled door
    public GameObject doorObject;

    public DoorProgramController() : base()
    {
        functions.Add("setLock", new System.Action<bool>(SetLock));
    }

    protected virtual void SetLock(bool state)
    {
        doorObject.GetComponent<UnlockableDoorWithLock>().SetLock(state);
    }

    public override void ExecuteNode(NodeBase node)
    {
        switch(CheckNodeType(node))
        {
            case "FunctionCallBase":
                FunctionCallBase functionCall = currentNode.GetComponent<FunctionCallBase>();
                // TODO: rewrite so we have a Dictionary of function names and function delegates, along with an array of types describing each parameter's type
                Delegate func = null;
                try
                {
                    func = functions[functionCall.functionName];
                }
                catch(Exception ex)
                {
                    Debug.Log($"Unknown function {functionCall.functionName}.");
                    return;
                }
                if (func != null)
                {
                    bool state = false;
                    if (bool.TryParse(functionCall.parameters[0].Value, out state))
                    {
                        func.DynamicInvoke(state);
                    }
                    else
                    {
                        Debug.LogWarning($"Can't convert function parameter for function {functionCall.functionName}. ({gameObject.name}).");
                        return;
                    }
                }
                break;
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
    }
}
