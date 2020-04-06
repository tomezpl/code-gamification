using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break : Continue
{
    public override string Serialize()
    {
        return "break";
    }
}
