using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class Level2SoundManager : MonoBehaviour
{
    public static Level2SoundManager Instance { get; private set; }

    public AudioClip Level2MusicClip;

    [Header("Clips del Jugador")]
    public AudioClip PlayerHitClip;
    public AudioClip PlayerDeathClip;
    public AudioClip PlayerShootClip;
    public AudioClip PlayerDashClip;
    public AudioClip HealthPackClip;

    [Header("Clips Taladirum")]
    public AudioClip TaladriumSound;
    public AudioClip TaladriumDeath;

    private AudioSource shortmusicSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.8f;

        shortmusicSource = gameObject.AddComponent<AudioSource>();
        shortmusicSource.loop = false;
        shortmusicSource.playOnAwake = false;
        shortmusicSource.volume = 1f;
    }



    private void Start()
    {
        PlayLevelMusic();
    }

    public void PlayClip(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            shortmusicSource.PlayOneShot(clip, 0.9f);
        }
    }

    public void PlayLevelMusic()
    {
        if (Level2MusicClip != null && !musicSource.isPlaying)
        {
            musicSource.clip = Level2MusicClip;
            musicSource.Play();
        }
    }

    public void StopLevelMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
