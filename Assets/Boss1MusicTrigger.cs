using UnityEngine;

public class Boss1MusicTrigger : MonoBehaviour
{
    public BackgroundMusicController musicController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player has entered the trigger
        if (other.CompareTag("Player")) // Make sure your player GameObject has the tag "Player"
        {
            musicController.Boss1Start();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Optional: Change back to original music when player leaves
        if (other.CompareTag("Player"))
        {
            musicController.PlayRandomBackgroundTrack();
        }
    }
}
