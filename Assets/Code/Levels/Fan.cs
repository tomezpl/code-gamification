using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : Programmable
{
    public double speed = 0.0;

    public bool isDeadly = false;
    public bool isSentient = false;

    public Transform respawnPoint;

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
