using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhileLoop : LogicalBlock
{

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
