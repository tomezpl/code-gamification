using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Implement this base class to add function calls that can execute in ExecuteFrame()
public class ProgramController : Interactable
{
    public Transform editorUi;

    public double tickTime = 1.0;
    protected double timeSinceTick = 0.0;
    protected EditorProgram program;
    public NodeBase currentNode;

    public string expectedOutput = "";

    public bool programRunning = false;

    // This is a sort of a "locking mechanism". If a tick needs more than tickTime to perform its actions (e.g. lerping 3D positions), 
    // then set this to false, and the program won't progress to the next node until it's set back to true.
    //
    // If using this, remember to set this to false in ExecuteNode, then back to true once you meet the termination condition in ExecuteFrame.
    public bool processingDone = true;

    protected bool processingDoneLastFrame = true;

    // Used to mark that we should wait for next tick before calling ExecuteNode to avoid desync. 
    // Is set to true when processingDone != processingDoneLastFrame
    protected bool waitForNextTick = false;

    protected bool firstTick = true;

    // Variables, constants etc. that are present in this program
    public Dictionary<string, FunctionParameter> symbolTable;

    public Dictionary<string, System.Delegate> functions;
    public List<string> hiddenFunctions = new List<string> { "create list" };

    // Prepend symbol names with this if the symbol is only used internally by the game.
    public const string HiddenSymbolPrefix = "_INTERNAL_GAME:";

    // This is used by CheckNodeType
    public enum NodeType { Unknown, FunctionCallBase, ProgramStart, ArithmeticOperationBase, CodeBlock, AssignValue, ProgramEnd, LogicalBlock, WhileLoop, AllocateArray, Continue };

    public string outputBuffer = "";

    // Returns true if there are any nodes other than Start and End present in the program editor
    bool hasAnyNodes()
    {
        List<NodeBase> nodes = new List<NodeBase>(program.elementContainer.GetComponentsInChildren<NodeBase>());
        return !(nodes.Count == 2 && nodes.Contains(program.programStart) && nodes.Contains(program.programEnd));
    }

    // Initialises a symbol table
    public void InitSymTable()
    {
        symbolTable = new Dictionary<string, FunctionParameter> {
            { "True", new FunctionParameter { Value = "True", Type = "Boolean" } },
            { "False", new FunctionParameter { Value = "False", Type = "Boolean" } },
            { "None", new FunctionParameter() } };
    }

    protected void OutPrintNewline()
    {
        outputBuffer += "\n";
    }

    public void CreateList(string size, string name)
    {
        int nSize = -1;

        if(int.TryParse(size, out nSize))
        {
            // TODO
        }

        if(nSize < 0 || string.IsNullOrWhiteSpace(name))
        {
            // TODO: log error?
        }
    }

    public void OutPrint(string text)
    {
        if (text.StartsWith("\"") && text.EndsWith("\""))
        {
            if (text.Length >= 3)
            {
                text = text.Substring(1, text.Length - 2);
            }
            else
            {
                text = "";
            }
        }
        outputBuffer += $"{text}";
        Logger.Log(text);
    }

    Dictionary<string, Delegate> BaseControllerFunctions()
    {
        Dictionary<string, Delegate> ret = new Dictionary<string, Delegate>();

        ret.Add("wait", new Action(Wait));
        ret.Add("print", new System.Action<string>(OutPrint));
        ret.Add("printNewline", new Action(OutPrintNewline));
        ret.Add("create list", new Action<string, string>(CreateList));

        return ret;
    }

    private void Wait()
    {
        return;
    }

    protected virtual Dictionary<string, Delegate> ControllerFunctions()
    {
        return BaseControllerFunctions();
    }

