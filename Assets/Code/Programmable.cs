﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Programmable : MonoBehaviour
{
    // Object containing program code to control this Programmable.
    public GameObject Computer;

    // Element (if the ProgramController addresses an array of objects)
    public int index = -1;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Computer.GetComponent<ProgramController>().ExecuteFrame(index, gameObject);

        if(GetComponentInChildren<TextMesh>())
        {
            GetComponentInChildren<TextMesh>().text = index.ToString();
        }
    }
}
