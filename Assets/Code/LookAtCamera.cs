using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = GameObject.Find("Player").GetComponentInChildren<Camera>().transform.position;
        transform.LookAt(new Vector3(camPos.x, camPos.y, -camPos.z), Vector3.up);
        
    }
}
