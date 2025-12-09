using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    public AudioClip clickSoundClip;
    public AudioClip quitSoundClip;
    public AudioClip blockSoundClip;

    public AudioClip menuMusicClip;

    private Dictionary<string, AudioClip> musicData = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        AudioSource musicSource = GetComponent<AudioSource>();
        if(musicSource == null )
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if(menuMusicClip != null && !musicSource.isPlaying)
        {
            musicSource.clip = menuMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayClickSound()
    {
        AudioSource audioSource = GetTemporaryAudioSource();
        if (clickSoundClip != null)
        {
            audioSource.PlayOneShot(clickSoundClip);
        }
    }

    public void PlayQuitSound()
    {
        AudioSource audioSource = GetTemporaryAudioSource();
        if (quitSoundClip != null)
        {
            audioSource.PlayOneShot(quitSoundClip);
        }

    }

    public void PlayBlockSound()
    {
        AudioSource audioSource = GetTemporaryAudioSource();
        if (blockSoundClip != null)
        {
            audioSource.PlayOneShot(blockSoundClip);
        }

    }

    private AudioSource GetTemporaryAudioSource()
    {
        AudioSource audioSource = GetComponent<AudioSource> ();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
        return audioSource;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
