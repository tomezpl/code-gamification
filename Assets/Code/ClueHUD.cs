using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueHUD : MonoBehaviour
{
    public GameObject FPPTerminalPrompts;

    public GameObject currentPromptSet = null;
    public GameObject currentPromptCaller = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePrompts(new List<GameObject> { FPPTerminalPrompts });
    }

    public void SetCurrentPrompt(GameObject promptSet, GameObject caller)
    {
        currentPromptSet = promptSet;
        currentPromptCaller = caller;
    }

    void UpdatePrompts(List<GameObject> promptSets)
    {
        foreach (GameObject promptSet in promptSets)
        {
            if (currentPromptSet != promptSet)
            {
                promptSet.SetActive(false);
            }
            else
            {
                promptSet.SetActive(true);
            }
        }
    }
}
