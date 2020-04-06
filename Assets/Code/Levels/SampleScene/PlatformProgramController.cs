using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformProgramController : ProgramController
{
    public GameObject PlatformContainer;

    public float stepHeight = 0.25f;

    // Original local positions of the platforms (at the time of calling raisePlatform/lowerPlatform, used to track progress in V3.Lerp)
    public List<Vector3> originalPositions = new List<Vector3>();

    // Initial local positions of the platforms (at the time of starting the level, used to cap their elevation)
    public List<Vector3> initPositions;

    // Time it takes to raise/lower a platform (in seconds)
    public float raisingTime = 0.5f;

    // Time taken already to raise/lower current platform (each PlatformProgram can only raise/lower one platform at a tick!)
    protected float currentRaiseTime = 0.0f;

    protected override Dictionary<string, Delegate> ControllerFunctions()
    {
        return new Dictionary<string, Delegate> { { "raisePlatform", new Action<string>(RaisePlatform) }, { "lowerPlatform", new Action<string>(LowerPlatform) } };
    }

    protected override void Start()
    {
        base.Start();
        CombineControllerFunctions(base.ControllerFunctions());
        CombineControllerFunctions(ControllerFunctions());

        GetPlatformPositions();

        foreach(Platform platform in PlatformContainer.GetComponentsInChildren<Platform>())
        {
            platform.Computer = gameObject;
        }
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

        if(initPositions == null || initPositions.Count < originalPositions.Count)
        {
            initPositions = originalPositions;
        }
    }

    public override ExecutionStatus ExecuteNode(NodeBase node)
    {
        ExecutionStatus baseStatus = base.ExecuteNode(node);
        if (!baseStatus.handover)
            return baseStatus;

        Logger.Log(CheckNodeType(node).ToString());
        switch(CheckNodeType(node))
        {
            // Handlers for different commands
            case NodeType.FunctionCallBase:
                Logger.Log($"Handling function {node.GetComponent<FunctionCallBase>().functionName}.");

                // If this node is going to alter the platform positions (e.g. by raising them), 
                // it's going to need their positions before calling this function node as a reference point, 
                // to be able to interpolate over time.
                //
                // Also, set a timer to mark processing as done after raisingTime passes.
                // Lastly, set processingDone to false, indicating that we may need an irregular tick time for this (the computer won't continue until processingDone is true)
                // TODO: replace the Action type value in the functions dictionary with a FunctionCall class that contains an "irregularTick" flag
                // TODO: GetPlatformPositions() should be in a virtual override (IrregularTickInit, IrregularTickFrame, IrregularTickFinished)
                FunctionCallBase functionCall = node.GetComponent<FunctionCallBase>();
                if (functionCall.functionName == "raisePlatform" || functionCall.functionName == "lowerPlatform")
                {
                    Platform platform = GetChildProgrammable(PlatformContainer, int.Parse((string)functionCall.GetRawParameters(symbolTable)[0])).GetComponent<Platform>();

                    // Check elevation boundaries
                    bool allowElevation = functionCall.functionName == "raisePlatform" && initPositions[platform.index].y + platform.MaxElevation < platform.transform.localPosition.y + stepHeight;
                    bool allowDown = functionCall.functionName == "lowerPlatform" && initPositions[platform.index].y + platform.MinElevation > platform.transform.localPosition.y - stepHeight;
                    bool allowMove = allowElevation || allowDown;

                    if (allowMove)
                    {
                        // Ignore this call if outside limits
                        processingDone = true;
                        return new ExecutionStatus { success = true, handover = false };
                    }

                    // Otherwise continue with raising/lowering the platform

                    GetPlatformPositions();
                    currentRaiseTime = 0.0f;
                    processingDone = false;
                    return new ExecutionStatus { success = true, handover = false };
                }
                break;
            default:
                Logger.Log("Unidentified node.");
                break;
        }

        return new ExecutionStatus { success = true, handover = false };
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
                if (ControllerFunctions().ContainsKey(functionCall.functionName))
                {
                    int index = -1;
                    string val = (string)functionCall.GetRawParameters(symbolTable)[0];
                    if (int.TryParse(val, out index) || int.TryParse(symbolTable[val].Value, out index))
                    {
                        if (index >= 0)
                        {
                            //Logger.Log($"Calling {functionCall.functionName}({index})");
                            functions[functionCall.functionName].DynamicInvoke(index.ToString());
                            // Increment the raising timer so that we know when we can set the processingDone flag
                            currentRaiseTime += Time.deltaTime;
                        }
                        else
                        {
                            Logger.LogWarning("Invalid platform index");
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"Can't convert platform index to integer ({gameObject.name}).");
                    }
                }
                else
                {
                    Logger.Log($"Unknown function {functionCall.functionName}.");
                }
            }

            // Finally, check if the timer overran raisingTime - if yes, mark processingDone as true.
            if(currentRaiseTime >= raisingTime)
            {
                Logger.Log("Platform raised!");
                processingDone = true;
            }
        }

        return true;
    }

    protected void RaisePlatform(string index)
    {
        int nIndex = -1;
        if(!int.TryParse(index, out nIndex))
        {
            Logger.Log("Invalid platform index, defaulting to 0.");
        }

        Programmable platform = GetChildProgrammable(PlatformContainer, nIndex).GetComponent<Programmable>();
        platform.transform.localPosition = Vector3.Lerp(platform.transform.localPosition, originalPositions[nIndex] + new Vector3(0.0f, stepHeight, 0.0f), currentRaiseTime / raisingTime);
    }
    protected void LowerPlatform(string index)
    {
        int nIndex = -1;
        if (!int.TryParse(index, out nIndex))
        {
            Logger.Log("Invalid platform index, defaulting to 0.");
        }

        Programmable platform = GetChildProgrammable(PlatformContainer, nIndex).GetComponent<Programmable>();
        platform.transform.localPosition = Vector3.Lerp(platform.transform.localPosition, originalPositions[nIndex] - new Vector3(0.0f, stepHeight, 0.0f), currentRaiseTime / raisingTime);
    }
}
