using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameController gameController;
    private Vector3 initPosition;

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        initPosition = transform.position;
    }

    private void FixedUpdate()
    {
        /*transform.position = new Vector3(
            transform.position.x,
            initPosition.y,
            initPosition.z
        );*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Hazard")
        {
            return;
        }

        gameController.gameIsOver = true;
        Destroy(gameObject);
    }
}
