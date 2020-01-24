using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorProgram : MonoBehaviour
{
    private Canvas lineCanvas;
    private static Material lineMaterial = null;

    // Start is called before the first frame update
    void Start()
    {
        lineCanvas = transform.Find("LineCanvas").GetComponent<Canvas>();
        if(!lineMaterial)
            lineMaterial = Resources.Load("Materials/LineMaterial") as Material;
    }

    // Update is called once per frame
    void Update()
    {
        DrawNodeLinks();
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

        // First, render the next-node links
        foreach (GameObject element in elements)
        {
            NodeBase potentialNode = element.GetComponent<NodeBase>();
            if (potentialNode && potentialNode.NextNodeObject)
            {
                GameObject currentLink = null;
                LineRenderer currentRenderer = null;
                Transform cachedLinkTransform = lineCanvas.transform.Find($"nextNode:{potentialNode.name}->{potentialNode.NextNodeObject.name}");
                if (cachedLinkTransform)
                {
                    currentLink = cachedLinkTransform.gameObject;
                    currentRenderer = currentLink.GetComponent<LineRenderer>();
                }
                else
                {
                    currentLink = new GameObject($"nextNode:{potentialNode.name}->{potentialNode.NextNodeObject.name}");
                    currentLink.transform.SetParent(lineCanvas.transform, false);
                    currentRenderer = currentLink.AddComponent<LineRenderer>();
                    currentRenderer.material = lineMaterial;
                    currentRenderer.useWorldSpace = false;
                }
                Rect    thisRect = potentialNode.GetComponent<RectTransform>().rect,
                        nextRect = potentialNode.NextNodeObject.GetComponent<RectTransform>().rect;
                Vector2 thisPos = new Vector2(potentialNode.GetComponent<RectTransform>().localPosition.x + thisRect.width / 2, potentialNode.GetComponent<RectTransform>().localPosition.y);
                Vector2 nextPos = new Vector2(potentialNode.NextNodeObject.GetComponent<RectTransform>().localPosition.x - nextRect.width / 2, potentialNode.NextNodeObject.GetComponent<RectTransform>().localPosition.y);
                currentRenderer.SetPositions(new Vector3[] {
                    thisPos,
                    nextPos
                });
            }
        }

        // Now create separate LineRenderers for FirstBodyNodeObjects
        foreach (GameObject element in elements)
        {
            CodeBlock potentialCodeBlock = element.GetComponent<CodeBlock>();
            if (potentialCodeBlock && potentialCodeBlock.FirstBodyNodeObject)
            {
                GameObject currentLink = null;
                LineRenderer currentRenderer = null;
                Transform cachedLinkTransform = lineCanvas.transform.Find($"firstBody:{potentialCodeBlock.name}->{potentialCodeBlock.FirstBodyNodeObject.name}");
                if (cachedLinkTransform)
                {
                    currentLink = cachedLinkTransform.gameObject;
                    currentRenderer = currentLink.GetComponent<LineRenderer>();
                }
                else
                {
                    currentLink = new GameObject($"firstBody:{potentialCodeBlock.name}->{potentialCodeBlock.FirstBodyNodeObject.name}");
                    currentLink.transform.SetParent(lineCanvas.transform, false);
                    currentRenderer = currentLink.AddComponent<LineRenderer>();
                    currentRenderer.material = lineMaterial;
                    currentRenderer.useWorldSpace = false;
                }
                currentRenderer.SetPositions(new Vector3[] {
                    potentialCodeBlock.GetComponent<RectTransform>().localPosition + new Vector3(.0f, potentialCodeBlock.GetComponent<RectTransform>().rect.height / -2.0f),
                    potentialCodeBlock.FirstBodyNodeObject.GetComponent<RectTransform>().localPosition + new Vector3(potentialCodeBlock.FirstBodyNodeObject.GetComponent<RectTransform>().rect.width / -2.0f, 0.0f)
                });

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
    }
}
