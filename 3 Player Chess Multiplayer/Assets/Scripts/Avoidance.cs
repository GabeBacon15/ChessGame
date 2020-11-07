using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avoidance : MonoBehaviour
{
    public float moveSpeed, turnSpeed, viewDistance, turnAngle;
    private float startAngle;
    private int dir;
    Vector3 position;
    bool turning;
    void Start()
    {
        position = this.transform.position;
        turning = false;
        startAngle = 0;
        dir = 0;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(!turning && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, viewDistance))
        {
            turning = true;
            startAngle = this.transform.eulerAngles.y;
            dir = Random.Range(0, 2);
            if(dir == 0)
            {
                dir = -1;
            }
            else if(dir != 1)
            {
                dir = 1;
            }
        }
        else if (turning)
        {
            this.transform.eulerAngles += new Vector3(0f, turnSpeed, 0f) * dir * Time.deltaTime;
            if (dir == -1)
            {
                if(this.transform.eulerAngles.y <= startAngle - turnAngle)
                {
                    turning = false;
                    //if (!turning && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, viewDistance))
                    //{
                    //    turning = true;
                    //    startAngle = this.transform.eulerAngles.y;
                    //}
                }
            }
            else
            {
                if (this.transform.eulerAngles.y >= startAngle + turnAngle)
                {
                    turning = false;
                    //if (!turning && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, viewDistance))
                    //{
                    //    turning = true;
                    //    startAngle = this.transform.eulerAngles.y;
                    //}
                }
            }
        }
        position += this.transform.forward * moveSpeed * Time.deltaTime;
        this.transform.position = position;
    }
}
