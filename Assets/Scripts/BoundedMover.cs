using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// moves item from start point to end and vise versa
public class BoundedMover : MonoBehaviour
{
    public Vector3 endPoint;
    public float speed;

    private Vector3 startPoint;
    private Vector3 target;

    void Start()
    {
        startPoint = transform.localPosition;
        target = endPoint;
    }

    void FixedUpdate()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);
        if (transform.localPosition == endPoint)
        {
            target = startPoint;
        }
        else if (transform.localPosition == startPoint)
        {
            target = endPoint;
        }
    }
}
