using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightBlast : MonoBehaviour
{
    public Transform waypointA;
    public Transform waypointB;
    public Light2D movingLight;

    public float speed = 2.0f;
    public int intensityMovingToB = 1;

    private void Start()
    {
        // Set initial light position to waypoint A
        movingLight.transform.position = waypointA.position;

        // Start the light movement coroutine
        StartCoroutine(MoveLightCoroutine());
    }

    IEnumerator MoveLightCoroutine()
    {
        while (true) // Infinite loop
        {
            // Move from A to B
            yield return MoveToWaypoint(waypointB, intensityMovingToB);

            // Set intensity to 0 instantly
            movingLight.intensity = 0;

            // Move from B to A
            yield return MoveToWaypoint(waypointA, 0f);
            // Set intensity to 0 instantly
            movingLight.intensity = intensityMovingToB;
        }
    }

    IEnumerator MoveToWaypoint(Transform targetWaypoint, float targetIntensity)
    {
        while (Vector3.Distance(movingLight.transform.position, targetWaypoint.position) > 0.01f)
        {
            float step = speed * Time.deltaTime;
            movingLight.transform.position = Vector3.MoveTowards(movingLight.transform.position, targetWaypoint.position, step);
            movingLight.intensity = Mathf.MoveTowards(movingLight.intensity, targetIntensity, step);
            yield return null;
        }
    }
}
