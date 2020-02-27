using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This script can actually be used for clickables etc. anything interactive, this name is irrelevant anymore.
public class EditorDraggableNode : MonoBehaviour
{
    protected RectTransform rectTransform;
    protected Selectable container;

    protected bool isDragged = false;
    protected Vector2 anchorPoint;

    protected Vector2 lastFramePointer;

    public EditorProgram owner;

    public bool allowDrag = true;

    protected int clickCounter;
    protected float timeSinceClick;
    protected const float doubleClickAllowedTime = 1.0f;

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
    }

    public void Drag(Vector2 input)
    {
        if (!allowDrag || owner.editingNodeProperty)
            return;

        if (!isDragged)
        {
            lastFramePointer = input; // set last frame already to anchor point
            isDragged = true;
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
        return transform.childCount == 1 && GetComponentInChildren<Text>().name == "Text";
    }

    public void DoubleClick()
    {
        if (transform.name == "FuncName")
        {
            DispatchEditingProperty(new System.Action<string>(FunctionNameEditingFinished), GetComponentInParent<FunctionCallBase>().functionName);
        }
        else if (IsReference() || (transform.name.Contains("Parameter") && name != "Parameter"))
        {
            // TODO
            int paramIndex = -1;
            if(transform.name.Contains("Parameter") && name != "Parameter")
            {
                paramIndex = System.Convert.ToInt32(transform.name.Substring("Parameter".Length)) - 1;
            }
            if (paramIndex >= 0)
            {
                DispatchEditingProperty(new System.Action<string>(ParamEditingFinished), GetComponentInParent<FunctionCallBase>().parameters[paramIndex].Value);
            }
            else
            {
                DispatchEditingProperty(new System.Action<string>(ReferenceEditingFinished), GetComponentInChildren<Text>().text);
            }
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
        }
    }

    public void FunctionNameEditingFinished(string finalName)
    {
        Debug.Log($"Got back '{finalName}'.");
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

        //Debug.Log("Pointer: " + pointer);
        //Debug.Log("Node: " + nodeRect);

        // Drag if LMB held down and inside the node rectangle
        if (Input.GetKeyDown(KeyCode.Mouse0) && nodeRect.Contains(pointer) || isDragged)
        {
            Drag(pointer);
        }
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            isDragged = false;
        }

        // Check if this Draggable isn't part of another node.
        if (transform.parent == owner.elementContainer.transform)
        {
            // Linking nodes
            if (Input.GetKeyUp(KeyCode.Mouse1) && nodeRect.Contains(pointer))
            {
                if (!owner.linkingNodes)
                {
                    owner.linkingNodes = true;
                    owner.linkingNodesObjects[0] = gameObject;
                }
                else
                {
                    owner.linkingNodesObjects[1] = gameObject;
                    owner.LinkCurrentlySelectedObjects();
                }
            }

            // Other events: Deleting node
            if (Input.GetKeyDown(KeyCode.Delete) && nodeRect.Contains(pointer) && GetComponent<NodeBase>())
            {
                GetComponent<NodeBase>().DeleteNode();
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
}
