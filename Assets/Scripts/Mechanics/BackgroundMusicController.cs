using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public AudioClip backgroundTrack1;
    public AudioClip backgroundTrack2;
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Randomly select a track
        AudioClip selectedTrack = Random.value > 0.5f ? backgroundTrack1 : backgroundTrack2;

        // Set the selected track and enable looping
        audioSource.clip = selectedTrack;
        audioSource.loop = true;

        // Play the track
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
