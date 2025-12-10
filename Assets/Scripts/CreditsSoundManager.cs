using UnityEngine;

public class CreditsSoundManager : MonoBehaviour
{
    public static CreditsSoundManager Instance { get; private set; }

    public AudioClip CreditsMusicClip;

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
    }

    private void Start()
    {
        PlayCreditsMusic();
    }

    public void PlayCreditsMusic()
    {
        if (CreditsMusicClip != null && !musicSource.isPlaying)
        {
            musicSource.clip = CreditsMusicClip;
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


    void Update()
    {
        
    }
}
