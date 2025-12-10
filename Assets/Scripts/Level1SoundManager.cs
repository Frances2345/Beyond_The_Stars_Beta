using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class Level1SoundManager : MonoBehaviour
{
    public static Level1SoundManager Instance { get; private set; }

    public AudioClip LevelMusicClip;

    [Header("Clips del Jugador")]
    public AudioClip PlayerHitClip;
    public AudioClip PlayerDeathClip;
    public AudioClip PlayerShootClip;
    public AudioClip PlayerDashClip;
    public AudioClip HealthPackClip;

    [Header("Clips Specialist Trooper")]
    public AudioClip SpecialSound;
    public AudioClip SpecialDeath;

    [Header("Clips Strider Trooper")]
    public AudioClip StriderSound;
    public AudioClip StriderShoot;
    public AudioClip StriderDeath;

    [Header("Clips Astro Trooper")]
    public AudioClip AstroSound;
    public AudioClip AstrorShoot;
    public AudioClip AstroDeath;

    [Header("Clips Extras")]
    public AudioClip MonoliumCollectClip;
    public AudioClip WallDestroyClip;

    private AudioSource SFXSource;
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

        SFXSource = gameObject.AddComponent<AudioSource>();
        SFXSource.loop = false;
        SFXSource.playOnAwake = false;
        SFXSource.volume = 1f;
    }

    private void Start()
    {
        PlayLevelMusic();
    }

    public void PlayClip(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            SFXSource.PlayOneShot(clip, 0.9f);
        }
    }

    public void PlayLevelMusic()
    {
        if (LevelMusicClip != null && !musicSource.isPlaying)
        {
            musicSource.clip = LevelMusicClip;
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