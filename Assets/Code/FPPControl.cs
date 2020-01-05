using System;
using UnityEngine;

public class FPPControl : ControlBase
{
    private Rigidbody playerRigidbody;
    private Camera playerCamera;

    public float strafeSpeed = 5.0f;
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 7.5f;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        GameObject.Destroy(GameObject.Find("Main Camera"));
        GameObject parentObj = transform.parent.gameObject;
        transform.parent = null;
        GameObject.Destroy(parentObj);
    }

    // Process character movement input
    protected override void KeyboardMove()
    {
        transform.Translate(new Vector3(Input.GetAxis(hAxis) * strafeSpeed * Time.deltaTime, 0.0f, Input.GetAxis(vAxis) * (Input.GetButton(sprintBtn) ? sprintSpeed : walkSpeed) * Time.deltaTime));
    }

    // Process camera mouse input
    protected override void MouseLook()
    {
        // Rotate the entire player game object on the Y-axis (yaw/left-right)
        transform.Rotate(new Vector3(0.0f, Input.GetAxis(mAxisX)));

        // Rotate ONLY the camera's X-axis (pitch/up-down)
        playerCamera.transform.Rotate(new Vector3(-Input.GetAxis(mAxisY), 0.0f));

        // We constrict the pitch to 80 degrees (safe bet) on each side.
        CapCamera();

        // Rotating the camera with euler angles sometimes induces rotation on the Z-axis (roll).
        // Here we reset that value to 0.0, making sure it stays level, while keeping the benefits of the built-in .Rotate() function.
        Vector3 oldRot = playerCamera.transform.eulerAngles;
        playerCamera.transform.eulerAngles = new Vector3(oldRot.x, oldRot.y, 0.0f);
    }

    protected void CapCamera()
    {
        Vector3 oldRot = playerCamera.transform.localEulerAngles;

        // This is a very rough estimate that the player will not be able to reach more than 160 degrees in a single frame
        if(oldRot.x < camPitchCapNeg && oldRot.x >= camPitchCap * 2)
        {
            oldRot.x = camPitchCapNeg;
        }
        else if(oldRot.x > camPitchCap && oldRot.x <= camPitchCapNeg)
        {
            oldRot.x = camPitchCap;
        }

        playerCamera.transform.localEulerAngles = oldRot;
    }
}
