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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlayClip(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}