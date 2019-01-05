using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void OnStartedMovingHorizontally(float direction);
    public OnStartedMovingHorizontally onStartedMovingHorizontally;
    public GameObject groundLevelObject;
    public float horizontalSpeed;
    public float horizontalAcceleration;
    public bool IsMovingHorizontally
    {
        get
        {
            return isMovingHorizontally;
        }
    }
    public float verticalRotationCoefficient;
    public float horizontalRotationCoefficient;
    public Field field;

    private GameController gameController;
    private bool isMovingHorizontally = false;
    private Vector3 moveTarget;
    private Rigidbody rb;
    private Animator animator;
    private Transform parentTransform;

    public void Move(float stepSize)
    {
        onStartedMovingHorizontally?.Invoke(stepSize);
        moveTarget = parentTransform.position + new Vector3(stepSize, 0, 0);
        isMovingHorizontally = true;
    }

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        parentTransform = transform.parent;

        field.onRowCreated += () =>
        {
            horizontalSpeed += horizontalAcceleration;
        };
    }

    void FixedUpdate()
    {
        if (isMovingHorizontally)
        {
            animator.speed = horizontalSpeed * horizontalRotationCoefficient;
            float direction = Mathf.Sign(moveTarget.x - parentTransform.position.x);
            if (direction > 0)
            {
                animator.Play("PlayerHorizontalRotationRight");
            }
            else
            {
                animator.Play("PlayerHorizontalRotationLeft");
            }

            parentTransform.position = Vector3.MoveTowards(
                    parentTransform.position,
                    moveTarget,
                    horizontalSpeed * Time.deltaTime
                );
            isMovingHorizontally = parentTransform.position != moveTarget;
        } else
        {
            animator.Play("PlayerVerticalRotation");

            animator.speed = field.verticalSpeed * verticalRotationCoefficient;
        }

        /*parentTransform.position = new Vector3(
            parentTransform.position.x,
            parentTransform.position.y,
            initPosition.z
        );*/

        if (parentTransform.position.y < groundLevelObject.transform.position.y)
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
