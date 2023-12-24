using Platformer.Mechanics;
using UnityEngine;
using static Assets.Scripts.Mechanics.MechanicEnums;

public class RaycastManager : MonoBehaviour
{
    public PlayerController Player;
    public LineRenderer LineRenderer;
    public float RayLength = 0.5f; // Global ray length

    private void Awake()
    {
        LineRenderer.startWidth = LineRenderer.endWidth = 0.05f; // Adjust this value as needed
    }
    private void OnDestroy()
    {
        Destroy(LineRenderer);
    }

    private void Update()
    {
        var WallHit = GetRaycastInfo();
        UpdateLineRenderer(WallHit.Item1, WallHit.Item2, RayLength);
    }

    public (Vector2 rayStart, Vector2 rayDirection) GetRaycastInfo()
    {
        Vector2 rayDirection = Player.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 rayStart = Player.collider2d.bounds.center;
        return (rayStart, rayDirection);
    }

    private Color GetRaycastColor()
    {
        switch (Player.grabState)
        {
            case GrabState.Grabbing:
            case GrabState.Holding:
                return Color.green;
            case GrabState.Ready:
                return Color.blue;
            case GrabState.Unable:
                return Color.red;
            default:
                return Color.yellow;
        }
    }

    private void UpdateLineRenderer(Vector2 start, Vector2 direction, float length)
    {
        Vector2 end = start + direction * length;
        LineRenderer.SetPositions(new Vector3[] { start, end });
        Color rayColor = GetRaycastColor();
        LineRenderer.startColor = LineRenderer.endColor = rayColor;
    }

    private void OnEnable()
    {
        // Enable the LineRenderer when RaycastManager is enabled
        if (LineRenderer != null)
            LineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        // Disable the LineRenderer when RaycastManager is disabled
        if (LineRenderer != null)
            LineRenderer.enabled = false;
    }
}

