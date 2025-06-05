using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for Lists

[RequireComponent(typeof(AudioSource))]
public class SimpleAudioManager : MonoBehaviour
{
    public static SimpleAudioManager Instance { get; private set; }

    public List<AudioClip> songs = new List<AudioClip>();
    public AudioSource audioSource; // Public for GameManager to Pause/UnPause

    private Coroutine playQueueCoroutine;
    private List<int> shuffledPlayOrderIndices = new List<int>();
    private int currentShuffledPlaybackIndex = 0;
    private System.Random rng = new System.Random();
    private bool _isMusicPlayingIntent = false; // Tracks if music *should* be playing (intent)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("[SimpleAudioManager] AudioSource component not found! Disabling.", this.gameObject);
            enabled = false;
            return;
        }
        audioSource.loop = false;
    }

    void Start()
    {
        // Music playback is now explicitly started by GameManager.Start()
        // by calling SimpleAudioManager.Instance.PlayMusic().
    }

    public void PlayMusic()
    {
        if (songs.Count == 0)
        {
            Debug.LogWarning("[SimpleAudioManager] No songs assigned. Music playback cannot start.", this.gameObject);
            _isMusicPlayingIntent = false;
            return;
        }

        // Stop any existing playback cleanly
        if (playQueueCoroutine != null)
        {
            StopCoroutine(playQueueCoroutine);
        }
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        GenerateShuffledPlaylist();
        _isMusicPlayingIntent = true; // Set intent to play
        playQueueCoroutine = StartCoroutine(PlayShuffledQueue());
        // Debug.Log("[SimpleAudioManager] PlayMusic called. Starting playback queue.");
    }

    public void StopMusic()
    {
        _isMusicPlayingIntent = false; // Set intent to NOT play
        if (playQueueCoroutine != null)
        {
            StopCoroutine(playQueueCoroutine);
            playQueueCoroutine = null;
        }
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        // Debug.Log("[SimpleAudioManager] StopMusic called. Playback halted.");
    }

    void GenerateShuffledPlaylist()
    {
        shuffledPlayOrderIndices.Clear();
        for (int i = 0; i < songs.Count; i++)
        {
            shuffledPlayOrderIndices.Add(i);
        }

        int n = shuffledPlayOrderIndices.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int tempValue = shuffledPlayOrderIndices[k];
            shuffledPlayOrderIndices[k] = shuffledPlayOrderIndices[n];
            shuffledPlayOrderIndices[n] = tempValue;
        }
        currentShuffledPlaybackIndex = 0;
    }

    IEnumerator PlayShuffledQueue()
    {
        if (audioSource == null)
        {
            _isMusicPlayingIntent = false;
            yield break;
        }

        while (_isMusicPlayingIntent) // Loop as long as we intend to play music
        {
            if (songs.Count == 0)
            {
                _isMusicPlayingIntent = false;
                yield break;
            }

            if (currentShuffledPlaybackIndex >= shuffledPlayOrderIndices.Count)
            {
                GenerateShuffledPlaylist(); // Reshuffle if end of list is reached
                if (songs.Count == 0) { _isMusicPlayingIntent = false; yield break; } // Check again after shuffle
            }

            int songOriginalIndex = shuffledPlayOrderIndices[currentShuffledPlaybackIndex];

            // Boundary and corruption checks
            if (songOriginalIndex < 0 || songOriginalIndex >= songs.Count) {
                Debug.LogError($"[SimpleAudioManager] Corrupted shuffled playlist index ({songOriginalIndex}). Regenerating playlist.", this.gameObject);
                GenerateShuffledPlaylist();
                if (songs.Count == 0 || currentShuffledPlaybackIndex >= shuffledPlayOrderIndices.Count ||
                    (shuffledPlayOrderIndices.Count > 0 && (shuffledPlayOrderIndices[currentShuffledPlaybackIndex] < 0 || shuffledPlayOrderIndices[currentShuffledPlaybackIndex] >= songs.Count)))
                {
                     _isMusicPlayingIntent = false; yield break; // Failed to recover
                }
                songOriginalIndex = shuffledPlayOrderIndices[currentShuffledPlaybackIndex];
            }

            AudioClip clipToPlay = songs[songOriginalIndex];
            audioSource.clip = clipToPlay;
            audioSource.Play();
            // Debug.Log($"[SimpleAudioManager] Now Playing: {clipToPlay.name}");

            // Wait for the song to finish OR for the intent to play music to change
            // OR for the game to pause (which will pause the AudioSource directly)
            while (_isMusicPlayingIntent && audioSource.clip == clipToPlay)
            {
                if (audioSource.isPlaying)
                {
                    yield return null; // Song is actively playing
                }
                else // AudioSource is not playing
                {
                    // Check if it's because the song naturally finished
                    if (audioSource.time >= clipToPlay.length - 0.1f)
                    {
                        // Debug.Log($"[SimpleAudioManager] Song finished: {clipToPlay.name}");
                        break; // Exit inner loop to advance to next song
                    }
                    // If not finished and not playing, it means it was paused by GameManager
                    // or stopped by an external call not using StopMusic().
                    // The _isMusicPlayingIntent flag handles StopMusic().
                    // The GameManager's AudioSource.Pause() handles game pause.
                    // We just need to yield here to allow UnPause to make it play again.
                    yield return null;
                }
            }

            if (!_isMusicPlayingIntent) // If StopMusic was called during playback or while waiting
            {
                // Debug.Log("[SimpleAudioManager] _isMusicPlayingIntent is false, exiting PlayShuffledQueue.");
                yield break;
            }

            // Only advance if the clip that was meant to play is still the current one
            // and it either finished or was stopped/paused.
            // If another function (like PlaySongAtIndex) changed audioSource.clip, this instance should stop.
            if (audioSource.clip != clipToPlay)
            {
                // Debug.Log($"[SimpleAudioManager] audioSource.clip changed during {clipToPlay.name} playback. Yielding control.");
                yield break; // Another function took over playback for this AudioSource.
            }

            currentShuffledPlaybackIndex++;
        }
    }

    // --- Other Playback Control Methods (Skip, PlayAtIndex) ---
    // These should also be mindful of _isMusicPlayingIntent and playQueueCoroutine

    public void SkipToNextSong()
    {
        if (songs.Count == 0 || !_isMusicPlayingIntent || audioSource == null || !gameObject.activeInHierarchy) return;

        // Stop current song playback & coroutine, advance index, and restart queue
        _isMusicPlayingIntent = false; // Temporarily set to false to stop current queue
        if (playQueueCoroutine != null) StopCoroutine(playQueueCoroutine);
        if (audioSource.isPlaying) audioSource.Stop();

        currentShuffledPlaybackIndex++;
        // Note: PlayShuffledQueue will handle wrapping or re-shuffling if index is out of bounds.

        _isMusicPlayingIntent = true; // Set intent to play again
        playQueueCoroutine = StartCoroutine(PlayShuffledQueue()); // Restart with new state
        // Debug.Log("[SimpleAudioManager] Skipped to next song.");
    }

    public void PlaySongAtIndex(int songIndexInOriginalList)
    {
        if (songIndexInOriginalList < 0 || songIndexInOriginalList >= songs.Count || audioSource == null || !gameObject.activeInHierarchy) return;

        _isMusicPlayingIntent = false; // Stop any current queue
        if (playQueueCoroutine != null) StopCoroutine(playQueueCoroutine);
        if (audioSource.isPlaying) audioSource.Stop();

        currentShuffledPlaybackIndex = 0; // Reset index for GenerateShuffledPlaylist
        shuffledPlayOrderIndices.Clear();
        // Create a temporary playlist starting with the chosen song, then the rest shuffled
        shuffledPlayOrderIndices.Add(songIndexInOriginalList);
        List<int> remainingIndices = new List<int>();
        for(int i=0; i<songs.Count; i++)
        {
            if (i != songIndexInOriginalList) remainingIndices.Add(i);
        }
        // Shuffle remainingIndices (simplified shuffle for brevity, use full Fisher-Yates if many songs)
        for (int i = 0; i < remainingIndices.Count; i++) {
            int k = rng.Next(i, remainingIndices.Count);
            int temp = remainingIndices[i];
            remainingIndices[i] = remainingIndices[k];
            remainingIndices[k] = temp;
        }
        shuffledPlayOrderIndices.AddRange(remainingIndices);


        _isMusicPlayingIntent = true; // Set intent to play
        playQueueCoroutine = StartCoroutine(PlayShuffledQueue()); // Start with this new "playlist"
        // Debug.Log($"[SimpleAudioManager] Playing song at index {songIndexInOriginalList} then resuming shuffle.");
    }

    // ResumeShuffleAfterSpecificSong is no longer strictly needed with the new PlaySongAtIndex logic
    // but if kept, it would also need to manage _isMusicPlayingIntent and playQueueCoroutine.
}
