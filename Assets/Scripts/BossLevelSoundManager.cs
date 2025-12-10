using UnityEngine;

public class BossLevelSoundManager : MonoBehaviour
{
    public static BossLevelSoundManager Instance { get; private set; }
    public AudioClip bossBattleMusic;

    public AudioClip BossRoarClip;
    public AudioClip BossShootClip;
    public AudioClip BossDeathClip;

    private AudioSource musicSource;
    private AudioSource clipSource;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.8f;

        clipSource = gameObject.AddComponent<AudioSource>();
        clipSource.loop = false;
        clipSource.playOnAwake = false;
        clipSource.volume = 0.6f;
    }

    void Start()
    {
        PlayBossMusic();
    }

    public void PlayBossSFX(AudioClip clip)
    {
        if (clip != null)
        {
            clipSource.PlayOneShot(clip);
        }
    }

    public void PlayBossMusic()
    {
        if (bossBattleMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = bossBattleMusic;
            musicSource.Play();
        }

    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
