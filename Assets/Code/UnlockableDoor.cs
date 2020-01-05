using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableDoor : Unlockable
{
    public float doorOpening = 0.25f;
    public float doorOpeningTime = 1.0f; // how long (in seconds) will it take to open the door?
    public float doorOpenDuration = 5.0f; // how long (in seconds) will the door be open for?

    private Vector3 originalPosL, originalPosR; // initial position of left and right door
    private Transform transformL, transformR;

    private float timeToLock = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name == "LDoor")
            {
                originalPosL = t.localPosition;
                transformL = t;
            }
            else if (t.gameObject.name == "RDoor")
            {
                originalPosR = t.localPosition;
                transformR = t;
            }
        }
        Debug.Log("LDoor original: " + originalPosL);
        Debug.Log("RDoor original: " + originalPosR);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeToLock <= 0.0f)
        {
            if (isUnlocked)
            {
                Debug.Log("Locking!");
                timeToLock = doorOpeningTime;
            }
            Lock();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isUnlocked)
        {
            Debug.Log("Unlocking!");
            timeToLock = doorOpenDuration + doorOpeningTime;
            Debug.Log("ttl: " + timeToLock);
            Unlock();
        }
        else if(isUnlocked)
        {
            timeToLock -= Time.deltaTime;
            Unlock();
        }
        else if(!isUnlocked)
        {
            timeToLock -= Time.deltaTime;
            Lock();
        }
    }

    public override void Unlock()
    {
        float t = doorOpeningTime - (timeToLock - doorOpenDuration);
        Debug.Log("t=" + t);
        t /= doorOpeningTime;
        Debug.Log("Step is " + t.ToString());
        isUnlocked = true;
        transformL.localPosition = Vector3.Lerp(transformL.localPosition, new Vector3(originalPosL.x - doorOpening, originalPosL.y, originalPosL.z), t);
        transformR.localPosition = Vector3.Lerp(transformR.localPosition, new Vector3(originalPosR.x + doorOpening, originalPosR.y, originalPosR.z), t);
    }

    public override void Lock()
    {
        float t = doorOpeningTime - timeToLock;
        Debug.Log("t=" + t);
        t /= doorOpeningTime;
        Debug.Log("Step is " + t.ToString());
        isUnlocked = false;
        transformL.localPosition = Vector3.Lerp(transformL.localPosition, originalPosL, t);
        transformR.localPosition = Vector3.Lerp(transformR.localPosition, originalPosR, t);
    }
}
