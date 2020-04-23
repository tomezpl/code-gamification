using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : Programmable
{
    // Current angular velocity
    public double speed = 0.0;

    // Is the fan deadly? (ie. does contact cause respawn?)
    public bool isDeadly = false;

    // Is the fan sentient? (ie. does it spin on its own)
    public bool isSentient = false;

    // Respawn point used if fan is deadly
    public Transform respawnPoint;

    // The fan is made out of multiple meshes, but only features one box collider for checking if player hits it.
    BoxCollider col;

    public float speedMult = 1.0f;

    protected override void Start()
    {
        base.Start();
        col = GetComponent<BoxCollider>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isSentient)
        {
            speedMult = Mathf.Max(0.0f, speedMult - (Time.deltaTime / 50.0f));
        }

        // failsafe for triggerenter
        if(isDeadly)
        {
            Transform player = GameObject.Find("Player").transform;
            if(player.position.y > transform.position.y)
            {
                player.position = respawnPoint.position;
                player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }

        speed *= speedMult;
        
        transform.Rotate(transform.up * (float)speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isDeadly)
        {
            if(other.transform.parent.name == "Player")
            {
                other.transform.parent.position = respawnPoint.position;
                other.transform.parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }
}
