using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject groundLevelObject;
    public float horizontalSpeed;
    public bool IsMovingHorizontally
    {
        get
        {
            return isMovingHorizontally;
        }
    }
    public float verticalRotationSpeed;
    public float horizontalRotationSpeed;
    public Field field;

    private GameController gameController;
    private Vector3 initPosition;
    private bool isMovingHorizontally = false;
    private Vector3 moveTarget;
    private Rigidbody rb;
    private Animator animator;

    public void Move(float stepSize)
    {
        moveTarget = transform.position + new Vector3(stepSize, 0, 0);
        isMovingHorizontally = true;
    }

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        initPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isMovingHorizontally)
        {
            animator.speed = horizontalSpeed * horizontalRotationSpeed;
            float direction = Mathf.Sign(moveTarget.x - transform.position.x);
            if (direction > 0)
            {
                animator.Play("PlayerHorizontalRotationRight");
            }
            else
            {
                animator.Play("PlayerHorizontalRotationLeft");
            }

            /*float direction = -Mathf.Sign(moveTarget.x - transform.position.x);

            transform.Rotate(Vector3.forward, direction * Time.deltaTime * horizontalRotationSpeed * horizontalSpeed);*/

            transform.position = Vector3.MoveTowards(
                    transform.position,
                    moveTarget,
                    horizontalSpeed * Time.deltaTime
                );
            isMovingHorizontally = transform.position != moveTarget;
        } else
        {
            animator.Play("PlayerVerticalRotation");

            animator.speed = field.verticalSpeed * verticalRotationSpeed;
            /*transform.rotation = Quaternion.Euler(
                0,  // transform.rotation.x, 
                0,  // -Time.deltaTime * verticalRotationSpeed * field.verticalSpeed,  // transform.rotation.y, 
                0 // transform.rotation.z
            );*/

            // transform.Rotate(Vector3.left, -Time.deltaTime * verticalRotationSpeed * field.verticalSpeed);
        }

        /*transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            initPosition.z
        );*/

        if (transform.position.y < groundLevelObject.transform.position.y)
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
