using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for using Lists

// Ensures an AudioSource component is attached to the same GameObject
[RequireComponent(typeof(AudioSource))]
public class SimpleAudioManager : MonoBehaviour
{
    // This list will be visible in the Inspector, where you can drag your audio clips
    public List<AudioClip> songs = new List<AudioClip>();

    // The AudioSource component that will play the songs
    private AudioSource audioSource;

    // Index to keep track of the current song
    private int currentSongIndex = 0;

    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Disable looping on the AudioSource itself, as we're handling the sequence manually
        audioSource.loop = false;

        // Start playing the first song if there are any songs in the list
        if (songs.Count > 0)
        {
            StartCoroutine(PlaySongQueue());
        }
        else
        {
            Debug.LogWarning("No songs assigned to the AudioManager.");
        }
    }

    IEnumerator PlaySongQueue()
    {
        // Infinite loop to keep playing songs
        while (true)
        {
            // Check if there are songs and the index is valid
            if (songs.Count == 0 || currentSongIndex < 0 || currentSongIndex >= songs.Count)
            {
                Debug.LogWarning("Song list is empty or index is out of bounds. Stopping playback.");
                yield break; // Exit the coroutine if something is wrong
            }

            // Assign the current song to the AudioSource
            audioSource.clip = songs[currentSongIndex];

            // Play the song
            audioSource.Play();

            // Wait until the current song has finished playing
            // We add a small buffer (0.1f) just in case 'isPlaying' becomes false slightly too early
            while (audioSource.isPlaying || audioSource.time < audioSource.clip.length - 0.1f)
            {
                yield return null; // Wait for the next frame
            }

            // Move to the next song
            currentSongIndex++;

            // If we've reached the end of the list, loop back to the first song
            if (currentSongIndex >= songs.Count)
            {
                currentSongIndex = 0;
            }
        }
    }

    // Optional: Public method to manually skip to the next song
    public void SkipToNextSong()
    {
        if (songs.Count == 0) return;

        // Stop the current song
        audioSource.Stop();

        // Increment index and loop
        currentSongIndex++;
        if (currentSongIndex >= songs.Count)
        {
            currentSongIndex = 0;
        }

        // Restart the coroutine to play from the new current song
        // This involves stopping any existing PlaySongQueue coroutine first
        StopCoroutine("PlaySongQueue"); // Stop by string name
        if (songs.Count > 0)
        {
            StartCoroutine(PlaySongQueue());
        }
    }

    // Optional: Public method to play a specific song by index
    public void PlaySongAtIndex(int songIndex)
    {
        if (songIndex < 0 || songIndex >= songs.Count)
        {
            Debug.LogWarning("Invalid song index: " + songIndex);
            return;
        }

        audioSource.Stop();
        currentSongIndex = songIndex;
        StopCoroutine("PlaySongQueue");
         if (songs.Count > 0)
        {
            StartCoroutine(PlaySongQueue());
        }
    }
}