using Platformer.Mechanics;
using TMPro;
using UnityEngine;

public class PlayerHealthUIManager : MonoBehaviour
{

    [SerializeField]
    public Health playerHealth;

    public TextMeshProUGUI HealthCounterText;

    void Awake()
    {
    }

    private void Update()
    {
        HealthCounterText.text = $"Health {playerHealth.currentHP}/{playerHealth.maxHP}";
    }
}
