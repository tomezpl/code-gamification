using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableDoorWithLock : UnlockableDoor
{
    public enum LockType { BasicLock = 0, LogicGate, KeyCode, FizzBuzz };

    protected bool basicLockState = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SetLock(basicLockState);
    }

    protected override bool CanUnlock()
    {
        return basicLockState; 
    }

    public void SetLock(bool state)
    {
        basicLockState = state;
    }
}
