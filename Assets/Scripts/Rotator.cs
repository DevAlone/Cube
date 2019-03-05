using UnityEngine;

// rotates object around its own center
public class Rotator : MonoBehaviour
{
    public float xRotationSpeed;
    public float yRotationSpeed;
    public float zRotationSpeed;

    void FixedUpdate()
    {
        transform.Rotate(
            xRotationSpeed * Time.deltaTime,
            yRotationSpeed * Time.deltaTime,
            zRotationSpeed * Time.deltaTime
        );
    }
}
