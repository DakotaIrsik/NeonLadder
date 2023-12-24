using Platformer.Mechanics;
using System.Collections;
using UnityEngine;

public class WallBorderController : MonoBehaviour
{
    // Get the collider component once on awake and use it later
    private Collider2D wallCollider;
    public float CollectionRedemptionTimeinMs = 100;

    private void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //playerController.grabState = Assets.Scripts.Mechanics.MechanicEnums.GrabState.Ready;
                playerController.FacingCollider = wallCollider; // Store the wall's collider
            }
        }
        //StartCoroutine(DisableCollision(other)); // Reset after 10 seconds
    }


private IEnumerator DisableCollision(Collider2D other)
{
        //yield return 
    yield return new WaitForSeconds(CollectionRedemptionTimeinMs);
        var playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            //playerController.grabState = Assets.Scripts.Mechanics.MechanicEnums.GrabState.Holding;
            playerController.FacingCollider = null; // Store the wall's collider
                                                            //set player able to hold.....
        }
    }

void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //playerController.grabState = Assets.Scripts.Mechanics.MechanicEnums.GrabState.Holding;
                playerController.FacingCollider = wallCollider; // Store the wall's collider
                //set player able to hold.....
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //playerController.grabState = Assets.Scripts.Mechanics.MechanicEnums.GrabState.ReleasingHold;
                playerController.FacingCollider = null; // Clear the collider when the player leaves
                //set player unable to hold
            }
        }
    }
}
