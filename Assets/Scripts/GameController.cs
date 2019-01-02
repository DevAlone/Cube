using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject playerObject;
    public InputQueue inputQueue;
    public bool IsGameOver
    {
        get => isGameOver;
        set
        {
            isGameOver = value;
            endGameUI.SetActive(isGameOver);
        }
    }
    public Field field;
    public GameObject endGameUI;


    private bool isProcessingInputQueue = false;
    private Vector3 playerTarget;
    private PlayerController playerController;
    private bool isGameOver = false;

    void Start()
    {
        playerController = playerObject.GetComponent<PlayerController>();
        field.onRowCreated += () =>
        {
            if (!inputQueue.IsEmpty())
            {
                isProcessingInputQueue = true;
            }
        };
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
                            playerController.Move(-field.stepSize);
                            break;
                        case InputAction.MoveRight:
                            playerController.Move(field.stepSize);
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
            field.Move();
        }
    }
}
