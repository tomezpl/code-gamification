using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implement this base class to add function calls that can execute in ExecuteFrame()
public abstract class ProgramController : Interactable
{
    public Transform editorUi;

    public double tickTime = 1.0;
    protected double timeSinceTick = 0.0;
    protected EditorProgram program;
    protected NodeBase currentNode;

    public bool programRunning = false;

    // This is a sort of a "locking mechanism". If a tick needs more than tickTime to perform its actions (e.g. lerping 3D positions), 
    // then set this to false, and the program won't progress to the next node until it's set back to true.
    //
    // If using this, remember to set this to false in ExecuteNode, then back to true once you meet the termination condition in ExecuteFrame.
    protected bool processingDone = true;

    protected bool processingDoneLastFrame = true;

    // Used to mark that we should wait for next tick before calling ExecuteNode to avoid desync. 
    // Is set to true when processingDone != processingDoneLastFrame
    protected bool waitForNextTick = false;

    protected bool firstTick = true;

    // Variables, constants etc. that are present in this program
    public Dictionary<string, FunctionParameter> symbolTable;

    protected Dictionary<string, System.Delegate> functions;

    public ProgramController() : base()
    {
        functions = new Dictionary<string, System.Delegate>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (!editorUi)
            Debug.LogWarning($"There is no EditorUI present in {gameObject.name}.{name}");
        else
        {
            program = editorUi.GetComponent<EditorProgram>();
            currentNode = program.programStart;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceTick += Time.deltaTime;
        if(currentNode == program.programStart && currentNode.NextNodeObject != null)
        {
            currentNode = (NodeBase)currentNode.nextNode;
            timeSinceTick = tickTime; // Skip the ProgramStart tick
        }
        if (programRunning)
        {
            processingDoneLastFrame = processingDone;
            ExecuteFrame();
            // Has processing finished this frame? If yes, wait for next tick.
            if(!processingDoneLastFrame && processingDone)
            {
                waitForNextTick = true;
            }
        }

        if (DistanceCheck())
        {
            if (Input.GetKeyUp(KeyCode.E))
                program.EditorActive = !program.EditorActive;
            if (Input.GetKeyUp(KeyCode.Space))
                programRunning = !programRunning;
        }
    }

    // Returns false if ExecuteFrame needs to proceed to next node on the next call.
    public virtual bool ExecuteFrame()
    {
        // TODO: move this to a separate method and leave ExecuteFrame as an abstract function?
        bool canTick = timeSinceTick >= tickTime;
        if (canTick && (processingDone || firstTick))
        {
            timeSinceTick = 0.0;
            if (currentNode != null && currentNode.gameObject.name != "ProgramEnd")
            {
                if (!waitForNextTick)
                {
                    ExecuteNode(currentNode);
                }
                else
                {
                    waitForNextTick = false;
                }
                firstTick = false;
                if (processingDone)
                {
                    currentNode = currentNode.NextNodeObject.GetComponent<NodeBase>();
                    Debug.Log("Continuing to next node!");
                    return false;
                }
            }
            else // There was an error. Go back to ProgramStart and stop the program execution.
            {
                programRunning = false;
                currentNode = program.programStart;
                return false;
            }
        }
        return true; // good to continue with any child implementations of this method
    }

    // Checks type of node and returns it as string
    protected string CheckNodeType(NodeBase node)
    {
        if (node.gameObject.GetComponent<ProgramStart>())
            return "ProgramStart";
        if (node.gameObject.GetComponent<FunctionCallBase>())
            return "FunctionCallBase";
        if (node.gameObject.GetComponent<ArithmeticOperationBase>())
            return "ArithmeticOperationBase";
        if (node.gameObject.GetComponent<CodeBlock>())
            return "CodeBlock";

        // If type can't be determined, return null
        Debug.LogWarning("Couldn't determine type of node, returning null! Check if ProgramController.CheckNodeType has been updated correctly with new node types.");
        return null;
    }

    protected GameObject GetChildProgrammable(GameObject container, int index = 0)
    {
        Programmable[] programmables = container.GetComponentsInChildren<Programmable>();
        foreach(Programmable programmable in programmables)
        {
            if(programmable.index == index)
            {
                return programmable.gameObject;
            }
        }
        return null;
    }

    // Performs actions defined by the Node
    // TODO: add some core functionality e.g. assigning, arithmetic etc? Then return bool to indicate if anything was invoked from the base method.
    // If not, only then continue to the derived implementations of this.
    public abstract void ExecuteNode(NodeBase node);
}
