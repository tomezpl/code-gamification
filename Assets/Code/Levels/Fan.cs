using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : Programmable
{
    public double speed = 0.0;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        transform.Rotate(transform.up * (float)speed);
    }
}
