using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogicalBlock : CodeBlock
{
    [SerializeField]
    public BoolCondition condition;

    public GameObject ConditionalObject;

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
                    //currentNode.isInitialised = false;
                    currentNode.indentLevel -= 2; // Don't ask why - this just fixes tabulation.
                    //lastNode.InitialiseNode();
                }
            }
        }

        UpdateUI();
    }

    // Assign itself as ownerLoop to the entire block body
    public virtual void PropagateOwnership()
    {
        if(firstBodyNode != null)
        {
            NodeBase currentNode = (NodeBase)firstBodyNode;
            while(currentNode != null)
            {
                currentNode.ownerLoop = this;

                currentNode = (NodeBase)currentNode.nextNode;
            }
        }
    }

    // Similar to FunctionCallBase.UpdateFunctionParameters
    public virtual void UpdateUI()
    {
        if (ConditionalObject == null)
        {
            if (transform.Find("ConditionBar").transform.Find("Conditional") != null)
            {
                ConditionalObject = transform.Find("ConditionBar").transform.Find("Conditional").gameObject;
            }
        }

        if (ConditionalObject != null && condition != null)
        {
            ConditionalObject.transform.Find("LHReference").transform.Find("Text").GetComponent<Text>().text = condition.leftHand.Serialize();
            ConditionalObject.transform.Find("Comparison").transform.Find("Text").GetComponent<Text>().text = condition.comparison;
            ConditionalObject.transform.Find("RHReference").transform.Find("Text").GetComponent<Text>().text = condition.rightHand.Serialize();
        }
    }

    public override string SerializeBlockHeader()
    {
        return $"if {condition.leftHand.Serialize()} {condition.comparison} {condition.rightHand.Serialize()}:";
    }
}
