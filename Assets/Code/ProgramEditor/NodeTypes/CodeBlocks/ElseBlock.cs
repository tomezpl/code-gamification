using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElseBlock : LogicalBlock
{
    public override string SerializeBlockHeader()
    {
        return "else:";
    }
}
