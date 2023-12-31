using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightIntensityController : MonoBehaviour
{
    // Reference to the 2D light component
    private Light2D light2D;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Light2D component attached to this GameObject
        light2D = GetComponent<Light2D>();

        // Check if the Light2D component is present
        if (light2D == null)
        {
            Debug.LogError("Light2D component not found on this GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Change the intensity of the light from 1 to 3 over time
        float intensity = Mathf.Lerp(1f, 3f, Mathf.PingPong(Time.time, 1f));

        // Set the intensity of the Light2D component
        light2D.intensity = intensity;
    }
}