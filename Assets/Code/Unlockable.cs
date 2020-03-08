using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for doors with keypads, terminals etc.
abstract public class Unlockable : Interactable
{
    public bool isUnlocked = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public abstract void Unlock();

    public abstract void Lock();
}
