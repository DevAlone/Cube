using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void OnStartedMovingHorizontally(float direction);
    public OnStartedMovingHorizontally onStartedMovingHorizontally;
    public delegate void OnStartedMovingVertically();
    public OnStartedMovingVertically onStartedMovingVertically;
    public GameObject groundLevelObject;
    public float HorizontalSpeed
    {
        get { return _horizontalSpeed; }
        set
        {
            _horizontalSpeed = value;
            if (_horizontalSpeed < minimumHorizontalSpeed)
            {
                _horizontalSpeed = minimumHorizontalSpeed;
            }
            else if (_horizontalSpeed > maximumHorizontalSpeed)
            {
                _horizontalSpeed = maximumHorizontalSpeed;
            }
        }
    }
    public float minimumHorizontalSpeed;
    public float maximumHorizontalSpeed;
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
    private float _horizontalSpeed;

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
            HorizontalSpeed += horizontalAcceleration;
        };
    }

    void FixedUpdate()
    {
        if (isMovingHorizontally)
        {
            animator.speed = HorizontalSpeed * horizontalRotationCoefficient * gameController.gameSpeedModifier;
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
                    HorizontalSpeed * Time.deltaTime * gameController.gameSpeedModifier
                );
            isMovingHorizontally = parentTransform.position != moveTarget;
            if (!isMovingHorizontally)
            {
                onStartedMovingVertically?.Invoke();
            }
        }
        else
        {
            animator.Play("PlayerVerticalRotation");

            animator.speed = field.verticalSpeed * verticalRotationCoefficient * gameController.gameSpeedModifier;
        }

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
