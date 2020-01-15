using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Print : FunctionCallBase
{

    // Start is called before the first frame update
    public override void InitialiseNode()
    {
        paramCount = 1;
        functionName = "print";
        base.InitialiseNode();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
