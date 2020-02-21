using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalLookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ImpersonateOverlay();
    }

    // Update is called once per frame
    void Update()
    {
        ImpersonateOverlay();
    }

    void ImpersonateOverlay()
    {
        Transform cam = GameObject.Find("Player").GetComponentInChildren<Camera>().transform;
        transform.eulerAngles = new Vector3(-90.0f + cam.eulerAngles.x, cam.eulerAngles.y, cam.eulerAngles.z);
    }
}
