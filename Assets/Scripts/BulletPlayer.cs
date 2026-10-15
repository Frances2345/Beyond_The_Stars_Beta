using UnityEngine;
using System;

public class BulletPlayer : MonoBehaviour, IAttackable
{
    [SerializeField] private float damageAmount = 15;

    // Cuánto empujan los fragmentos al romper un asteroide (más bajo = menos vuelo).
    [SerializeField] private float splitForceMultiplier = 0.7f;

    public string IgnoreTag = "Player";

    public float DamageAmount => damageAmount;
    public float TimeDestroy = 5f;

    [SerializeField] private AudioClip shootSoundClip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = shootSoundClip;

        if (shootSoundClip != null)
        {
            audioSource.Play();
        }
    }

    public void AttackTarget(IDamageable target)
    {

    }

    private void Start()
    {
        Destroy(gameObject, TimeDestroy);

        if (shootSoundClip != null && shootSoundClip.length > TimeDestroy)
        {
            Destroy(gameObject, shootSoundClip.length);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(IgnoreTag))
        {
            return;
        }

        // La bala del jugador atraviesa su propio escudo.
        if (collision.GetComponentInParent<PlayerShield>() != null)
        {
            return;
        }

        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            ISplitForceDamageable splitTarget = collision.GetComponent<ISplitForceDamageable>();
            if (splitTarget != null)
            {
                splitTarget.TakeDamage(DamageAmount, splitForceMultiplier);
            }
            else
            {
                target.TakeDamage(DamageAmount);
            }

            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}