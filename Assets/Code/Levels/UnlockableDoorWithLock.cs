using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableDoorWithLock : UnlockableDoor
{
    public enum LockType { BasicLock = 0, LogicGate, KeyCode, FizzBuzz };

    protected bool basicLockState = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override bool CanUnlock()
    {
        return basicLockState; 
    }
}
