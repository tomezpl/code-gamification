﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

abstract public class ControlBase : MonoBehaviour
{

    protected const string hAxis = "Horizontal", vAxis = "Vertical", mAxisX = "Mouse X", mAxisY = "Mouse Y", sprintBtn = "Sprint";
    protected const float camPitchCap = 80.0f, camPitchCapNeg = 360.0f - camPitchCap;

    public bool allowConsole = true;

    public bool allowMove = true;

    public bool consoleVisible = false;

    string enteredCommand = "";

    protected virtual void Update()
    {
        if (!consoleVisible && allowMove)
        {
            KeyboardMove();
            MouseLook();
        }
        DebugControls();

        foreach(GameObject obj in FindObjectsOfType<GameObject>())
        {
            if(obj.GetComponent<EditorProgram>())
            {
                if(obj.GetComponent<EditorProgram>().EditorActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }

    protected void DebugControls()
    {
        if(allowConsole)
        {
            if(Input.GetKeyUp(KeyCode.BackQuote))
            {
                consoleVisible = !consoleVisible;
                if (consoleVisible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else if (allowMove)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            if(consoleVisible)
            {
                if(Input.GetKeyUp(KeyCode.Return) || enteredCommand.Contains("\n"))
                {
                    switch (enteredCommand.Replace("\n", "").ToLower())
                    {
                        case "quit":
                            Application.Quit();
                            break;
                        case "restart":
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                            break;
                        case "stopall":
                            GameObject[] allObjects = FindObjectsOfType<GameObject>();
                            foreach(GameObject obj in allObjects)
                            {
                                ProgramController computer = obj.GetComponent<ProgramController>();
                                if(computer != null)
                                {
                                    computer.programRunning = false;
                                    computer.processingDone = true;
                                    computer.currentNode = computer.editorUi.GetComponent<EditorProgram>().elementContainer.GetComponentInChildren<ProgramStart>();
                                }
                            }
                            break;
                    }
                    if (enteredCommand.ToLower().Contains("load "))
                    {
                        int index = Convert.ToInt32(enteredCommand.Trim().Substring("load ".Length));
                        SceneManager.LoadScene(index);
                    }
                    if(enteredCommand.ToLower().Contains("section "))
                    {
                        int sectionIndex = Convert.ToInt32(enteredCommand.Trim().Substring("section ".Length));
                        transform.position = GameObject.Find($"Section{sectionIndex}").transform.position;
                    }
                    enteredCommand = "";
                    consoleVisible = false;
                }
            }
        }
    }

    void OnGUI()
    {
        if(consoleVisible)
        {
            GUI.SetNextControlName("debugconsole");
            enteredCommand = GUI.TextArea(new Rect(0, 0, Screen.width, 20.0f), enteredCommand);
            GUI.FocusControl("debugconsole");
            if(GUI.GetNameOfFocusedControl() != "debugconsole" || enteredCommand.Contains("`"))
            {
                consoleVisible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                enteredCommand = "";
            }
        }
    }

    // Process entity movement input
    protected abstract void KeyboardMove();

    // Process camera mouse input
    protected abstract void MouseLook();

}
