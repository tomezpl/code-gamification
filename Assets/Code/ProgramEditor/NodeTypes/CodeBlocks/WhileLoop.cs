using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhileLoop : LogicalBlock
{
    // Should we break out of the loop? (Every time this is detected as true, it will be reset back to false after breaking, to allow the loop to be ran again later)
    public bool breakNow = false;
    public override string SerializeBlockHeader()
    {
        return $"while {condition.leftHand.Serialize()} {condition.comparison} {condition.rightHand.Serialize()}:";
    }

    public override void InitialiseNode()
    {
        base.InitialiseNode();

        transform.Find("ConditionBar").transform.Find("Text").GetComponent<Text>().text = "while";
    }
}
