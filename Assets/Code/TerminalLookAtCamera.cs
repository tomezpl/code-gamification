using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalLookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    private Renderer renderer;
    void Start()
    {
        renderer = GetComponent<Renderer>();
        ImpersonateOverlay();
    }

    // Update is called once per frame
    void Update()
    {
        ImpersonateOverlay();
    }

    void ImpersonateOverlay()
    {
        Camera playerCam = GameObject.Find("Player").GetComponentInChildren<Camera>();

        Transform cam = playerCam.transform;

        renderer.enabled = !Physics.Raycast(cam.position, transform.position - cam.position, Vector3.Distance(cam.position, transform.position) + 0.1f);
        //Debug.DrawRay(cam.position, transform.position - cam.position);
        transform.eulerAngles = new Vector3(-90.0f + cam.eulerAngles.x, cam.eulerAngles.y, cam.eulerAngles.z);
    }
}
