using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgramNode
{
    // Creates program code for this node.
    string Serialize();
}
