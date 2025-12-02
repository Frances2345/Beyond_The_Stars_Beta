using UnityEngine;

public class BulletPlayer : MonoBehaviour, IAttackable
{
    public string IgnoreTag = "Player";

    [SerializeField] private float damageAmount = 15;

    public float DamageAmount => damageAmount;
    public float TimeDestroy = 5f;

    [SerializeField] private AudioClip Shootsound;
    private AudioSource audioSource;

    public void AttackTarget(IDamageable target)
    {

    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(audioSource != null )
        {
            gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = Shootsound;

        if(Shootsound != null)
        {
            audioSource.Play();
        }

    }

    private void Start()
    {
        Destroy(gameObject, TimeDestroy);

        if(Shootsound != null && Shootsound.length > TimeDestroy)
        {
            Destroy(gameObject, Shootsound.length);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(DamageAmount);
            Destroy(gameObject);
            return;
        }

        if(collision.CompareTag(IgnoreTag))
        {
            return;
        }
    }


}
