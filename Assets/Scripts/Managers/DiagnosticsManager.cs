using Platformer.Mechanics;
using TMPro;
using UnityEngine;

public class DiagnosticsManager : MonoBehaviour
{
    public static DiagnosticsManager Instance { get; private set; }

    public TextMeshProUGUI debugText;
    public PlayerController Player;
    public LineRenderer lineRenderer; // Reference to LineRenderer

    void Awake()
    {
        Player = GameObject.Find("Player").GetComponent<PlayerController>();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DisplayDiagnostics(false);
    }

    public void DisplayDiagnostics(bool isVisible)
    {
        debugText.gameObject.SetActive(isVisible);
        lineRenderer.gameObject.SetActive(isVisible); // Enable or disable LineRenderer
    }


    public void UpdateDebugMessage(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        else
        {
            Debug.LogWarning("DebugText not set on " + gameObject.name);
        }
    }
}
