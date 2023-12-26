using Platformer.Mechanics;
using TMPro;
using UnityEngine;

public class PlayerStaminaUIManager : MonoBehaviour
{

    [SerializeField]
    public Stamina playerStamina;

    public TextMeshProUGUI StaminaCounterTest;

    void Awake()
    {
    }

    private void Update()
    {
        StaminaCounterTest.text = $"Stamina {playerStamina.currentStamina}/{playerStamina.maxStamina}";
    }
}
