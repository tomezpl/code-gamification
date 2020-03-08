using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanLauncher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"Triggered {other.name}");
        Rigidbody rb = other.GetComponent<Rigidbody>() ? other.GetComponent<Rigidbody>() : (other.GetComponentInParent<Rigidbody>() ? other.GetComponentInParent<Rigidbody>() : null);
        if (rb != null)
        {
            // Simulate wind pushing the object/player away by the fan
            rb.AddForce(transform.up * (float)GetComponentInParent<Fan>().speed);
        }
    }
}
