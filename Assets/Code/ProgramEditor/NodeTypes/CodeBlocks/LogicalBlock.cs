using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A CodeBlock that only triggers if a condition is met.
// In other terms: an if-statement.
public class LogicalBlock : CodeBlock
{
    // The logical condition (leftHand, comparison, rightHand) of this if-statement/loop
    [SerializeField]
    public BoolCondition condition;

    // UI object holding the conditional parameters
    public GameObject ConditionalObject;

    // Most recent outcome of evaluating the logical condition
    public bool evaluatedResult = false;

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
    // Updates the elements inside the command node UI with their respective values
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
