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
    }

    public (Vector2 rayStart, Vector2 rayDirection) GetRaycastInfo()
    {
        Vector2 rayDirection = Player.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 rayStart = Player.collider2d.bounds.center;
        return (rayStart, rayDirection);
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

