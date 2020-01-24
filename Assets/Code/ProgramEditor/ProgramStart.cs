using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                fullProgram += $"{((IProgramNode)currentNode).Serialize()}\n";
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
