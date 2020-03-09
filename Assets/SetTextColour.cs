using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTextColour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.SetColor("_Color", GetComponent<TextMesh>().color);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
