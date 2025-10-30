using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class PortraitVideoController : MonoBehaviour
{
    [Header("Video Components")]
    public RawImage portraitDisplay;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [Header("Video Clips")]
    public VideoClip idleClip;
    public VideoClip[] moodClips; // mood clips in order (0,1,2,...)

    [Header("Timing")]
    public float minDelay = 5f;
    public float maxDelay = 10f;

    private int currentIndex = 0;

    private void Start()
    {
        // Setup audio output
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // Disable auto loop in inspector
        videoPlayer.isLooping = false;

        // Start with idle
        PlayClip(idleClip);

        // Begin the sequence
        StartCoroutine(PlayMoodSequence());
    }

    private void PlayClip(VideoClip clip)
    {
        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.Play();
        audioSource.Play();
        Debug.Log("Playing: " + clip.name);
    }

    private IEnumerator PlayMoodSequence()
    {
        while (true)
        {
            // Wait a random time before the next mood clip
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            // Play the current mood clip
            VideoClip moodClip = moodClips[currentIndex];
            PlayClip(moodClip);

            // Wait for the clip to finish
            yield return new WaitForSeconds((float)moodClip.length);

            // Return to idle
            PlayClip(idleClip);

            // Move to the next index
            currentIndex++;

            // Loop back to 0 after reaching the end
            if (currentIndex >= moodClips.Length)
                currentIndex = 0;
        }
    }
}
