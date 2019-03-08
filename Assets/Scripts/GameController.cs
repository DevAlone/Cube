using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text scoreValueWidget;
    public Text endTextWidget;
    public int scorePerRow;
    public int scorePerHorizontalMove;
    public GameObject playerObject;
    public InputQueue inputQueue;
    public bool IsGameOver
    {
        get => isGameOver;
        set
        {
            isGameOver = value;
            if (isGameOver)
            {
                endTextWidget.text = endTextWidget.text.Replace("$score", score.ToString());
            }
            endGameUI.SetActive(isGameOver);
        }
    }

    public int Score
    {
        get => score;
        set
        {
            if (IsGameOver)
            {
                return;
            }

            score = value;
            scoreValueWidget.text = score.ToString();
        }
    }

    public float gameSpeedModifier = 1;
    public float defaultGameSpeedModifier = 1;
    public Field field;
    public GameObject endGameUI;


    private int score = 0;
    private bool isProcessingInputQueue = false;
    private Vector3 playerTarget;
    private PlayerController playerController;
    private bool isGameOver = false;

    public static GameController GetCurrent()
    {
        return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    public void ResetGameSpeedModifier()
    {
        gameSpeedModifier = defaultGameSpeedModifier;
    }

    void Start()
    {
        playerController = playerObject.GetComponent<PlayerController>();
        field.onRowCreated += () =>
        {
            isProcessingInputQueue = true;
            Score += scorePerRow;

            field.verticalSpeed += field.verticalAcceleration;
            field.hazardProbability += field.hazardProbabilityIncrementer;
            field.hazardInTheGroundProbability += field.hazardInTheGroundProbabilityIncrementer;
        };

        inputQueue.onActionAdded += (InputAction action) =>
        {
            switch (action)
            {
                case InputAction.MoveLeft:
                    field.currentTargetPosition.x -= 1;
                    break;
                case InputAction.MoveRight:
                    field.currentTargetPosition.x += 1;
                    break;
                case InputAction.SkipStep:
                    field.currentTargetPosition.y += 1;
                    break;
            }

            field.MarkCellAsTarget(field.currentTargetPosition);
        };

        inputQueue.onLastActionRemoved += (InputAction action) =>
        {
            switch (action)
            {
                case InputAction.MoveLeft:
                    field.currentTargetPosition.x += 1;
                    break;
                case InputAction.MoveRight:
                    field.currentTargetPosition.x -= 1;
                    break;
                case InputAction.SkipStep:
                    field.currentTargetPosition.y -= 1;
                    break;
            }
        };
    }

    void MovePlayer(float shift)
    {
        playerController.Move(shift);
        Score += scorePerHorizontalMove;
    }

    void Update()
    {
        if (isGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (isProcessingInputQueue)
        {
            ProcessInputQueue();
            if (!isProcessingInputQueue && inputQueue.IsEmpty())
            {
                // mark next cell if we're moving not by player's input but automatically
                field.currentTargetPosition = field.playerPositionInMap;
                field.currentTargetPosition.y += 1;
                field.MarkCellAsTarget(field.currentTargetPosition);
            }
        }
        else
        {
            field.Move(Time.deltaTime);
        }
    }

    void ProcessInputQueue()
    {
        if (!playerController.IsMovingHorizontally)
        {
            if (inputQueue.IsEmpty())
            {
                isProcessingInputQueue = false;
            }
            else
            {
                switch (inputQueue.Dequeue())
                {
                    case InputAction.MoveLeft:
                        MovePlayer(-field.cellSize);
                        break;
                    case InputAction.MoveRight:
                        MovePlayer(field.cellSize);
                        break;
                    default:
                        // skip current step
                        isProcessingInputQueue = false;
                        break;
                }
            }
        }
    }
}
