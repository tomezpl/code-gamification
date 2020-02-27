using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformProgramController : ProgramController
{
    public GameObject PlatformContainer;

    public float stepHeight = 0.25f;

    // Original local positions of the platforms
    protected List<Vector3> originalPositions = new List<Vector3>();

    // Time it takes to raise/lower a platform (in seconds)
    public float raisingTime = 0.5f;

    // Time taken already to raise/lower current platform (each PlatformProgram can only raise/lower one platform at a tick!)
    protected float currentRaiseTime = 0.0f;

    public PlatformProgramController() : base()
    {
        functions.Add("raisePlatform", new Action<int>(RaisePlatform));
        functions.Add("lowerPlatform", new Action<int>(LowerPlatform));
    }

    protected override void Start()
    {
        base.Start();

        GetPlatformPositions();
    }

    protected void GetPlatformPositions()
    {
        Programmable[] platforms = PlatformContainer.GetComponentsInChildren<Programmable>();

        originalPositions = new List<Vector3>();
        // TODO: this loop will crash if there are other children in PlatformContainer than Platform objects!
        for (int i = 0; i < platforms.Length; i++)
        {
            GameObject platform = GetChildProgrammable(PlatformContainer, i);
            if (platform.name.Contains("Platform"))
            {
                originalPositions.Add(platform.transform.localPosition);
            }
        }
    }

    public override bool ExecuteNode(NodeBase node)
    {
        if (base.ExecuteNode(node))
            return false;

        Debug.Log(CheckNodeType(node));
        switch(CheckNodeType(node))
        {
            // Handlers for different commands
            case NodeType.FunctionCallBase:
                Debug.Log($"Handling function {node.GetComponent<FunctionCallBase>().functionName}.");

                // If this node is going to alter the platform positions (e.g. by raising them), 
                // it's going to need their positions before calling this function node as a reference point, 
                // to be able to interpolate over time.
                //
                // Also, set a timer to mark processing as done after raisingTime passes.
                // Lastly, set processingDone to false, indicating that we may need an irregular tick time for this (the computer won't continue until processingDone is true)
                // TODO: replace the Action type value in the functions dictionary with a FunctionCall class that contains an "irregularTick" flag
                // TODO: GetPlatformPositions() should be in a virtual override (IrregularTickInit, IrregularTickFrame, IrregularTickFinished)
                if(node.GetComponent<FunctionCallBase>().functionName == "raisePlatform" || node.GetComponent<FunctionCallBase>().functionName == "lowerPlatform")
                {
                    GetPlatformPositions();
                    currentRaiseTime = 0.0f;
                    processingDone = false;
                    return true;
                }
                break;
            default:
                Debug.Log("Unidentified node.");
                break;
        }

        return false;
    }

    public override bool ExecuteFrame()
    {
        if (!base.ExecuteFrame())
            return false;

        if(!processingDone)
        {
            if(CheckNodeType(currentNode) == NodeType.FunctionCallBase)
            {
                FunctionCallBase functionCall = currentNode.GetComponent<FunctionCallBase>();
                // TODO: rewrite so we have a Dictionary of function names and function delegates, along with an array of types describing each parameter's type
                if (functions.ContainsKey(functionCall.functionName))
                {
                    if (functions[functionCall.functionName].Method.GetParameters()[0].ParameterType.Name.Contains("Int"))
                    {
                        int index = -1;
                        if (int.TryParse(functionCall.parameters[0].Value, out index) || int.TryParse(symbolTable[functionCall.parameters[0].Value].Value, out index))
                        {
                            if (index >= 0)
                            {
                                Debug.Log($"Calling {functionCall.functionName}({index})");
                                functions[functionCall.functionName].DynamicInvoke(index);
                                // Increment the raising timer so that we know when we can set the processingDone flag
                                currentRaiseTime += Time.deltaTime;
                            }
                            else
                            {
                                Debug.LogWarning("Invalid platform index");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Can't convert platform index to integer ({gameObject.name}).");
                        }
                    }
                }
                else
                {
                    Debug.Log($"Unknown function {functionCall.functionName}.");
                }
            }

            // Finally, check if the timer overran raisingTime - if yes, mark processingDone as true.
            if(currentRaiseTime >= raisingTime)
            {
                Debug.Log("Platform raised!");
                processingDone = true;
            }
        }

        return true;
    }

    protected void RaisePlatform(int index)
    {
        Programmable platform = GetChildProgrammable(PlatformContainer, index).GetComponent<Programmable>();
        platform.transform.localPosition = Vector3.Lerp(platform.transform.localPosition, originalPositions[index] + new Vector3(0.0f, stepHeight, 0.0f), currentRaiseTime / raisingTime);
    }
    protected void LowerPlatform(int index)
    {
        Programmable platform = GetChildProgrammable(PlatformContainer, index).GetComponent<Programmable>();
        platform.transform.localPosition = Vector3.Lerp(platform.transform.localPosition, originalPositions[index] - new Vector3(0.0f, stepHeight, 0.0f), currentRaiseTime / raisingTime);
    }
}
