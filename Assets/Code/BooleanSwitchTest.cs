using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooleanSwitchTest : MonoBehaviour
{
    public Material offMaterialForCasing, offMaterialForInside, onMaterialForCasing, onMaterialForInside;
    public bool isEnabled = false;
    public GameObject insideBit, insideLight;
    public float triggerRadius = 2.5f; // Distance at which the boolean switch can be triggered with the Spacebar

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && Vector3.Distance(GameObject.Find("Player").transform.position, transform.position) < triggerRadius)
            ToggleBoolean();
    }

    public void ToggleBoolean()
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
