using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhileLoop : CodeBlock
{
    [SerializeField]
    public BoolCondition condition;

    public GameObject ConditionalObject;

    public override string SerializeBlockHeader()
    {
        return $"while {condition.leftHand.Serialize()} {condition.comparison} {condition.rightHand.Serialize()}:";
    }

    public override void InitialiseNode()
    {
        base.InitialiseNode();

        {
            if (FirstBodyNodeObject)
            {
                NodeBase currentNode = FirstBodyNodeObject.GetComponent<NodeBase>() == null ? null : FirstBodyNodeObject.GetComponent<NodeBase>();
                while (currentNode != null)
                {
                    currentNode.inLoop = true;

                    NodeBase lastNode = currentNode;
                    if (currentNode.NextNodeObject != null)
                    {
                        currentNode = currentNode.NextNodeObject.GetComponent<NodeBase>();
                    }
                    else
                    {
                        break;
                    }

                    // Reinitialise node
                    currentNode.isInitialised = false;
                    currentNode.indentLevel -= 2; // Don't ask why - this just fixes tabulation.
                    lastNode.InitialiseNode();
                }
            }
        }

        if(ConditionalObject == null)
        {
            if(transform.Find("Conditional") != null)
            {
                ConditionalObject = transform.Find("Conditional").gameObject;
            }
        }

        transform.Find("Text").GetComponent<Text>().text = "while";
        if (ConditionalObject != null && condition != null)
        {
            ConditionalObject.transform.Find("LHReference").transform.Find("Text").GetComponent<Text>().text = condition.leftHand.Serialize();
            ConditionalObject.transform.Find("Comparison").transform.Find("Text").GetComponent<Text>().text = condition.comparison;
            ConditionalObject.transform.Find("RHReference").transform.Find("Text").GetComponent<Text>().text = condition.rightHand.Serialize();
        }
    }
}
