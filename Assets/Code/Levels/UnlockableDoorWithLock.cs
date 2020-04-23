using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableDoorWithLock : UnlockableDoor
{
    public enum LockType { BasicLock = 0, LogicGate, KeyCode, FizzBuzz };

    // For FizzBuzz puzzle: for how many numbers should the fizzbuzz game be played?
    public int fizzBuzzCount = 20;

    // For BasicLock: the door's current lock state (true = locked)
    protected bool basicLockState = true;

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
        basicLockState = !state;
    }
}
