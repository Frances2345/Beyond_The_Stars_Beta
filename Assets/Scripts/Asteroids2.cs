using UnityEngine;
using System;

public class Asteroids2 : MonoBehaviour, IDamageable
{
    public AsteroidsSO data;
    public event Action OnDied;

    [Header("División al romperse")]
    public int splitCount = 2;
    public int maxSplits = 2;
    public float splitHealthFactor = 0.5f;
    public float splitScaleFactor = 0.5f;
    public float splitEjectSpeed = 20f;

    [Header("Daño por choque de enemigo")]
    public float collisionDamageToAsteroid = 20f;

    [Header("Interno (no tocar)")]
    public float instanceHealth = -1f;
    public int instanceSplit = -1;

    private int currentSplit;
    private float currentHealth;
    private Rigidbody2D rb;

    void Start()
    {
        if (data == null)
        {
            Debug.LogError("AsteroideData no asignado en el Inspector.", this);
            Destroy(gameObject);
            return;
        }

        currentSplit = instanceSplit > 0 ? instanceSplit : 0;
        currentHealth = instanceHealth > 0f ? instanceHealth : data.health;
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direction = Vector2.left;
            float slowFactor = 0.5f;

            rb.linearVelocity = direction * data.baseSpeed * slowFactor;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        OnDied?.Invoke();
        SplitIfNeeded();
        Destroy(gameObject);
    }

    private void SplitIfNeeded()
    {
        if (splitCount <= 0 || currentSplit >= maxSplits)
            return;

        float childHealth = data.health * splitHealthFactor;
        Vector3 childScale = transform.localScale * splitScaleFactor;

        instanceHealth = childHealth;
        instanceSplit = currentSplit + 1;

        for (int i = 0; i < splitCount; i++)
        {
            Vector3 offset = (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f;
            GameObject child = Instantiate(gameObject,
                transform.position + offset,
                Quaternion.identity);
            child.transform.localScale = childScale;

            Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                Vector2 ejectDir = (i == 0) ? Vector2.up : Vector2.down;
                childRb.linearVelocity += ejectDir * splitEjectSpeed;
            }
        }

        instanceHealth = -1f;
        instanceSplit = -1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player1 playerScript = Player1.Instance;
            if (playerScript != null)
            {
                IDamageable playerDamageable = playerScript.GetComponent<IDamageable>();

                if (playerDamageable != null)
                {
                    playerDamageable.TakeDamage(data.damageToPlayer);
                    Die();
                }
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamageToAsteroid);
            return;
        }

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamageToAsteroid);
        }
    }
}