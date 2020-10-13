using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovment : MonoBehaviour
{
    public bool move;
    public float speed;
    public float scalex, scaley;
    private float height;
    private float angleDown;
    // Start is called before the first frame update
    void Start()
    {
        move = true;
        height = transform.position.y;
        angleDown = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            float angle = Mathf.Deg2Rad * (Time.time * speed-90);
            transform.position = new Vector3(Mathf.Cos(angle) * scalex, transform.position.y, Mathf.Sin(angle) * scaley);
            transform.rotation = Quaternion.Euler(angleDown, -Mathf.Rad2Deg * angle - 90, 0);
        }
    }
}
