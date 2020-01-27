using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Programmable : MonoBehaviour
{
    // Object containing program code to control this Programmable.
    public GameObject Computer;

    // Element (if the ProgramController addresses an array of objects)
    public int index = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Computer.GetComponent<ProgramController>().ExecuteFrame(index, gameObject);
    }
}
