using UnityEngine;

public class DynamicObjectRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public float rotationSpeed = 50.0f; // Speed of rotation

    void Update()
    {
        // Rotate the sphere around its up axis at the specified speed
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
