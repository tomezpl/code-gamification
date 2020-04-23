using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// On-screen help prompts, dynamically adapting to the situation
public class ClueHUD : MonoBehaviour
{
    public GameObject FPPTerminalPrompts;
    public GameObject HoveredOverNodePrompt;
    public GameObject HoveredOverParamPrompt;

    public GameObject EditingParamHoveredPrompt;
    public GameObject EditingParamUnhoveredPrompt;

    public GameObject CopyNodePrompt;
    public GameObject CopyPasteNodePrompt;
    public GameObject PasteNodePrompt;

    // Node linking prompts
    public GameObject LinkNodePrompt;
    public Text LinkNodeCaption;

    public GameObject currentPromptSet = null;
    public GameObject currentPromptCaller = null;

    public GameObject alwaysOnFlowChartPrompt = null;
    public GameObject alwaysOnCodeViewerPrompt = null;

    public GameObject promptBackgroundLeft = null;
    public GameObject promptBackgroundRight = null;

    private EditorProgram[] editors;
    public GameObject hoveredNode;
    public EditorProgram.LinkingMode potentialLinkingMode;

    // Start is called before the first frame update
    void Start()
    {
        List<EditorProgram> editorsInScene = new List<EditorProgram>();
        foreach(GameObject obj in FindObjectsOfType<GameObject>())
        {
            if(obj.GetComponent<EditorProgram>())
            {
                editorsInScene.Add(obj.GetComponent<EditorProgram>());
            }
        }

        editors = editorsInScene.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePrompts(new List<GameObject> { FPPTerminalPrompts, HoveredOverNodePrompt, HoveredOverParamPrompt, EditingParamHoveredPrompt, EditingParamUnhoveredPrompt });
    }

    public void SetCurrentPrompt(GameObject promptSet, GameObject caller)
    {
        currentPromptSet = promptSet;
        currentPromptCaller = caller;
    }

    void UpdatePrompts(List<GameObject> promptSets)
    {
        EditorProgram currentEditor = null;

        alwaysOnFlowChartPrompt.SetActive(false);
        alwaysOnCodeViewerPrompt.SetActive(false);
        promptBackgroundLeft.SetActive(false);
        promptBackgroundRight.SetActive(false);
        foreach (EditorProgram editor in editors)
        {
            if (editor.EditorActive)
            {
                if (editor.editorMode == EditorProgram.EditorMode.FlowChart)
                {
                    alwaysOnFlowChartPrompt.SetActive(true);
                    promptBackgroundLeft.SetActive(true);
                }
                else
                {
                    alwaysOnCodeViewerPrompt.SetActive(true);
                }

                currentEditor = editor;
                break;
            }
        }

        if (currentEditor && currentEditor.EditorActive && !currentEditor.editingNodeProperty && !currentEditor.linkingNodes)
        {
            if (CopyPasteNodePrompt.activeInHierarchy || CopyNodePrompt.activeInHierarchy || PasteNodePrompt.activeInHierarchy)
            {
                promptBackgroundRight.SetActive(true);
            }
        }
        else
        {
            promptBackgroundRight.SetActive(false);
            CopyPasteNodePrompt.SetActive(false);
            CopyNodePrompt.SetActive(false);
            PasteNodePrompt.SetActive(false);
        }

        if(currentEditor && currentEditor.EditorActive && currentEditor.linkingNodes && hoveredNode)
        {
            if(currentEditor.linkingNodeMode == EditorProgram.LinkingMode.NextNode)
            {
                if(!hoveredNode.GetComponent<ProgramStart>())
                {
                    LinkNodeCaption.text = "Set as next node";
                    LinkNodePrompt.SetActive(true);
                }

                if(hoveredNode == currentEditor.linkingNodesObjects[0])
                {
                    LinkNodeCaption.text = "Remove link";
                    LinkNodePrompt.SetActive(true);
                }
            }
            else
            {
                if (!hoveredNode.GetComponent<ProgramStart>())
                {
                    LinkNodeCaption.text = "Set as first node";
                    LinkNodePrompt.SetActive(true);
                }

                if (hoveredNode == currentEditor.linkingNodesObjects[0])
                {
                    LinkNodeCaption.text = "Remove link";
                    LinkNodePrompt.SetActive(true);
                }
            }
        }
        else if(currentEditor && currentEditor.EditorActive && !currentEditor.linkingNodes && hoveredNode && !currentEditor.editingNodeProperty && !currentEditor.choosingNode && !currentEditor.choosingFunctionCall)
        {
            if (!hoveredNode.GetComponent<ProgramEnd>())
            {
                if (potentialLinkingMode == EditorProgram.LinkingMode.FirstBodyNode && hoveredNode.GetComponent<LogicalBlock>())
                {
                    LinkNodeCaption.text = hoveredNode.GetComponent<WhileLoop>() ? "Start loop" : (hoveredNode.GetComponent<ElseBlock>() ? "Start\nElse Block" : "Start\nIf statement");
                    LinkNodePrompt.SetActive(true);
                }
                else
                {
                    LinkNodeCaption.text = "Connect to\nnext node";
                    LinkNodePrompt.SetActive(true);
                }
            }
        }
        else
        {
            LinkNodePrompt.SetActive(false);
        }

        foreach (GameObject promptSet in promptSets)
        {
            if (currentPromptSet != promptSet)
            {
                promptSet.SetActive(false);
            }
            else
            {
                if (currentEditor == null || (currentEditor != null && (!currentEditor.choosingNode && !currentEditor.choosingFunctionCall)))
                {
                    promptSet.SetActive(true);
                }
            }
        }
    }
}
