using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class ControlBase : MonoBehaviour
{

    protected const string hAxis = "Horizontal", vAxis = "Vertical", mAxisX = "Mouse X", mAxisY = "Mouse Y", sprintBtn = "Sprint";
    protected const float camPitchCap = 80.0f, camPitchCapNeg = 360.0f - camPitchCap;

    protected virtual void Update()
    {
        KeyboardMove();
        MouseLook();
    }

    // Process entity movement input
    protected abstract void KeyboardMove();

    // Process camera mouse input
    protected abstract void MouseLook();

}
