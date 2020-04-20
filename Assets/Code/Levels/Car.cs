using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : Programmable
{
    public int XDir = 0;
    public int YDir = 1;

    public Vector2 StartingCoord = new Vector2(2, 1);
    public Vector2 CurrentCoord = new Vector2(2, 1);

    // Start is called before the first frame update
    void Start()
    {
        CurrentCoord = StartingCoord;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.UpArrow))
            Move();
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
            TurnLeft();
        else if (Input.GetKeyUp(KeyCode.RightArrow))
            TurnRight();*/
    }

    public void TurnRight()
    {
        transform.Rotate(transform.up * 90.0f);
        if(XDir == 0)
        {
            // pointing down
            if (YDir < 0)
            {
                XDir = -1; // reorient to left
            }
            else // pointing up
            {
                XDir = 1; // reorient to right
            }
            YDir = 0;
        }
        else if(YDir == 0)
        {
            if(XDir < 0) // pointing left
            {
                YDir = 1; // reorient to up
            }
            else // pointing right
            {
                YDir = -1; // reorient to down
            }
            XDir = 0;
        }
    }

    public void TurnLeft()
    {
        transform.Rotate(transform.up * -90.0f);
        if (XDir == 0)
        {
            // pointing down
            if (YDir < 0)
            {
                XDir = 1; // reorient to right
            }
            else // pointing up
            {
                XDir = -1; // reorient to left
            }
            YDir = 0;
        }
        else if (YDir == 0)
        {
            if (XDir < 0) // pointing left
            {
                YDir = -1; // reorient to down
            }
            else // pointing right
            {
                YDir = 1; // reorient to up
            }
            XDir = 0;
        }
    }

    public void Move()
    {
        transform.position += new Vector3(-XDir, 0.0f, -YDir * 0.9f);
        CurrentCoord += new Vector2(XDir, YDir);
    }
}
