using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : Programmable
{
    // Lowest position relative to the platform's original position
    public float MinElevation = 0.0f;

    // Highest position relative to the platform's original position
    public float MaxElevation = 1000.0f;
}
