using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooleanSwitchTest : MonoBehaviour
{
    public Material offMaterialForCasing, offMaterialForInside, onMaterialForCasing, onMaterialForInside;
    private bool isEnabled = false;
    public GameObject insideBit, insideLight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
            ToggleBoolean();
    }

    void ToggleBoolean()
    {
        isEnabled = !isEnabled;

        if(!isEnabled)
        {
            this.GetComponent<MeshRenderer>().material = offMaterialForCasing;
            insideBit.GetComponent<MeshRenderer>().material = offMaterialForInside;
            insideLight.GetComponent<Light>().enabled = false;
        }

        if (isEnabled)
        {
            this.GetComponent<MeshRenderer>().material = onMaterialForCasing;
            insideBit.GetComponent<MeshRenderer>().material = onMaterialForInside;
            insideLight.GetComponent<Light>().enabled = true;
        }
    }
}
