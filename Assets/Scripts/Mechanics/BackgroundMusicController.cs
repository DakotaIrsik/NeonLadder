using System.Collections;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public AudioClip backgroundTrack1;
    public AudioClip backgroundTrack2;
    private AudioSource audioSource;

    public AudioClip boss1Intro;
    public AudioClip boss1Loop;
    public AudioClip boss1Outro;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomBackgroundTrack();
    }

    // ... PlayRandomBackgroundTrack and SetVolume methods ...
    public void PlayRandomBackgroundTrack()
    {
        AudioClip selectedTrack = Random.value > 0.5f ? backgroundTrack1 : backgroundTrack2;
        audioSource.clip = selectedTrack;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    public void Boss1Start()
    {
        StopAllCoroutines();
        StartCoroutine(PlayBoss1IntroMusic());
    }

    private IEnumerator PlayBoss1IntroMusic()
    {
        // Stop current music and play boss intro
        audioSource.Stop();
        audioSource.clip = boss1Intro;
        audioSource.loop = false;
        audioSource.Play();

        // Wait for the intro to finish playing
        yield return new WaitForSeconds(boss1Intro.length - 0.5f);
        

        // Play the boss loop
        Boss1MidFight();
    }

    private void Boss1MidFight()
    {
        audioSource.clip = boss1Loop;
        audioSource.loop = true;
        audioSource.Play();
    }

    // ... Boss1Failure and Boss1Stop methods ...
}
