using Platformer.Mechanics;
using Platformer.Mechanics.Stats;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color meleeAttackColor = Color.green;

    void Start()
    {
        // Initialization if needed
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"Triggered By:\n\n Name: {collision.gameObject.name} \n Tag: {collision.gameObject.tag} \n Layer: {collision.gameObject.layer}\n" + $"Me \n\n Name: {gameObject.name}\n Tag: {gameObject.tag} \nLayer: {gameObject.layer}");
    }
}