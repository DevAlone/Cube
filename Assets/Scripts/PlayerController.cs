using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float horizontalSpeed;
    public bool IsMovingHorizontally
    {
        get
        {
            return isMovingHorizontally;
        }
    }

    private GameController gameController;
    private Vector3 initPosition;
    private bool isMovingHorizontally = false;
    private Vector3 moveTarget;

    public void Move(float stepSize)
    {
        moveTarget = transform.position + new Vector3(stepSize, 0, 0);
        isMovingHorizontally = true;
    }

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        initPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isMovingHorizontally)
        {
            transform.position = Vector3.MoveTowards(
                    transform.position,
                    moveTarget,
                    horizontalSpeed * Time.deltaTime
                );
            isMovingHorizontally = transform.position != moveTarget;
        }

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            initPosition.z
        );

        if (transform.position.y < 0)
        {
            gameController.IsGameOver = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Hazard")
        {
            return;
        }

        gameController.IsGameOver = true;
        gameObject.SetActive(false);
    }
}
