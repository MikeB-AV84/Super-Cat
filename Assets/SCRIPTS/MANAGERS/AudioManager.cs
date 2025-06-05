using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for Lists

// Ensures an AudioSource component is attached to the same GameObject
[RequireComponent(typeof(AudioSource))]
public class SimpleAudioManager : MonoBehaviour
{
    // This list will be visible in the Inspector, where you can drag your audio clips
    public List<AudioClip> songs = new List<AudioClip>();

    private AudioSource audioSource;

    // Stores the original indices of songs in the current shuffled play order
    private List<int> shuffledPlayOrderIndices = new List<int>();
    // Current position in the shuffledPlayOrderIndices list
    private int currentShuffledPlaybackIndex = 0;

    // System.Random for shuffling. Initialized once to maintain state.
    private System.Random rng = new System.Random();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Disable looping on the AudioSource itself, as we're handling the sequence and loop manually
        audioSource.loop = false;

        if (songs.Count > 0)
        {
            GenerateShuffledPlaylist();
            StartCoroutine(PlayShuffledQueue());
        }
        else
        {
            Debug.LogWarning("[SimpleAudioManager] No songs assigned in the Inspector. Music playback will not start.");
        }
    }

    /// <summary>
    /// Clears the current play order, populates it with indices from the songs list,
    /// and then shuffles it using the Fisher-Yates algorithm.
    /// Resets the playback index to the beginning of the new list.
    /// </summary>
    void GenerateShuffledPlaylist()
    {
        shuffledPlayOrderIndices.Clear();
        for (int i = 0; i < songs.Count; i++)
        {
            shuffledPlayOrderIndices.Add(i);
        }

        // Fisher-Yates shuffle algorithm
        int n = shuffledPlayOrderIndices.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); // Generates a random integer k such that 0 <= k <= n.
            // Swap elements
            int tempValue = shuffledPlayOrderIndices[k];
            shuffledPlayOrderIndices[k] = shuffledPlayOrderIndices[n];
            shuffledPlayOrderIndices[n] = tempValue;
        }
        currentShuffledPlaybackIndex = 0; // Reset to the start of the new shuffled list
        // Uncomment to see the new shuffle order in console:
        // Debug.Log("[SimpleAudioManager] Playlist shuffled. New order: " + string.Join(", ", shuffledPlayOrderIndices.ConvertAll(i => i.ToString())));
    }

    /// <summary>
    /// Coroutine that plays songs from the shuffled list one after another.
    /// When the list ends, it re-shuffles and continues.
    /// </summary>
    IEnumerator PlayShuffledQueue()
    {
        while (true) // Loop indefinitely to keep music playing
        {
            if (songs.Count == 0)
            {
                Debug.LogWarning("[SimpleAudioManager] Song list is empty. Halting playback coroutine.");
                yield break; // Exit coroutine if no songs are available
            }

            // Check if we've played all songs in the current shuffled sequence
            if (currentShuffledPlaybackIndex >= shuffledPlayOrderIndices.Count)
            {
                if (songs.Count > 0) // Ensure there are songs before trying to re-shuffle
                {
                    // Debug.Log("[SimpleAudioManager] Current shuffled playlist ended. Generating a new one.");
                    GenerateShuffledPlaylist(); // Re-shuffle and reset index
                }
                else 
                {
                     Debug.LogWarning("[SimpleAudioManager] Song list became empty during playback. Halting playback.");
                     yield break;
                }
            }
            
            // Get the actual song index from our shuffled list
            int songOriginalIndex = shuffledPlayOrderIndices[currentShuffledPlaybackIndex];

            // Safety check for the song index (should generally be fine if GenerateShuffledPlaylist is correct)
            if (songOriginalIndex < 0 || songOriginalIndex >= songs.Count) {
                Debug.LogError($"[SimpleAudioManager] Corrupted shuffled playlist: index {songOriginalIndex} is out of bounds for songs list (count: {songs.Count}). Attempting to recover by re-shuffling.");
                if (songs.Count > 0) {
                    GenerateShuffledPlaylist();
                    if (shuffledPlayOrderIndices.Count == 0 || currentShuffledPlaybackIndex >= shuffledPlayOrderIndices.Count || 
                        (shuffledPlayOrderIndices.Count > 0 && (shuffledPlayOrderIndices[currentShuffledPlaybackIndex] < 0 || shuffledPlayOrderIndices[currentShuffledPlaybackIndex] >= songs.Count))) {
                         Debug.LogError("[SimpleAudioManager] Failed to recover from corrupted playlist after re-shuffle. Halting playback.");
                         yield break;
                    }
                    songOriginalIndex = shuffledPlayOrderIndices[currentShuffledPlaybackIndex]; 
                } else {
                     Debug.LogError("[SimpleAudioManager] Songs list is empty, cannot recover from corrupted playlist. Halting playback.");
                     yield break;
                }
            }

            audioSource.clip = songs[songOriginalIndex];
            // Debug.Log($"[SimpleAudioManager] Now Playing (Shuffle #{currentShuffledPlaybackIndex + 1}/{shuffledPlayOrderIndices.Count}): {audioSource.clip.name} (Original Index: {songOriginalIndex})");
            audioSource.Play();

            // Wait until the current song has finished playing.
            // The audioSource.clip != null check is a good safety measure.
            while (audioSource.isPlaying || (audioSource.clip != null && audioSource.time < audioSource.clip.length - 0.1f))
            {
                yield return null; // Wait for the next frame
            }

            currentShuffledPlaybackIndex++; // Advance to the next song in the shuffled list
        }
    }

    /// <summary>
    /// Stops the current song and plays the next song in the shuffled sequence.
    /// If at the end of the sequence, a new shuffled sequence is generated.
    /// </summary>
    public void SkipToNextSong()
    {
        if (songs.Count == 0)
        {
            Debug.LogWarning("[SimpleAudioManager] No songs available to skip to.");
            return;
        }
        if (!gameObject.activeInHierarchy) return; // Don't operate if GameObject is not active

        audioSource.Stop();

        // Stop the current playback coroutine.
        StopCoroutine("PlayShuffledQueue"); 
        
        currentShuffledPlaybackIndex++; // Advance index. PlayShuffledQueue will handle re-shuffling if it goes out of bounds.

        // Restart the coroutine. It will pick up from the new index or re-shuffle.
        StartCoroutine(PlayShuffledQueue());
        // Debug.Log("[SimpleAudioManager] Skipped to next song.");
    }

    /// <summary>
    /// Stops current playback, plays the song at the specified original index,
    /// and then resumes shuffled playback with a new shuffle.
    /// </summary>
    /// <param name="songIndexInOriginalList">The index of the song in the public 'songs' list.</param>
    public void PlaySongAtIndex(int songIndexInOriginalList)
    {
        if (songIndexInOriginalList < 0 || songIndexInOriginalList >= songs.Count)
        {
            Debug.LogWarning($"[SimpleAudioManager] Invalid song index requested: {songIndexInOriginalList}. Total songs: {songs.Count}");
            return;
        }
        if (!gameObject.activeInHierarchy) return;

        StopCoroutine("PlayShuffledQueue"); // Stop the main shuffled queue
        if(audioSource.isPlaying) audioSource.Stop();

        // Play the specifically requested song
        AudioClip specificClip = songs[songIndexInOriginalList];
        audioSource.clip = specificClip;
        // Debug.Log($"[SimpleAudioManager] Playing specific song (Original Index: {songIndexInOriginalList}): {specificClip.name}");
        audioSource.Play();

        // Start a new coroutine to wait for this specific song to finish, 
        // then resume the main shuffle with a new playlist.
        StartCoroutine(ResumeShuffleAfterSpecificSong(specificClip));
    }

    /// <summary>
    /// Waits for a specifically played clip to finish, then generates a new
    /// shuffled playlist and restarts the main playback queue.
    /// </summary>
    /// <param name="specificallyPlayedClip">The AudioClip that was played out of sequence.</param>
    IEnumerator ResumeShuffleAfterSpecificSong(AudioClip specificallyPlayedClip)
    {
        // Wait for the specifically chosen song to finish playing.
        // Ensure audioSource.clip hasn't changed to something else while waiting.
        while (audioSource.isPlaying && audioSource.clip == specificallyPlayedClip)
        {
            yield return null;
        }

        // Debug.Log($"[SimpleAudioManager] Specific song '{specificallyPlayedClip.name}' finished. Resuming shuffled playback with a new shuffle.");
        
        if (songs.Count > 0 && gameObject.activeInHierarchy)
        {
            GenerateShuffledPlaylist(); // Create a fresh shuffle
            StartCoroutine(PlayShuffledQueue()); // Restart the main queue
        }
    }
}