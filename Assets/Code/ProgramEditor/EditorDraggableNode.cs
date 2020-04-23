using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

// This script can actually be used for clickables etc. anything interactive, this name is irrelevant anymore.
public class EditorDraggableNode : MonoBehaviour
{
    protected RectTransform rectTransform;
    protected Selectable container;

    // Is the node being dragged right now?
    protected bool isDragged = false;
    // Location of the cursor on the node's surface when dragging started
    protected Vector2 anchorPoint;

    protected Vector2 lastFramePointer;

    // Editor UI this node is located in
    public EditorProgram owner;

    // should we allow dragging this node? set to false for START/END nodes
    public bool allowDrag = true;

    // is this clickable the arithmetic operator in the UI? if so, mark as true in prefab inspector.
    public bool isArithmeticOperator = false;

    // Tracking double-clicks: two left-clicks need to be detected within 1 second.
    protected int clickCounter;
    protected float timeSinceClick;
    protected const float doubleClickAllowedTime = 1.0f;

    // Is some node already being dragged? Prevents nodes on top of each other being dragged together at the same time.
    public static bool nodeAlreadyDragged = false;

    // On-screen help prompts
    private ClueHUD clueHud;

    // Default background colour of the node. The editor highlights an erroneous node as red, so this is just stored to revert to the original colour.
    public float[] color = null;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Find the owner EditorProgram
        Transform ownerTransform = transform.parent;
        while ((owner = ownerTransform.GetComponent<EditorProgram>()) == null)
        {
            ownerTransform = ownerTransform.parent;
        }

        clueHud = GameObject.Find("ClueHUD").GetComponent<ClueHUD>();

