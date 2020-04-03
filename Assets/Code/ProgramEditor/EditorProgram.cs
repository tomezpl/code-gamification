using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorProgram : MonoBehaviour
{
    public Canvas lineCanvas;
    public static Material lineMaterial = null;

    public GameObject elementContainer;

    public ProgramStart programStart;
    public ProgramEnd programEnd;
    public ProgramController programController;

    public GameObject generatedCodeContainer;
    public Text generatedCodeText;

    public int framesToDisable = 2;

    int frameCounter = 0;
    protected static Dictionary<string, KeyValuePair<string, GameObject>> nodePrefabs = null;

    public GUISkin guiSkin;

    // initial pose for the newly added node
    // will be set to the mouse cursor's last position before pressing TAB
    public Vector2 newNodeInitPos;

    private ClueHUD clueHud;
    
    public bool EditorActive {
        get { return editorActive; }
        set {
            if (!elementContainer)
            {
                elementContainer = transform.Find("Canvas").Find("Elements").gameObject;
            }
            if (!lineCanvas)
            {
                lineCanvas = transform.Find("LineCanvas").GetComponent<Canvas>();
            }
            if (value == true && editorActive != true)
            {
                EnableEditor();
            }
            else if(value == false && editorActive != false)
            {
                DisableEditor();
            }
        }
    }

    private bool editorActive = false;

    public bool enableEditorOnStartup = false;

    // Editor state
    public enum LinkingMode { NextNode, FirstBodyNode }
    public bool choosingNode = false;
    public bool choosingFunctionCall = false;
    GameObject addedNode = null;
    public bool linkingNodes = false;
    public LinkingMode linkingNodeMode = LinkingMode.NextNode;
    public GameObject[] linkingNodesObjects = new GameObject[2];
    public GameObject linkingPreviewLine;
    public GameObject linkingPreviewNode;
    public bool editingNodeProperty = false;
    public string editingNodeValue = ""; // original value of the string that is edited - this will be returned if Esc is pressed
    public string editedNodeValue = ""; // final edited value to return if Enter is pressed
    public bool editingNodeInPlace = false;
    public Rect editingNodeInPlaceRect = new Rect();
    public System.Action<string> editingNodeFinishedClb = null;
    bool editedNodeFocused = false;

    public GameObject nodeClipboard;

    // FlowChart: the default node editor
    // CodeViewer: the screen with generated Python-syntax code representation of our flowchart
    public enum EditorMode { FlowChart, CodeViewer };
    public EditorMode editorMode = EditorMode.FlowChart;

    void SwitchMode(EditorMode mode)
    {

        if (mode == EditorMode.FlowChart)
        {
            elementContainer.SetActive(true);
            generatedCodeContainer.SetActive(false);
            editorMode = mode;
        }
        else
        {
            if (editorMode == EditorMode.FlowChart)
            {
                choosingNode = false;
                choosingFunctionCall = false;

                linkingNodes = false;
                linkingNodesObjects[0] = linkingNodesObjects[1] = null;
                // Delete the linking preview line
                if (linkingPreviewLine != null)
                {
                    Destroy(linkingPreviewLine);
                }

                clueHud.SetCurrentPrompt(null, null);
            }

            if (!string.IsNullOrWhiteSpace(programStart.Serialize()))
            {
                elementContainer.SetActive(false);
                generatedCodeContainer.SetActive(true);
                generatedCodeText.text = programStart.Serialize();
                editorMode = mode;
            }
        }
    }

    void DisableEditor()
    {
        Cursor.lockState = CursorLockMode.Locked;

        foreach (EditorDraggableNode draggable in elementContainer.GetComponentsInChildren<EditorDraggableNode>())
        {
            draggable.enabled = false;
        }
        lineCanvas.worldCamera.enabled = false;
        lineCanvas.GetComponent<Canvas>().enabled = false;
        foreach (LineRenderer line in lineCanvas.GetComponentsInChildren<LineRenderer>())
        {
            line.enabled = false;
        }
        if (GameObject.Find("Player"))
            GameObject.Find("Player").GetComponentInChildren<Camera>().enabled = true;
        transform.Find("Canvas").GetComponent<Canvas>().enabled = false;
        editorActive = false;

        linkingNodes = false;
        linkingNodesObjects[0] = linkingNodesObjects[1] = null;
        // Delete the linking preview line
        if (linkingPreviewLine != null)
        {
            Destroy(linkingPreviewLine);
        }

        nodeClipboard = null;

        choosingNode = false;
        choosingFunctionCall = false;
    }

    void EnableEditor()
    {
        SwitchMode(EditorMode.FlowChart);

        Cursor.lockState = CursorLockMode.None;

        foreach (EditorDraggableNode draggable in elementContainer.GetComponentsInChildren<EditorDraggableNode>())
        {
            draggable.enabled = true;
        }
        lineCanvas.worldCamera.enabled = true;
        lineCanvas.GetComponent<Canvas>().enabled = true;
        foreach(LineRenderer line in lineCanvas.GetComponentsInChildren<LineRenderer>())
        {
            line.enabled = true;
        }
        if (GameObject.Find("Player"))
            GameObject.Find("Player").GetComponentInChildren<Camera>().enabled = false;
        transform.Find("Canvas").GetComponent<Canvas>().enabled = true;

        editorActive = true;
    }

    public GameObject AddNode(string type = "NodeBase", float x = 0.0f, float y = 0.0f)
    {
        return AddNode(nodePrefabs[type].Value, x, y);
    }

    public GameObject AddNode(GameObject copy, float x = 0.0f, float y = 0.0f)
    {
        GameObject nodeObject = Instantiate(copy, elementContainer.transform);

        nodeObject.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(x, y, 0.0f), Quaternion.identity);

        return nodeObject;
    }

    public void LinkCurrentlySelectedObjects()
    {
        linkingNodes = false;

        // Stop here if we're linking nodes that are already connected the same way
        if(linkingNodeMode == LinkingMode.NextNode)
        {
            if(linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject == linkingNodesObjects[1])
            {
                linkingNodesObjects[0] = linkingNodesObjects[1] = null;
                return;
            }
        }
        else if(linkingNodeMode == LinkingMode.FirstBodyNode)
        {
            /*NodeBase prevNode = (NodeBase)(linkingNodesObjects[1].GetComponent<NodeBase>().prevNode);
            if (prevNode != null && prevNode == linkingNodesObjects[1].GetComponent<LogicalBlock>())
            {
                linkingNodesObjects[0] = linkingNodesObjects[1] = null;
                return;
            }*/
        }

        // Cancel linking by linking the same node: removes nextNode from obj0 (watch out for ProgramEnd as that won't have a NextNodeObject)
        if(linkingNodesObjects[0] == linkingNodesObjects[1] && !linkingNodesObjects[0].GetComponent<ProgramEnd>())
        {
            if (linkingNodeMode == LinkingMode.NextNode)
            {
                if (linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject)
                {
                    linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject.GetComponent<NodeBase>().PrevNodeObject = null;
                    linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject.GetComponent<NodeBase>().prevNode = null;
                }
                linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject = null;
                linkingNodesObjects[0].GetComponent<NodeBase>().nextNode = null;
            }
            else
            {
                if (linkingNodesObjects[0].GetComponent<CodeBlock>().FirstBodyNodeObject)
                {
                    linkingNodesObjects[0].GetComponent<CodeBlock>().FirstBodyNodeObject.GetComponent<NodeBase>().PrevNodeObject = null;
                    linkingNodesObjects[0].GetComponent<CodeBlock>().FirstBodyNodeObject.GetComponent<NodeBase>().prevNode = null;
                    linkingNodesObjects[0].GetComponent<CodeBlock>().FirstBodyNodeObject.GetComponent<NodeBase>().ownerLoop = null;
                }
                linkingNodesObjects[0].GetComponent<CodeBlock>().FirstBodyNodeObject = null;
                linkingNodesObjects[0].GetComponent<CodeBlock>().firstBodyNode = null;
            }
            linkingNodesObjects[0] = linkingNodesObjects[1] = null;
            return;
        }

        // Prevent ProgramEnd being used as a prevNode and ProgramStart being used as nextNode
        if(linkingNodesObjects[0].GetComponent<ProgramEnd>() || linkingNodesObjects[1].GetComponent<ProgramStart>())
        {
            linkingNodesObjects[0] = linkingNodesObjects[1] = null;
            return;
        }

        if (linkingNodeMode == LinkingMode.NextNode)
        {
            // Prevent connecting to nextNode that's also the prevNode
            if (linkingNodesObjects[1] == linkingNodesObjects[0].GetComponent<NodeBase>().PrevNodeObject)
            {
                linkingNodesObjects[0] = linkingNodesObjects[1] = null;
                return;
            }
            GameObject nextObj = linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject;
            NodeBase nextObjNode = null;
            if (nextObj != null)
                nextObjNode = nextObj.GetComponent<NodeBase>();

            // obj0 is no longer its former nextNode's prevNode
            if (nextObjNode != null && nextObjNode.PrevNodeObject != null && nextObjNode.PrevNodeObject == linkingNodesObjects[0])
            {
                nextObjNode.PrevNodeObject.GetComponent<NodeBase>().NextNodeObject = null;
                nextObjNode.PrevNodeObject.GetComponent<NodeBase>().nextNode = null;
                nextObjNode.PrevNodeObject = null;
            }
            nextObj = linkingNodesObjects[1];
            nextObjNode = nextObj.GetComponent<NodeBase>();
            // assign obj0 as obj1's prevNode, unassigning obj1's former prevNode
            if (nextObjNode != null && nextObjNode.PrevNodeObject != null && nextObjNode.PrevNodeObject != linkingNodesObjects[0])
            {
                nextObjNode.PrevNodeObject.GetComponent<NodeBase>().NextNodeObject = null;
                nextObjNode.PrevNodeObject.GetComponent<NodeBase>().nextNode = null;
                nextObjNode.PrevNodeObject = linkingNodesObjects[0];
                linkingNodesObjects[0].GetComponent<NodeBase>().NextNodeObject = linkingNodesObjects[1];
            }

            nextObjNode.GetComponent<NodeBase>().PrevNodeObject = linkingNodesObjects[0];
            nextObjNode.GetComponent<NodeBase>().PrevNodeObject.GetComponent<NodeBase>().NextNodeObject = linkingNodesObjects[1];
        }
        else if(linkingNodeMode == LinkingMode.FirstBodyNode)
        {
            // failsafe
            if (linkingNodesObjects[0].GetComponent<CodeBlock>() == null)
            {
                linkingNodes = false;
                linkingNodesObjects[0] = linkingNodesObjects[1] = null;
                return;
            }
            else
            {
                linkingNodesObjects[0].GetComponent<CodeBlock>().FirstBodyNodeObject = linkingNodesObjects[1];
                linkingNodesObjects[1].GetComponent<NodeBase>().PrevNodeObject = linkingNodesObjects[0];
            }
        }
        if (linkingNodesObjects[1].GetComponent<NodeBase>())
        {
            if (linkingNodeMode == LinkingMode.NextNode)
            {
                linkingNodesObjects[0].GetComponent<NodeBase>().nextNode = linkingNodesObjects[1].GetComponent<NodeBase>();
                linkingNodesObjects[1].GetComponent<NodeBase>().prevNode = linkingNodesObjects[0].GetComponent<NodeBase>();
            }
            else if(linkingNodeMode == LinkingMode.FirstBodyNode)
            {
                linkingNodesObjects[0].GetComponent<CodeBlock>().firstBodyNode = linkingNodesObjects[1].GetComponent<NodeBase>();
                linkingNodesObjects[1].GetComponent<NodeBase>().prevNode = linkingNodesObjects[0].GetComponent<NodeBase>();
            }

            // Assign firstbody loop/logicalblock ownership
            if (linkingNodesObjects[0].GetComponent<LogicalBlock>() != null && linkingNodeMode == LinkingMode.FirstBodyNode)
            {
                linkingNodesObjects[1].GetComponent<NodeBase>().ownerLoop = linkingNodesObjects[0].GetComponent<LogicalBlock>();
            }

            // Propagate loop/logicalblock ownership
            if (linkingNodesObjects[0].GetComponent<NodeBase>().ownerLoop != null && linkingNodeMode == LinkingMode.NextNode)
            {
                linkingNodesObjects[1].GetComponent<NodeBase>().ownerLoop = linkingNodesObjects[0].GetComponent<NodeBase>().ownerLoop;
            }
        }
        linkingNodesObjects[0] = linkingNodesObjects[1] = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        clueHud = GameObject.Find("ClueHUD").GetComponent<ClueHUD>();

        // Load editor node prefabs from Resources
        if(nodePrefabs == null)
        {
            nodePrefabs = new Dictionary<string, KeyValuePair<string, GameObject>>();
            nodePrefabs.Add("AssignValue", new KeyValuePair<string, GameObject>("Set variable: Assigns a value to a variable. The variable is created if it doesn't exist.", Resources.Load("Prefabs/ProgramEditor/Nodes/Operations/AssignValue") as GameObject));
            nodePrefabs.Add("CreateList", new KeyValuePair<string, GameObject>("Create List: Initialises a named list with a specific size.", Resources.Load("Prefabs/ProgramEditor/Nodes/Operations/CreateList") as GameObject));
            nodePrefabs.Add("FunctionCallBase", new KeyValuePair<string, GameObject>("Function call: Triggers the specified function. Can pass parameters.", Resources.Load("Prefabs/ProgramEditor/Nodes/Operations/FunctionCall") as GameObject));

            // ArithmeticOperationBase uses ArithmeticAdd as a default.
            //nodePrefabs.Add("ArithmeticOperationBase", new KeyValuePair<string, GameObject>("Arithmetic: Performs arithmetic math operations.", Resources.Load("Prefabs/ProgramEditor/Nodes/Operations/ArithmeticAdd") as GameObject));

            nodePrefabs.Add("LogicalBlock", new KeyValuePair<string, GameObject>("If Statement: Runs a block of code if a condition is met.", Resources.Load("Prefabs/ProgramEditor/Nodes/IfStatement") as GameObject));
            nodePrefabs.Add("WhileLoop", new KeyValuePair<string, GameObject>("While loop: Repeats a block of code as long as condition is met.", Resources.Load("Prefabs/ProgramEditor/Nodes/WhileLoop") as GameObject));

            nodePrefabs.Add("Continue", new KeyValuePair<string, GameObject>("Continue: Moves to the next iteration of the loop without waiting for the current one to finish.", Resources.Load("Prefabs/ProgramEditor/Nodes/Continue") as GameObject));
        }

        if(!lineCanvas)
            lineCanvas = transform.Find("LineCanvas").GetComponent<Canvas>();

        if(!lineMaterial)
            lineMaterial = Resources.Load("Materials/LineMaterial") as Material;

        if (!programStart)
            programStart = elementContainer.GetComponentInChildren<ProgramStart>();

        if (!programEnd)
            programEnd = elementContainer.GetComponentInChildren<ProgramEnd>();

        linkingPreviewNode = new GameObject("previewNode", new Type[] { typeof(RectTransform), typeof(EditorDraggableNode) });
        linkingPreviewNode.transform.parent = elementContainer.transform;
        linkingPreviewNode.GetComponent<EditorDraggableNode>().allowDrag = false;
        linkingPreviewNode.GetComponent<EditorDraggableNode>().isArithmeticOperator = false;

        if (!enableEditorOnStartup)
        {
            DisableEditor();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Lock player movement if in editor
        if (GameObject.Find("Player"))
        {
            GameObject.Find("Player").GetComponent<FPPControl>().allowMove = !EditorActive;
        }

        DrawNodeLinks();

        // Editor creation occurs at startup so we may need a couple of frames to disable it for good measure?
        // (not sure how Unity's script execution order works in detail...)
        if (frameCounter < framesToDisable)
        {
            if (!enableEditorOnStartup)
            {
                DisableEditor();
            }
            else
            {
                EnableEditor();
            }
            frameCounter++;
        }

        if(Input.GetKeyUp(KeyCode.Tab) && EditorActive && editorMode == EditorMode.FlowChart)
        {
            choosingNode = !choosingNode;
            if(choosingNode)
            {
                newNodeInitPos = Input.mousePosition;
                clueHud.SetCurrentPrompt(null, null);
            }
            choosingFunctionCall = choosingNode && choosingFunctionCall;
        }

        if (editingNodeProperty)
        {
            /*if (Input.GetKeyDown(KeyCode.Escape))
            {
                editingNodeFinishedClb.DynamicInvoke(editingNodeValue);
                editingNodeProperty = false;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                editingNodeFinishedClb.DynamicInvoke(editedNodeValue);
                editingNodeProperty = false;
            }*/
        }
        else
        {
            // Toggle editor mode
            if(Input.GetKeyDown(KeyCode.F1))
            {
                SwitchMode(1 - editorMode);
            }
        }

        // Delete the linking preview line if we're not in linking mode
        if (linkingPreviewLine != null && !linkingNodes)
        {
            Destroy(linkingPreviewLine);
        }
    }

    private Vector2 ndcToScreen(Vector2 ndc)
    {
        return new Vector2(ndc.x * Screen.width, ndc.y * Screen.height);
    }

    private Vector2 ndcToScreen(float x, float y)
    {
        return ndcToScreen(new Vector2(x, y));
    }

    private void OnGUI()
    {
        GUI.skin = guiSkin;

        if (choosingNode)
        {
            GUI.Box(new Rect(ndcToScreen(0.5f - 0.75f / 2.0f, 0.25f), ndcToScreen(0.75f, 0.5f)), !choosingFunctionCall ? "Choose a node type to add:" : "Choose function to call");

            if (!choosingFunctionCall)
            {
                // TODO: This could just be reduced to one foreach with a counter variable, instead of a foreach & for-loop. Potential performance boost.
                List<string> nodeTypeStrings = new List<string>();
                foreach (string nodeTypeString in nodePrefabs.Keys)
                {
                    nodeTypeStrings.Add(nodeTypeString);
                }

                for (int i = 0; i < nodeTypeStrings.Count; i++)
                {
                    float buttonHeight = 0.05f;
                    float buttonWidth = 0.7f;
                    bool pressed = GUI.Button(new Rect(ndcToScreen(0.5f - buttonWidth / 2.0f, 0.3f + buttonHeight * i + 0.01f * i), ndcToScreen(buttonWidth, buttonHeight)), nodePrefabs[nodeTypeStrings[i]].Key);
                    if (pressed)
                    {
                        addedNode = AddNode(nodeTypeStrings[i], newNodeInitPos.x, newNodeInitPos.y);
                        if (nodeTypeStrings[i] == "FunctionCallBase")
                        {
                            choosingFunctionCall = true;
                        }
                        else
                        {
                            choosingNode = false;
                        }
                        // Fixes node link renderer
                        addedNode.name = addedNode.name.Replace("(Clone)", $"-{(addedNode.GetInstanceID())}");

                        // Special cases
                        if(nodeTypeStrings[i] == "CreateList")
                        {
                            addedNode.GetComponentInParent<AllocateArray>().InitialiseNode();
                        }
                        break;
                    }
                }
            }
            if(choosingFunctionCall)
            {
                List<string> functionNames = new List<string>();
                foreach (string functionName in programController.functions.Keys)
                {
                    if(programController.hiddenFunctions.Contains(functionName))
                    {
                        continue;
                    }

                    string paramList = "";
                    foreach(System.Reflection.ParameterInfo funcParams in programController.functions[functionName].Method.GetParameters())
                    {
                        paramList += $"{funcParams.Name}, ";
                    }
                    if (!string.IsNullOrWhiteSpace(paramList))
                    {
                        paramList = paramList.Substring(0, paramList.LastIndexOf(','));
                    }
                    functionNames.Add($"{functionName}({paramList})");
                }
                functionNames.Add("Other: type function name manually.");

                for (int i = 0; i < functionNames.Count; i++)
                {
                    float buttonHeight = 0.05f;
                    float buttonWidth = 0.7f;
                    bool pressed = GUI.Button(new Rect(ndcToScreen(0.5f - buttonWidth / 2.0f, 0.3f + buttonHeight * (float)i + 0.01f * (float)i), ndcToScreen(buttonWidth, buttonHeight)), functionNames[i]);
                    if (pressed)
                    {
                        // Last functionName is always the manually typed function
                        if (i != functionNames.Count - 1)
                        {
                            addedNode.GetComponent<EditorDraggableNode>().FunctionNameEditingFinished(functionNames[i].Substring(0, functionNames[i].IndexOf('(')));
                        }
                        else
                        {
                            addedNode.GetComponent<EditorDraggableNode>().FunctionNameEditingFinished("");
                        }
                        choosingNode = false;
                        choosingFunctionCall = false;
                        break;
                    }
                }
            }
        }

        if (editingNodeProperty)
        {
            GUI.SetNextControlName("editedNodeValue");
            if (editingNodeInPlace)
            {
                float x = editingNodeInPlaceRect.x;
                float y = editingNodeInPlaceRect.y;
                float width = editingNodeInPlaceRect.width;
                float height = editingNodeInPlaceRect.height;
                float cvsHeight = elementContainer.GetComponentInParent<RectTransform>().rect.height;
                editedNodeValue = GUI.TextArea(new Rect(x, Screen.height - (y + height), width, height), editedNodeValue);
            }
            else
            {
                editedNodeValue = GUI.TextArea(new Rect(ndcToScreen(0.5f - 0.375f / 2.0f, 0.5f - 0.05f / 2.0f), ndcToScreen(0.375f, 0.05f)), editedNodeValue);
            }

            if (!editedNodeFocused)
            {
                GUI.FocusControl("editedNodeValue");
                editedNodeFocused = true;
            }
            else if(string.IsNullOrWhiteSpace(GUI.GetNameOfFocusedControl()))
            {
                editingNodeProperty = false;
                editedNodeFocused = false;
                editingNodeInPlace = false;

                clueHud.SetCurrentPrompt(null, null);
            }

            if(editedNodeValue != null && editedNodeValue.Contains("\n"))
            {
                editedNodeValue = editedNodeValue.Replace("\n", "");
                editingNodeFinishedClb.DynamicInvoke(editedNodeValue);
                editingNodeProperty = false;
                editingNodeInPlace = false;
                editedNodeFocused = false;

                clueHud.SetCurrentPrompt(null, null);
            }
        }
    }

    void DrawNodeLinks()
    {
        List<GameObject> elements = new List<GameObject>();
        for(int childIndex = 0; childIndex < transform.Find("Canvas").Find("Elements").childCount; childIndex++)
        {
            elements.Add(transform.Find("Canvas").Find("Elements").GetChild(childIndex).gameObject);
        }
        /*LineRenderer renderer = lineCanvas.transform.Find("NextNodeLines").GetComponent<LineRenderer>();
        renderer.SetPositions(new List<Vector3>().ToArray());
        renderer.positionCount = 0;
        if (elements.Count > 2)
        {
            GameObject currentObject = elements[0];
            {
                while (currentObject.GetComponent<NodeBase>() != null && currentObject.GetComponent<NodeBase>().NextNodeObject != null)
                {
                    renderer.positionCount += 2;
                    renderer.SetPosition(renderer.positionCount - 2, currentObject.GetComponent<RectTransform>().localPosition);
                    renderer.SetPosition(renderer.positionCount - 1, currentObject.GetComponent<NodeBase>().NextNodeObject.GetComponent<RectTransform>().localPosition);

                    currentObject = currentObject.GetComponent<NodeBase>().NextNodeObject;
                }
            }
        }*/

        // Names of valid link GameObjects
        List<string> validLinks = new List<string>();

        // First, render the next-node links
        foreach (GameObject element in elements)
        {
            NodeBase potentialNode = element.GetComponent<NodeBase>();
            GameObject potentialNextNodeObj = potentialNode ? potentialNode.NextNodeObject : null;
            if (linkingNodes && linkingNodeMode == LinkingMode.NextNode && element.name == linkingNodesObjects[0].name)
            {
                potentialNextNodeObj = linkingPreviewNode;
                //Logger.Log("Found preview node");
            }
            if (potentialNode && potentialNextNodeObj)
            {
                GameObject currentLink = null;
                LineRenderer currentRenderer = null;
                Transform linkTransform = lineCanvas.transform.Find($"nextNode:{potentialNode.name}->{potentialNextNodeObj.name}");
                if (linkTransform)
                {
                    currentLink = linkTransform.gameObject;
                    currentRenderer = currentLink.GetComponent<LineRenderer>();

                    currentLink.GetComponent<LinkDescriptor>().prev = potentialNode.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialNextNodeObj;
                }
                else
                {
                    currentLink = new GameObject($"nextNode:{potentialNode.name}->{potentialNextNodeObj.name}");
                    currentLink.transform.SetParent(lineCanvas.transform, false);
                    currentRenderer = currentLink.AddComponent<LineRenderer>();
                    currentRenderer.material = lineMaterial;
                    currentRenderer.useWorldSpace = false;

                    currentLink.AddComponent<LinkDescriptor>();
                    currentLink.GetComponent<LinkDescriptor>().prev = potentialNode.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialNextNodeObj;
                }
                // This is to prevent culling from other 3D elements in the scene
                currentRenderer.gameObject.layer = LayerMask.NameToLayer("CodeEditor");
                Rect    thisRect = potentialNode.GetComponent<RectTransform>().rect,
                        nextRect = potentialNextNodeObj.GetComponent<RectTransform>().rect;
                Vector2 thisPos = new Vector2(potentialNode.GetComponent<RectTransform>().localPosition.x + thisRect.width / 2, potentialNode.GetComponent<RectTransform>().localPosition.y);
                Vector2 nextPos = new Vector2(potentialNextNodeObj.GetComponent<RectTransform>().localPosition.x - nextRect.width / 2, potentialNextNodeObj.GetComponent<RectTransform>().localPosition.y);

                // If we're rendering link preview, make it point to the rect center so that it aligns with the cursor
                if (potentialNextNodeObj.name == "previewNode")
                {
                    nextPos.x += nextRect.width / 2.0f;
                }

                currentRenderer.positionCount = 2;
                currentRenderer.SetPositions(new Vector3[] {
                    thisPos,
                    nextPos
                });

                validLinks.Add(currentLink.name);
            }
        }

        // Now create separate LineRenderers for FirstBodyNodeObjects
        foreach (GameObject element in elements)
        {
            CodeBlock potentialCodeBlock = element.GetComponent<CodeBlock>();
            GameObject potentialFirstBodyObj = potentialCodeBlock ? potentialCodeBlock.FirstBodyNodeObject : null;
            if (linkingNodes && linkingNodeMode == LinkingMode.FirstBodyNode && element.name == linkingNodesObjects[0].name)
            {
                potentialFirstBodyObj = linkingPreviewNode;
            }
            if (potentialCodeBlock && potentialFirstBodyObj)
            {
                GameObject currentLink = null;
                LineRenderer currentRenderer = null;
                Transform linkTransform = lineCanvas.transform.Find($"firstBody:{potentialCodeBlock.name}->{potentialFirstBodyObj.name}");
                if (linkTransform)
                {
                    currentLink = linkTransform.gameObject;
                    currentRenderer = currentLink.GetComponent<LineRenderer>();

                    currentLink.GetComponent<LinkDescriptor>().prev = potentialCodeBlock.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialFirstBodyObj;
                }
                else
                {
                    currentLink = new GameObject($"firstBody:{potentialCodeBlock.name}->{potentialFirstBodyObj.name}");
                    currentLink.transform.SetParent(lineCanvas.transform, false);
                    currentRenderer = currentLink.AddComponent<LineRenderer>();
                    currentRenderer.material = lineMaterial;
                    currentRenderer.useWorldSpace = false;

                    currentLink.AddComponent<LinkDescriptor>();
                    currentLink.GetComponent<LinkDescriptor>().prev = potentialCodeBlock.gameObject;
                    currentLink.GetComponent<LinkDescriptor>().next = potentialFirstBodyObj;
                }

                currentRenderer.positionCount = 2;
                currentRenderer.SetPositions(new Vector3[] {
                    potentialCodeBlock.GetComponent<RectTransform>().localPosition + new Vector3(.0f, potentialCodeBlock.GetComponent<RectTransform>().rect.height / -2.0f),
                    potentialFirstBodyObj.GetComponent<RectTransform>().localPosition + new Vector3(potentialFirstBodyObj.GetComponent<RectTransform>().rect.width / -2.0f, 0.0f)
                });

                // If we're rendering link preview, make it point to the rect center so that it aligns with the cursor
                if (potentialFirstBodyObj.name == "previewNode")
                {
                    currentRenderer.SetPosition(1, potentialFirstBodyObj.GetComponent<RectTransform>().localPosition);
                }

                // This is to prevent culling from other 3D elements in the scene
                currentRenderer.gameObject.layer = LayerMask.NameToLayer("CodeEditor");
                validLinks.Add(currentLink.name);

                /*GameObject currentObject = potentialCodeBlock.FirstBodyNodeObject;
                {
                    while (currentObject.GetComponent<NodeBase>() != null && currentObject.GetComponent<NodeBase>().NextNodeObject != null)
                    {
                        currentRenderer.positionCount += 2;
                        currentRenderer.SetPosition(renderer.positionCount - 2, currentObject.GetComponent<RectTransform>().localPosition);
                        currentRenderer.SetPosition(renderer.positionCount - 1, currentObject.GetComponent<NodeBase>().NextNodeObject.GetComponent<RectTransform>().localPosition);

                        currentObject = currentObject.GetComponent<NodeBase>().NextNodeObject;
                    }
                }*/
            }
        }

        // Lastly, remove any lines that are not needed (e.g after deleting a node)
        Transform[] allLines = lineCanvas.GetComponentsInChildren<Transform>();
        foreach (Transform line in allLines)
        {
            if(!validLinks.Contains(line.name) && line.name != "LineCanvas" && !line.name.Contains("previewNode"))
            {
                Destroy(line.gameObject);
            }
        }

        FixLines();
    }

    // Introduce 90-degree angle vertices to the lines if they're not fully straight
    void FixLines()
    {
        // Straight line tolerance expressed as maximum vertical offset between two vertices of the line
        const float verticalTolerance = 10.0f;

        const float connectorPadding = 20.0f;

        LineRenderer[] lines = lineCanvas.GetComponentsInChildren<LineRenderer>();
        foreach(LineRenderer line in lines)
        {
            LinkDescriptor linkDesc = line.GetComponent<LinkDescriptor>();
            Vector2 finalPoint = line.GetPosition(1);

            if (Mathf.Abs(line.GetPosition(0).y - finalPoint.y) > verticalTolerance)
            {
                RectTransform prevTransform = linkDesc.prev.GetComponent<RectTransform>(), nextTransform = linkDesc.next.GetComponent<RectTransform>();
                Rect prevRect = prevTransform.rect, nextRect = nextTransform.rect;
                bool isFirstBodyNodeLink = (linkDesc.prev.GetComponent<CodeBlock>() && linkDesc.prev.GetComponent<CodeBlock>().FirstBodyNodeObject == linkDesc.next.gameObject) || (linkingNodes && linkingNodeMode == LinkingMode.FirstBodyNode && prevTransform.gameObject == linkingNodesObjects[0]);
                if (prevTransform.localPosition.x + prevRect.width + connectorPadding < nextTransform.localPosition.x)
                {
                    line.positionCount = 4 - (isFirstBodyNodeLink ? 1 : 0);
                    int i = 1;
                    if (isFirstBodyNodeLink)
                    {
                        line.SetPosition(i++, new Vector2((prevTransform.localPosition.x), finalPoint.y));
                    }
                    else
                    {
                        line.SetPosition(i++, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f) - connectorPadding, line.GetPosition(0).y));
                        line.SetPosition(i++, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f) - connectorPadding, finalPoint.y));
                    }

                    // If we're rendering link preview, make it point to the rect center so that it aligns with the cursor
                    if (nextTransform.name == "previewNode")
                    {
                        line.SetPosition(i++, new Vector2(nextTransform.localPosition.x, finalPoint.y));
                    }
                    else
                    {
                        line.SetPosition(i++, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f), finalPoint.y));
                    }
                }
                else
                {
                    line.positionCount = 6 - (isFirstBodyNodeLink ? 1 : 0);
                    int i = 1;
                    if (isFirstBodyNodeLink)
                    {
                        line.SetPosition(i++, new Vector2((prevTransform.localPosition.x), (prevTransform.localPosition.y + nextTransform.localPosition.y) / 2.0f));
                    }
                    else
                    {
                        line.SetPosition(i++, new Vector2((prevTransform.localPosition.x + prevRect.width / 2.0f) + connectorPadding, line.GetPosition(0).y));
                        line.SetPosition(i++, new Vector2((prevTransform.localPosition.x + prevRect.width / 2.0f) + connectorPadding, (prevTransform.localPosition.y + nextTransform.localPosition.y) / 2.0f));
                    }
                    line.SetPosition(i++, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f) - connectorPadding, (prevTransform.localPosition.y + nextTransform.localPosition.y) / 2.0f));
                    line.SetPosition(i++, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f) - connectorPadding, finalPoint.y));

                    // If we're rendering link preview, make it point to the rect center so that it aligns with the cursor
                    if (nextTransform.name == "previewNode")
                    {
                        line.SetPosition(i++, new Vector2(nextTransform.localPosition.x, finalPoint.y));
                    }
                    else
                    {
                        line.SetPosition(i++, new Vector2((nextTransform.localPosition.x - nextRect.width / 2.0f), finalPoint.y));
                    }
                }
            }
        }
    }
}
