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

    private Transform playerTransform;

    private float timeToLock = 0.0f;

    // Start is called before the first frame update
    protected virtual void Start()
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
        //Logger.Log("LDoor original: " + originalPosL);
        //Logger.Log("RDoor original: " + originalPosR);

        playerTransform = GameObject.Find("Player").transform;
    }

    protected virtual bool CanUnlock()
    {
        return Input.GetKeyDown(KeyCode.Space) && !isUnlocked && DistanceCheck();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (timeToLock <= 0.0f || !CanUnlock())
        {
            if (isUnlocked)
            {
                //Logger.Log("Locking!");
                timeToLock = doorOpeningTime;
            }
            Lock();
        }
        if (CanUnlock() && !isUnlocked)
        {
            //Logger.Log("Unlocking!");
            timeToLock = doorOpenDuration + doorOpeningTime;
            //Logger.Log("ttl: " + timeToLock);
            Unlock();
        }
        else if(!CanUnlock() && isUnlocked)
        {
            Lock();
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
        //Logger.Log("t=" + t);
        t /= doorOpeningTime;
        //Logger.Log("Step is " + t.ToString());
        isUnlocked = true;
        transformL.localPosition = Vector3.Lerp(transformL.localPosition, new Vector3(originalPosL.x - doorOpening, originalPosL.y, originalPosL.z), t);
        transformR.localPosition = Vector3.Lerp(transformR.localPosition, new Vector3(originalPosR.x + doorOpening, originalPosR.y, originalPosR.z), t);
    }

    public override void Lock()
    {
        float t = doorOpeningTime - timeToLock;
        //Logger.Log("t=" + t);
        t /= doorOpeningTime;
        //Logger.Log("Step is " + t.ToString());
        isUnlocked = false;
        transformL.localPosition = Vector3.Lerp(transformL.localPosition, originalPosL, t);
        transformR.localPosition = Vector3.Lerp(transformR.localPosition, originalPosR, t);
    }
}