        // Find original colour
        Image image = GetComponent<Image>();
        if (image)
        {
            color = new float[4] { image.color.r, image.color.g, image.color.b, image.color.a };
        }
    }

    public void Drag(Vector2 input)
    {
        if (!allowDrag || owner.editingNodeProperty || (nodeAlreadyDragged && !isDragged))
            return;

        if (!isDragged)
        {
            lastFramePointer = input; // set last frame already to anchor point
            isDragged = true;
            nodeAlreadyDragged = true;
            anchorPoint = input - new Vector2(rectTransform.rect.xMin, rectTransform.rect.yMin);
        }
        else
        {
            Vector2 delta = -1 * (lastFramePointer - input);
            rectTransform.Translate(delta);
        }
    }

    // Checks if this object is a "Reference" prefab.
    public bool IsReference()
    {
        return !isArithmeticOperator && transform.childCount == 1 && GetComponentInChildren<Text>().name == "Text";
    }

    // Handle a double-click
    public void DoubleClick()
    {
        // Check if clicked a conditional
        if(transform.name == "Comparison")
        {
            DispatchEditingProperty(new Action<string>(LogicalOpEditingFinished), transform.parent.GetComponentInParent<LogicalBlock>().condition.comparison);
        }
        // Check if clicked an arithmetic operator 
        else if (isArithmeticOperator)
        {
            DispatchEditingProperty(new Action<string>(ArithmeticOpEditingFinished), GetComponentInParent<ArithmeticOperationBase>().operatorStr);
        }
        // Check if clicked a function name
        else if (transform.name == "FuncName")
        {
            // Removed: editing function names was causing issues
            //DispatchEditingProperty(new Action<string>(FunctionNameEditingFinished), GetComponentInParent<FunctionCallBase>().functionName);
        }
        else if (IsReference() || (transform.name.Contains("Parameter") && name != "Parameter"))
        {
            // TODO
            int paramIndex = -1;
            if (transform.name.Contains("Parameter") && name != "Parameter")
            {
                paramIndex = System.Convert.ToInt32(transform.name.Substring("Parameter".Length)) - 1;
                /*if (GetComponentInParent<FunctionCallBase>())
                {
                    paramIndex = (GetComponentInParent<FunctionCallBase>().parameters.Count - 1) - paramIndex;
                }*/
            }
            // FunctionCallBase
            if (paramIndex >= 0)
            {
                DispatchEditingProperty(new System.Action<string>(ParamEditingFinished), GetComponentInParent<FunctionCallBase>().parameters[paramIndex].Value);
            }
            // LogicalBlock
            else if(transform.name == "LHReference" || transform.name == "RHReference")
            {
                if (transform.name == "LHReference")
                {
                    DispatchEditingProperty(new Action<string>(x => ReferenceEditingFinished(transform.parent.GetComponentInParent<LogicalBlock>().condition.leftHand.Value = x)), transform.parent.GetComponentInParent<LogicalBlock>().condition.leftHand.Value);
                }
                else if (transform.name == "RHReference")
                {
                    DispatchEditingProperty(new Action<string>(x => ReferenceEditingFinished(transform.parent.GetComponentInParent<LogicalBlock>().condition.rightHand.Value = x)), transform.parent.GetComponentInParent<LogicalBlock>().condition.rightHand.Value);
                }
            }
            // Unknown
            else
            {
                //DispatchEditingProperty(new System.Action<string>(ReferenceEditingFinished), GetComponentInChildren<Text>().text);
            }
        }
    }

    public void LogicalOpEditingFinished(string finalValue)
    {
        if(string.IsNullOrWhiteSpace(finalValue))
        {
            return;
        }

        List<string> allowedOperators = new List<string>{ "==", "!=", ">=", "<=", ">", "<" };

        string enteredOp = finalValue.Trim();
        if(allowedOperators.Contains(enteredOp))
        {
            transform.parent.GetComponentInParent<LogicalBlock>().condition.comparison = enteredOp;
            transform.parent.GetComponentInParent<LogicalBlock>().UpdateUI();
        }
    }

    public void ArithmeticOpEditingFinished(string finalValue)
    {
        if (string.IsNullOrWhiteSpace(finalValue))
            return;

        Dictionary<string, string> allowedOperators = new Dictionary<string, string> { { "+", "add" }, { "-", "subtract" }, { "*", "multiply" }, { "/", "divide" }, { "%", "modulo" } };
        string enteredOp = finalValue.Trim()[0].ToString();
        if (allowedOperators.ContainsKey(enteredOp))
        {
            GetComponentInParent<ArithmeticOperationBase>().operatorStr = enteredOp;
            GetComponentInParent<ArithmeticOperationBase>().operatorName = allowedOperators[enteredOp];
            GetComponentInParent<ArithmeticOperationBase>().UpdateFunctionProperties();
        }
    }

    public void DispatchEditingProperty(System.Action<string> finishedCallback, string initValue)
    {
        owner.editedNodeValue = owner.editingNodeValue = initValue;

        Vector3[] corners = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        // Get screen-space rectangle of the node
        rectTransform.GetWorldCorners(corners);
        Rect nodeRect = new Rect(corners[0], corners[2] - corners[0]);

        owner.editingNodeInPlaceRect = nodeRect;
        owner.editingNodeInPlace = true;
        owner.editingNodeFinishedClb = finishedCallback;
        owner.editingNodeProperty = true;

        clueHud.SetCurrentPrompt(clueHud.EditingParamHoveredPrompt, gameObject);
    }

    public void ReferenceEditingFinished(string finalValue)
    {
        GetComponentInChildren<Text>().text = finalValue;
    }

    public void ParamEditingFinished(string finalValue)
    {

        int paramIndex = -1;
        if (transform.name.Contains("Parameter") && name != "Parameter")
        {
            paramIndex = System.Convert.ToInt32(transform.name.Substring("Parameter".Length)) - 1;
        }
        if (paramIndex >= 0)
        {
            GetComponentInParent<FunctionCallBase>().parameters[paramIndex].Value = finalValue;
            GetComponentInParent<FunctionCallBase>().UpdateFunctionProperties();

            ArithmeticOperationBase potentialArithmetic = GetComponentInParent<ArithmeticOperationBase>();
            if (potentialArithmetic != null && potentialArithmetic.NextNodeObject != null && potentialArithmetic.NextNodeObject.GetComponent<FunctionCallBase>() && potentialArithmetic.NextNodeObject.GetComponent<FunctionCallBase>().prevArithmetic == potentialArithmetic)
            {
                potentialArithmetic.NextNodeObject.GetComponent<FunctionCallBase>().UpdateFunctionProperties();
            }
        }
    }

    public void FunctionNameEditingFinished(string finalName)
    {
        Logger.Log($"Got back '{finalName}'.");
        GetComponentInParent<FunctionCallBase>().functionName = finalName;

        // TODO: maybe this should be done in FunctionCallBase?
        if (owner.programController && owner.programController.functions.ContainsKey(finalName))
        {
            System.Reflection.ParameterInfo[] paramInfo = owner.programController.functions[finalName].Method.GetParameters();
            GetComponentInParent<FunctionCallBase>().paramCount = (ushort)paramInfo.Length;
            GetComponentInParent<FunctionCallBase>().parameters = new List<FunctionParameter>();
            for(int i = 0; i < paramInfo.Length; i++)
            {
                GetComponentInParent<FunctionCallBase>().parameters.Add(new FunctionParameter { Type = paramInfo[i].ParameterType.ToString(), Name = paramInfo[i].Name, Value = "" });
            }
        }

        GetComponentInParent<FunctionCallBase>().UpdateFunctionProperties();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] corners = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        // Get screen-space rectangle of the node
        rectTransform.GetWorldCorners(corners);
        Rect nodeRect = new Rect(corners[0], corners[2] - corners[0]);
        Vector2 pointer = Input.mousePosition;

        UpdateHUD(pointer, nodeRect);

        // Drag if LMB held down and inside the node rectangle
        if (Input.GetKeyDown(KeyCode.Mouse0) && nodeRect.Contains(pointer) || isDragged)
        {
            Drag(pointer);
        }
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            isDragged = false;
            nodeAlreadyDragged = false;
        }

        // Have previewNode follow the mouse
        // This is used to show a preview of the node connection as the user is moving the mouse to the next/firstBody node
        if(name == "previewNode")
        {
            rectTransform.SetPositionAndRotation(pointer, Quaternion.identity);
        }

        // Check if this Draggable isn't part of another node and isn't the previewNode.
        if (transform.parent == owner.elementContainer.transform && name != "previewNode")
        {
            // Linking nodes

            // coords of the point needed to be right-clicked in order to select a firstBodyNode
            Vector2 firstBodyHook = new Vector2(nodeRect.xMin + nodeRect.width / 2.0f, nodeRect.yMax);
            Vector2 nextHook = new Vector2(nodeRect.xMax, nodeRect.yMin + nodeRect.height / 2.0f);

            if (!owner.linkingNodes && clueHud.hoveredNode == gameObject)
            {
                if (Vector2.Distance(pointer, firstBodyHook) < Vector2.Distance(pointer, nextHook))
                {
                    clueHud.potentialLinkingMode = EditorProgram.LinkingMode.FirstBodyNode;
                }
                else
                {
                    clueHud.potentialLinkingMode = EditorProgram.LinkingMode.NextNode;
                }
            }

            if (Input.GetKeyUp(KeyCode.Mouse1) && nodeRect.Contains(pointer))
            {
                if (!owner.linkingNodes)
                {
                    // Make sure we're not trying to link FROM ProgramEnd
                    if (GetComponent<ProgramEnd>() == null)
                    {
                        // Deteremining the right LinkingMode for CodeBlocks
                        if (GetComponent<CodeBlock>() != null)
                        {
                            // Check if it's node A and not node B
                            if (owner.linkingNodesObjects[0] == null)
                            {
                                if (Vector2.Distance(pointer, firstBodyHook) < Vector2.Distance(pointer, nextHook))
                                {
                                    owner.linkingNodeMode = EditorProgram.LinkingMode.FirstBodyNode;
                                }
                                else
                                {
                                    owner.linkingNodeMode = EditorProgram.LinkingMode.NextNode;
                                }
                            }
                        }
                        // By default, non-CodeBlocks have NextNode linkingmode
                        else
                        {
                            owner.linkingNodeMode = EditorProgram.LinkingMode.NextNode;
                        }

                        owner.linkingPreviewLine = new GameObject(
                            $"{(owner.linkingNodeMode == EditorProgram.LinkingMode.NextNode ? "nextNode" : "firstBody")}:{gameObject.name}->previewNode",
                            new Type[] { typeof(LineRenderer), typeof(LinkDescriptor) }
                        );
                        owner.linkingPreviewLine.transform.SetParent(owner.lineCanvas.transform, false);
                        LinkDescriptor linkDesc = owner.linkingPreviewLine.GetComponent<LinkDescriptor>();
                        linkDesc.prev = gameObject;
                        linkDesc.next = owner.linkingPreviewNode;
                        LineRenderer lineRenderer = owner.linkingPreviewLine.GetComponent<LineRenderer>();
                        lineRenderer.material = EditorProgram.lineMaterial;
                        lineRenderer.useWorldSpace = false;

                        owner.linkingNodes = true;
                        owner.linkingNodesObjects[0] = gameObject;
                    }
                }
                else
                {
                    owner.linkingNodesObjects[1] = gameObject;
                    owner.LinkCurrentlySelectedObjects();
                }
            }

            // Other events: Deleting node
            if (Input.GetKeyDown(KeyCode.Delete) && nodeRect.Contains(pointer) && GetComponent<NodeBase>() && !owner.editingNodeProperty && !GetComponent<ProgramEnd>() && !GetComponent<ProgramStart>())
            {
                clueHud.SetCurrentPrompt(null, null);
                GetComponent<NodeBase>().DeleteNode();
            }

            if (GetComponent<NodeBase>())
            {
                if(!owner.editingNodeProperty && owner.EditorActive && !GetComponent<ProgramStart>() && !GetComponent<ProgramEnd>() && !owner.choosingFunctionCall && !owner.choosingNode)
                {
                    if(nodeRect.Contains(pointer))
                    {
                        if (owner.nodeClipboard)
                        {
                            clueHud.CopyPasteNodePrompt.SetActive(true);
                            clueHud.CopyNodePrompt.SetActive(false);
                            clueHud.PasteNodePrompt.SetActive(false);
                        }
                        else
                        {
                            clueHud.CopyPasteNodePrompt.SetActive(false);
                            clueHud.CopyNodePrompt.SetActive(true);
                            clueHud.PasteNodePrompt.SetActive(false);
                        }
                    }
                    else if(clueHud.hoveredNode == gameObject)
                    {
                        if (owner.nodeClipboard)
                        {
                            clueHud.CopyPasteNodePrompt.SetActive(false);
                            clueHud.CopyNodePrompt.SetActive(false);
                            clueHud.PasteNodePrompt.SetActive(true);
                        }
                        else
                        {
                            clueHud.CopyPasteNodePrompt.SetActive(false);
                            clueHud.CopyNodePrompt.SetActive(false);
                            clueHud.PasteNodePrompt.SetActive(false);
                        }
                    }
                    else if(clueHud.hoveredNode == null)
                    {
                        if (owner.nodeClipboard)
                        {
                            clueHud.CopyPasteNodePrompt.SetActive(false);
                            clueHud.CopyNodePrompt.SetActive(false);
                            clueHud.PasteNodePrompt.SetActive(true);
                        }
                        else
                        {
                            clueHud.CopyPasteNodePrompt.SetActive(false);
                            clueHud.CopyNodePrompt.SetActive(false);
                            clueHud.PasteNodePrompt.SetActive(false);
                        }
                    }
                }

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    // Other events: Copying a node
                    if (Input.GetKeyDown(KeyCode.C) && !owner.editingNodeProperty && nodeRect.Contains(pointer) && !GetComponent<ProgramStart>() && !GetComponent<ProgramEnd>())
                    {
                        owner.nodeClipboard = gameObject;

                        clueHud.CopyPasteNodePrompt.SetActive(true);
                        clueHud.CopyNodePrompt.SetActive(false);
                        clueHud.PasteNodePrompt.SetActive(false);
                    }
                    // Other events: Pasting a node
                    else if (Input.GetKeyDown(KeyCode.V) && !owner.editingNodeProperty && owner.nodeClipboard == gameObject)
                    {
                        GameObject copy = owner.AddNode(owner.nodeClipboard, (pointer.x), pointer.y);
                        NodeBase copyNode = copy.GetComponent<NodeBase>();
                        copyNode.NextNodeObject = null;
                        copyNode.nextNode = null;
                        copyNode.PrevNodeObject = null;
                        copyNode.prevNode = null;
                        copyNode.ownerLoop = null;

                        if(copyNode.GetComponent<LogicalBlock>())
                        {
                            copyNode.GetComponent<LogicalBlock>().firstBodyNode = null;
                            copyNode.GetComponent<LogicalBlock>().FirstBodyNodeObject = null;
                        }

                        int guidStartIndex = copy.name.IndexOf("-");
                        copy.name = copy.name.Replace("(Clone)", "");
                        copy.name = copy.name.Substring(0, guidStartIndex <= 0 ? copy.name.Length : guidStartIndex) + copy.GetInstanceID();
                    }
                }
            }
        }

        // Double-click: usually this is editing a node
        if (Input.GetKeyUp(KeyCode.Mouse0) && nodeRect.Contains(pointer))
        {
            clickCounter++;
            timeSinceClick = 0.0f;
        }
        if(clickCounter >= 2)
        {
            if (transform.parent != owner)
            {
                DoubleClick();
            }
            clickCounter = 0;
        }
        if(timeSinceClick > doubleClickAllowedTime)
        {
            clickCounter = 0;
        }

        timeSinceClick += Time.deltaTime;

        lastFramePointer = pointer;
    }

    void UpdateHUD(Vector2 pointer, Rect nodeRect)
    {
        if (nodeRect.Contains(pointer) && owner.EditorActive)
        {
            if (name != "previewNode" && (GetComponent<NodeBase>() || owner.editingNodeProperty))
            {
                clueHud.hoveredNode = gameObject;
            }

            if (!owner.editingNodeProperty && !owner.linkingNodes)
            {
                if (GetComponent<NodeBase>())
                {
                    if (!GetComponent<ProgramStart>() && !GetComponent<ProgramEnd>())
                    {
                        clueHud.SetCurrentPrompt(clueHud.HoveredOverNodePrompt, gameObject);
                    }
                }
                else if(name != "previewNode" && transform.parent != owner && name != "FuncName")
                {
                    clueHud.SetCurrentPrompt(clueHud.HoveredOverParamPrompt, gameObject);
                }
            }
        }

        if (owner.editingNodeProperty && clueHud.hoveredNode == gameObject)
        {
            if (clueHud.currentPromptCaller == gameObject && clueHud.hoveredNode == gameObject)
            {
                clueHud.SetCurrentPrompt(clueHud.EditingParamHoveredPrompt, gameObject);
            }
        }
        else if (owner.editingNodeProperty && clueHud.hoveredNode != gameObject)
        {
            if(clueHud.currentPromptCaller == gameObject)
            {
                clueHud.SetCurrentPrompt(clueHud.EditingParamUnhoveredPrompt, gameObject);
            }
        }

        if ((!nodeRect.Contains(pointer) && owner.EditorActive))
        {
            if (clueHud.currentPromptCaller == gameObject && (clueHud.currentPromptSet == clueHud.HoveredOverNodePrompt || clueHud.currentPromptSet == clueHud.HoveredOverParamPrompt))
            {
                clueHud.SetCurrentPrompt(null, null);
            }

            if (clueHud.hoveredNode == gameObject)
            {
                clueHud.hoveredNode = null;
            }
        }
    }
}
