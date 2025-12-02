using UnityEngine;
using System;

public class BulletPlayer : MonoBehaviour, IAttackable
{
    [SerializeField] private float damageAmount = 15;

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

        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(DamageAmount);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}