using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditorDraggableNode : MonoBehaviour
{
    protected RectTransform rectTransform;
    protected Selectable container;

    protected bool isDragged = false;
    protected Vector2 anchorPoint;

    protected Vector2 lastFramePointer;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Drag(Vector2 input)
    {
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

        lastFramePointer = pointer;
    }
}
