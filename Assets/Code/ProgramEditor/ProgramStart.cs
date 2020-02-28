using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgramStart : NodeBase
{
    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log(Serialize());
        }
    }

    public override string Serialize()
    {
        string fullProgram = "";

        if (NextNodeObject != null)
        {
            NodeBase currentNode = NextNodeObject.GetComponent<NodeBase>();

            while (currentNode != null)
            {
                if (!(currentNode.GetComponent<ArithmeticOperationBase>() && !currentNode.GetComponent<AssignValue>()))
                {
                    string currentLine = ((IProgramNode)currentNode).Serialize();
                    // Add current line to full program
                    fullProgram += $"{currentLine}\n";
                }
                if (currentNode.NextNodeObject != null)
                {
                    currentNode = currentNode.NextNodeObject.GetComponent<NodeBase>();
                }
                else
                {
                    break;
                }
            }
        }

        return fullProgram;
    }
}
