using UnityEngine;
using System;

public class Asteroids : MonoBehaviour, IDamageable
{
    public AsteroidsSO data;
    public event Action OnDied;

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

        currentHealth = data.health;
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direction = Vector2.right;
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
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

        }

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }
    }
}