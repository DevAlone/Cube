using System.Collections;
using System.Collections.Generic;
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
            if (!inputQueue.IsEmpty())
            {
                isProcessingInputQueue = true;
            }
            Score += scorePerRow;
        };
    }

    void MovePlayer(float shift)
    {
        playerController.Move(shift);
        Score += scorePerHorizontalMove;
    }

    void Update()
    {
        if (isProcessingInputQueue)
        {
            if (isGameOver)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

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
                            return;
                    }
                }
            }
        }
        else
        {
            field.Move(Time.deltaTime);
        }
    }
}
