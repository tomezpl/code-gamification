using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool interactionRadiusLock = true;
    public float interactionRadius = 2.5f;

    public bool DistanceCheck()
    {
        return Vector3.Distance(GameObject.Find("Player").transform.position, transform.position) <= interactionRadius;
    }
}