    protected virtual void CombineControllerFunctions(Dictionary<string, Delegate> childFunctions)
    {
        if(functions == null)
        {
            functions = new Dictionary<string, Delegate>();
        }

        foreach (KeyValuePair<string, Delegate> childFunc in childFunctions)
        {
            if (!functions.ContainsKey(childFunc.Key))
            {
                functions.Add(childFunc.Key, childFunc.Value);
            }
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        functions = new Dictionary<string, System.Delegate>();
        InitSymTable();

        CombineControllerFunctions(ControllerFunctions());

        if (!editorUi)
            Logger.LogWarning($"There is no EditorUI present in {gameObject.name}.{name}");
        else
        {
            program = editorUi.GetComponent<EditorProgram>();
            program.programController = this;
            currentNode = program.programStart;
        }

        // Add OutputRenderer if it doesn't exist yet
        if(!GameObject.Find("OutputRenderer"))
        {
            Instantiate(Resources.Load("Prefabs/ProgramEditor/OutputRenderer")).name = "OutputRenderer";
        }

        if (transform.Find("CurrentLine"))
        {
            transform.Find("CurrentLine").gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceTick += Time.deltaTime;
        if(currentNode == program.programStart && currentNode.NextNodeObject != null)
        {
            if (programRunning)
            {
                currentNode = (NodeBase)currentNode.nextNode;
            }
            timeSinceTick = tickTime; // Skip the ProgramStart tick
            InitSymTable();
            // TODO: this doesn't actually reset the buffer?
            outputBuffer = "";
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

        // Before doing key detection, first check if the user isn't inputting anything in the editor
        if (!editorUi.GetComponent<EditorProgram>().editingNodeProperty)
        {
            if (DistanceCheck())
            {
                if (Input.GetKeyUp(KeyCode.E))
                    program.EditorActive = !program.EditorActive;
                if (Input.GetKeyUp(KeyCode.Space) && !programRunning)
                {
                    processingDone = true;
                    programRunning = true;
                    currentNode = program.programStart;

                    // Make sure Start and End are linked if they're the only nodes in the program
                    if(!hasAnyNodes())
                    {
                        program.programStart.NextNodeObject = program.programEnd.gameObject;
                        program.programStart.nextNode = program.programEnd;

                        program.programEnd.PrevNodeObject = program.programStart.gameObject;
                        program.programEnd.prevNode = program.programStart;
                    }
                    // Otherwise, check if the End node is linked to by anything. 
                    // If not, choose the node that isn't in a loop and doesn't have a nextNode yet.
                    else
                    {
                        if(program.programEnd.PrevNodeObject == null)
                        {
                            NodeBase[] nodes = program.elementContainer.GetComponentsInChildren<NodeBase>();
                            foreach(NodeBase node in nodes)
                            {
                                if(node.ownerLoop == null && node.NextNodeObject == null && node.PrevNodeObject != null)
                                {
                                    node.NextNodeObject = program.programEnd.gameObject;
                                    node.nextNode = program.programEnd;

                                    program.programEnd.PrevNodeObject = node.gameObject;
                                    program.programEnd.prevNode = node;

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        if(transform.Find("CurrentLine") && editorUi.GetComponent<EditorProgram>().EditorActive)
        {
            transform.Find("CurrentLine").gameObject.SetActive(false);
        }

        if (transform.Find("CurrentLine") && !editorUi.GetComponent<EditorProgram>().EditorActive && programRunning)
        {
            transform.Find("CurrentLine").gameObject.SetActive(true);
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
                    string currentLine = ((IProgramNode)currentNode).Serialize();
                    if (transform.Find("CurrentLine"))
                    {
                        if (!editorUi.GetComponent<EditorProgram>().EditorActive)
                        {
                            transform.Find("CurrentLine").gameObject.SetActive(true);
                        }
                        else
                        {
                            transform.Find("CurrentLine").gameObject.SetActive(false);
                        }
                    }
                    GameObject.Find("OutputRenderer").transform.Find("Canvas").GetComponentInChildren<Text>().text = currentLine;
                }
                else
                {
                    waitForNextTick = false;
                }
                firstTick = false;
                if (processingDone)
                {
                    // Regular flow
                    if (currentNode.nextNode != null)
                    {
                        currentNode = currentNode.NextNodeObject.GetComponent<NodeBase>();
                    }
                    // Reached end of loop
                    else if(currentNode.nextNode == null && currentNode.ownerLoop != null)
                    {
                        if (currentNode.ownerLoop.GetComponent<WhileLoop>())
                        {
                            currentNode = currentNode.ownerLoop;
                        }
                        else if(currentNode.ownerLoop.GetComponent<LogicalBlock>())
                        {
                            currentNode = (NodeBase)currentNode.ownerLoop.nextNode;
                        }
                    }
                    Logger.Log($"Continuing to next node: {currentNode.name}");
                    return false;
                }
            }
            else if(currentNode.gameObject.name == "ProgramEnd")
            {
                ProgramEndCallback();
                programRunning = false;
                currentNode = program.programStart;
                GameObject.Find("OutputRenderer").transform.Find("Canvas").GetComponentInChildren<Text>().text = "";
                if (transform.Find("CurrentLine"))
                {
                    transform.Find("CurrentLine").gameObject.SetActive(false);
                }
            }
            else // There was an error. Go back to ProgramStart and stop the program execution.
            {
                programRunning = false;
                currentNode = program.programStart;
                GameObject.Find("OutputRenderer").transform.Find("Canvas").GetComponentInChildren<Text>().text = "";
                if (transform.Find("CurrentLine"))
                {
                    transform.Find("CurrentLine").gameObject.SetActive(false);
                }
                return false;
            }
        }
        return true; // good to continue with any child implementations of this method
    }

    protected virtual void ProgramEndCallback()
    {
        
    }

    // Checks type of node and returns it as NodeType
    // MAKE SURE TO FOLLOW THE INHERITANCE HIERARCHY FROM CHILD TO BASE. Otherwise, you'll run into problems where everything is a NodeBase...
    protected NodeType CheckNodeType(NodeBase node)
    {
        if (node.gameObject.GetComponent<ProgramStart>())
            return NodeType.ProgramStart;
        if (node.gameObject.GetComponent<ProgramEnd>())
            return NodeType.ProgramEnd;

        if (node.gameObject.GetComponent<AllocateArray>())
            return NodeType.AllocateArray;
        if (node.gameObject.GetComponent<AssignValue>())
            return NodeType.AssignValue;
        if (node.gameObject.GetComponent<ArithmeticOperationBase>())
            return NodeType.ArithmeticOperationBase;
        if (node.gameObject.GetComponent<FunctionCallBase>())
            return NodeType.FunctionCallBase;

        if (node.gameObject.GetComponent<WhileLoop>())
            return NodeType.WhileLoop;
        if (node.gameObject.GetComponent<LogicalBlock>())
            return NodeType.LogicalBlock;
        if (node.gameObject.GetComponent<CodeBlock>())
            return NodeType.CodeBlock;

        if (node.gameObject.GetComponent<Continue>())
            return NodeType.Continue;

        // If type can't be determined, return null
        Logger.LogWarning("Couldn't determine type of node, returning null! Check if ProgramController.CheckNodeType has been updated correctly with new node types.");
        return NodeType.Unknown;
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
    public virtual ExecutionStatus ExecuteNode(NodeBase node)
    {
        if (node == null)
        {
            programRunning = false;
            currentNode = program.programStart;
            return new ExecutionStatus { success = false, handover = false };
        }

        switch (CheckNodeType(node))
        {
            // Handlers for different commands
            case NodeType.ProgramStart:
                Logger.Log("Program starting!");
                processingDone = true;

                Debug.DebugBreak();
                InitSymTable();
                // TODO: this doesn't actually reset the buffer?
                outputBuffer = "";

                return new ExecutionStatus { success = true, handover = false };
            case NodeType.AssignValue:
                AssignValue assignValue = node.GetComponent<AssignValue>();
                string symbolVal = ArithmeticOperationBase.GetResult(assignValue.rightHand.Value, ref symbolTable);
                string symbolName = "";

                // Is it an array index?
                string indexName = "";
                string[] indexSplit = assignValue.leftHand.Value.Split('[');
                if (indexSplit != null && indexSplit.Length == 2)
                {
                    symbolName = indexSplit[0];
                    indexName = indexSplit[1].Substring(0, indexSplit[1].Length - 1);
                    indexName = ArithmeticOperationBase.GetResult(indexName, ref symbolTable);
                    Logger.Log($"\"{currentNode.Serialize()}\": index was {indexName}");
                }
                else
                {
                    symbolName = assignValue.leftHand.Value;
                }

                bool isString = assignValue.leftHand.IsReference ? (symbolTable.ContainsKey(symbolVal) && symbolTable[symbolVal].Value.Trim().StartsWith("\"") && symbolTable[symbolVal].Value.Trim().EndsWith("\"")) : (symbolVal.Trim().StartsWith("\"") && symbolVal.Trim().EndsWith("\""));
                bool isReference = assignValue.leftHand.IsReference ? true : !isString && symbolTable.ContainsKey(symbolVal);

                Logger.Log($"indexName={indexName}");
                if (!string.IsNullOrWhiteSpace(indexName))
                {
                    if(symbolTable.ContainsKey(indexName))
                    {
                        symbolName += $"[{symbolTable[indexName].Value}]";
                    }
                    else
                    {
                        symbolName += $"[{indexName}]";
                    }
                }

                Logger.Log($"Assigning {symbolVal} to {symbolName}");
                double tempNum = 0.0;
                string assignedType = (isString ? "String" : (symbolVal.Trim() == "True" || symbolVal.Trim() == "False" ? "Boolean" : (double.TryParse(symbolVal.Trim(), out tempNum) ? "Number" : "")));
                if (!symbolTable.ContainsKey(symbolName))
                {
                    // TODO: Type = "Int" won't always work, we need a generic type like Number, however Reflection.ParameterInfo needs to be converted in that case
                    symbolTable.Add(symbolName, new FunctionParameter { Name = assignValue.rightHand.Name, Type = isReference ? symbolTable[symbolVal].Type : assignedType, Value = isReference ? symbolTable[symbolVal].Value : symbolVal }); ;
                }
                else
                {
                    symbolTable[symbolName] = new FunctionParameter { Name = assignValue.rightHand.Name, Type = isReference ? symbolTable[symbolVal].Type : assignedType, Value = isReference ? symbolTable[symbolVal].Value : symbolVal };
                }
                return new ExecutionStatus { success = true, handover = false };
            case NodeType.ArithmeticOperationBase:
                // Arithmetic only takes a tick when it gets executed
                currentNode = (NodeBase)currentNode.nextNode;
                return ExecuteNode(currentNode);
            case NodeType.ProgramEnd:
                processingDone = true;
                programRunning = false;
                return new ExecutionStatus { success = true, handover = false };
            case NodeType.FunctionCallBase:
                string funcName = node.GetComponent<FunctionCallBase>().functionName;
                if (BaseControllerFunctions().ContainsKey(funcName))
                {
                    // TODO: passing a copy of symbolTable here might consume too much memory. Make static?
                    functions[funcName].DynamicInvoke(node.GetComponent<FunctionCallBase>().GetRawParameters(symbolTable));
                    Logger.Log($"Found base function {funcName}");
                    return new ExecutionStatus { success = true, handover = false };
                }
                Logger.Log($"Couldn't find base function {funcName}");
                break;
            case NodeType.LogicalBlock: case NodeType.WhileLoop:
                if (node.GetComponent<LogicalBlock>().condition.Evaluate(ref symbolTable))
                {
                    NodeBase nodeToFollow = (NodeBase)(node.GetComponent<LogicalBlock>().firstBodyNode);
                    if(nodeToFollow != null)
                    {
                        currentNode = nodeToFollow;
                    }
                    else
                    {
                        currentNode = (NodeBase)currentNode.nextNode;
                    }
                    return ExecuteNode(currentNode);
                }
                break;
            case NodeType.AllocateArray:
                if(node.GetComponent<AllocateArray>())
                {
                    int count = -1;
                    // Check if entered size was a valid >= 0 integer.
                    // TODO: unexpected behaviour when allocating with size == 0
                    Logger.Log($"Allocating array with count {(string)node.GetComponent<AllocateArray>().GetRawParameters(symbolTable)[0]}");
                    if (int.TryParse((string)node.GetComponent<AllocateArray>().GetRawParameters(symbolTable)[0], out count))
                    {
                        string arrName = node.GetComponent<AllocateArray>().parameters[1].Value;
                        if (string.IsNullOrWhiteSpace(arrName))
                        {
                            // TODO: error too?
                            return new ExecutionStatus { success = false, handover = false };
                        }
                        for (int i = 0; i < count; i++)
                        {
                            Logger.Log($"Adding array element \"{arrName}[{i}]\"");
                            symbolTable.Add($"{arrName}[{i}]", new FunctionParameter());
                        }
                        return new ExecutionStatus { success = true, handover = false };
                    }
                }
                break;
            case NodeType.Continue:
                if(node.GetComponent<Continue>())
                {
                    // Find the while loop
                    LogicalBlock owner = node.ownerLoop;
                    while(owner != null)
                    {
                        WhileLoop loop = owner.GetComponent<WhileLoop>();
                        if (loop != null)
                        {
                            currentNode = loop;
                            return ExecuteNode(currentNode);
                        }
                        else
                        {
                            owner = owner.ownerLoop;
                        }
                    }
                    if(owner == null)
                    {
                        return new ExecutionStatus { success = false, handover = false };
                    }
                }
                break;
        }

        return new ExecutionStatus { success = true, handover = true };
    }
}
