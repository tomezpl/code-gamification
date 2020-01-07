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
        foreach(Component c in GetComponents<Component>())
        {
            Debug.Log(c);
        }
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
            Debug.Log(delta);
            rectTransform.Translate(delta);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pointer = Input.mousePosition;
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Debug.Log("FUCK");
            Drag(pointer);
        }
        lastFramePointer = pointer;
    }
}
