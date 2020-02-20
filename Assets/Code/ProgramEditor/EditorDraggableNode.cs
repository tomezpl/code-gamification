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

    public void DoubleClick()
    {
        if (transform.name == "FuncName")
        {
            DispatchEditingProperty(new System.Action<string>(FunctionNameEditingFinished), GetComponentInParent<FunctionCallBase>().functionName);
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

    public void FunctionNameEditingFinished(string finalName)
    {
        Debug.Log($"Got back '{finalName}'.");
        GetComponentInParent<FunctionCallBase>().functionName = finalName;
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

        // Other events:
        if(Input.GetKeyDown(KeyCode.Delete) && nodeRect.Contains(pointer))
        {
            GetComponent<NodeBase>().DeleteNode();
        }

        // Linking nodes
        // Check if this Draggable isn't part of another node.
        if (transform.parent == owner.elementContainer.transform)
        {
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
        }

        // Double-click: usually this is editing a node
        if(Input.GetKeyUp(KeyCode.Mouse0) && nodeRect.Contains(pointer))
        {
            clickCounter++;
            timeSinceClick = 0.0f;
        }
        if(clickCounter >= 2)
        {
            DoubleClick();
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
