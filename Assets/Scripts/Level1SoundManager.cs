using UnityEngine;

public class Level1SoundManager : MonoBehaviour
{
    public static Level1SoundManager Instance { get; private set; }

    [Header("Clips del Jugador")]
    public AudioClip PlayerHitClip;
    public AudioClip PlayerDeathClip;
    public AudioClip PlayerShootClip;
    public AudioClip PlayerDashClip;
    public AudioClip HealthPackClip;
    public AudioClip MonoliumCollectClip;
    public AudioClip WallDestroyClip;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _audioSource = gameObject.AddComponent<AudioSource>();

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

    }

    public void PlayClip(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            _audioSource.PlayOneShot(clip, 0.9f);
        }
    }
}