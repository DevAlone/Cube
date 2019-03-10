using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text scoreValueWidget;
    public Text bestScoreValueWidget;
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
                inputQueue.Clear();
                endTextWidget.text = endTextWidget.text.Replace("$score", score.ToString());
                if (score > PlayerPrefs.GetInt("BestScore", 0))
                {
                    PlayerPrefs.SetInt("BestScore", score);
                }
                updateBestScore();
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
    public GameObject fieldGameObject;
    public GameObject endGameUI;

    public Field field;

    private int score = 0;
    private bool isProcessingInputQueue = false;
    private Vector3 playerTarget;
    private PlayerController playerController;
    private bool isGameOver = false;
    private FieldGenerator fieldGenerator;

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
        field = fieldGameObject.GetComponent<Field>();
        fieldGenerator = fieldGameObject.GetComponent<FieldGenerator>();
        field.onRowCreated += () =>
        {
            isProcessingInputQueue = true;
            Score += scorePerRow;

            field.verticalSpeed += field.verticalAcceleration;
            fieldGenerator.hazardProbability += fieldGenerator.hazardProbabilityIncrementer;
            fieldGenerator.hazardInTheGroundProbability += fieldGenerator.hazardInTheGroundProbabilityIncrementer;
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
            field.UnmarkCellAsTarget(field.currentTargetPosition);
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
        updateBestScore();
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
            if (!inputQueue.IsEmpty())
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
        }
        else
        {
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

    private void updateBestScore()
    {
        var bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreValueWidget.text = "best " + bestScore;
    }
}
