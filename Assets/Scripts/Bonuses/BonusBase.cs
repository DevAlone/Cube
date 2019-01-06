using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BonusBase : MonoBehaviour
{
    public abstract void ApplyBonus();
    public GameObject visualObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ApplyBonus();
            Destroy(visualObject);
        }
    }
}
