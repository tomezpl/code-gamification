using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorProgram : MonoBehaviour
{
    private Canvas lineCanvas;
    private static Material lineMaterial = null;

    public GameObject elementContainer;

    public ProgramStart programStart;

    public int framesToDisable = 2;

    int frameCounter = 0;
    
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

    void DisableEditor()
    {
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
    }

    void EnableEditor()
    {
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

    // Start is called before the first frame update
    void Start()
    {
        if(!lineCanvas)
            lineCanvas = transform.Find("LineCanvas").GetComponent<Canvas>();

        if(!lineMaterial)
            lineMaterial = Resources.Load("Materials/LineMaterial") as Material;

        if (!programStart)
            programStart = elementContainer.GetComponentInChildren<ProgramStart>();

        DisableEditor();
    }

    // Update is called once per frame
    void Update()
    {
        DrawNodeLinks();

        // Editor creation occurs at startup so we may need a couple of frames to disable it for good measure?
        if (frameCounter < framesToDisable)
        {
            DisableEditor();
            frameCounter++;
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
                // This is to prevent culling from other 3D elements in the scene
                currentRenderer.gameObject.layer = LayerMask.NameToLayer("CodeEditor");
                Rect    thisRect = potentialNode.GetComponent<RectTransform>().rect,
                        nextRect = potentialNode.NextNodeObject.GetComponent<RectTransform>().rect;
                Vector2 thisPos = new Vector2(potentialNode.GetComponent<RectTransform>().localPosition.x + thisRect.width / 2, potentialNode.GetComponent<RectTransform>().localPosition.y);
                Vector2 nextPos = new Vector2(potentialNode.NextNodeObject.GetComponent<RectTransform>().localPosition.x - nextRect.width / 2, potentialNode.NextNodeObject.GetComponent<RectTransform>().localPosition.y);
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
            if(!validLinks.Contains(line.name) && line.name != "LineCanvas")
            {
                Destroy(line.gameObject);
            }
        }

    }
}
