using UnityEngine;
using System;

public class Asteroids : MonoBehaviour, IDamageable, ISplitForceDamageable, IDashBreakable
{
    public AsteroidsSO data;
    public event Action OnDied;

    [Header("División al romperse")]
    public int splitCount = 2;
    public int maxSplits = 2;
    public float splitHealthFactor = 0.5f;
    public float splitScaleFactor = 0.5f;
    public float splitEjectSpeed = 20f;
    public float damageEjectMultiplier = 0.35f;
    public float maxEjectSpeed = 150f;

    [Header("Daño por choque de enemigo")]
    public float collisionDamageToAsteroid = 20f;

    [Header("Romperse con el dash")]
    public float dashBreakSizeThreshold = 0.5f;

    [Header("Interno (no tocar)")]
    public float instanceHealth = -1f;
    public int instanceSplit = -1;
    public Vector2 instanceEjectVelocity = Vector2.zero;

    private int currentSplit;
    private float currentHealth;
    private float lastHitDamage;
    private float lastHitSplitMultiplier = 1f;
    private bool isDead;
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
            Vector2 direction = Vector2.right;
            float slowFactor = 0.5f;

            rb.linearVelocity = direction * data.baseSpeed * slowFactor;

            if (instanceEjectVelocity != Vector2.zero)
            {
                rb.linearVelocity += instanceEjectVelocity;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, 1f);
    }

    public void TakeDamage(float amount, float splitForceMultiplier)
    {
        if (isDead) return;

        lastHitDamage = amount;
        lastHitSplitMultiplier = splitForceMultiplier;
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

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

        // Mientras más daño tenga la bala que lo rompe, más fuerte salen los fragmentos.
        // El multiplicador de la fuente permite que el strider empuje mucho menos que el jugador.
        float ejectSpeed = Mathf.Min(
            splitEjectSpeed + lastHitDamage * damageEjectMultiplier * lastHitSplitMultiplier,
            maxEjectSpeed);

        Vector2 splitAxis = UnityEngine.Random.insideUnitCircle.normalized;
        if (splitAxis.sqrMagnitude < 0.001f)
            splitAxis = Vector2.up;

        for (int i = 0; i < splitCount; i++)
        {
            // Cada fragmento sale en dirección opuesta al anterior.
            Vector2 ejectDir = (i % 2 == 0) ? splitAxis : -splitAxis;
            Vector3 offset = (Vector3)ejectDir * 0.5f;
            GameObject child = Instantiate(gameObject,
                transform.position + offset,
                Quaternion.identity);
            child.transform.localScale = childScale;

            Asteroids childScript = child.GetComponent<Asteroids>();
            if (childScript != null)
            {
                childScript.instanceEjectVelocity = ejectDir * ejectSpeed;
            }

            Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                childRb.linearVelocity += ejectDir * ejectSpeed;
            }
        }

        instanceHealth = -1f;
        instanceSplit = -1;
    }

    public bool CanBeBrokenByDash
    {
        get
        {
            Collider2D myCol = GetComponent<Collider2D>();
            Collider2D playerCol = Player1.Instance != null
                ? Player1.Instance.GetComponent<Collider2D>()
                : null;

            if (myCol == null || playerCol == null)
                return false;

            float mySize = Mathf.Max(myCol.bounds.size.x, myCol.bounds.size.y);
            float playerSize = Mathf.Max(playerCol.bounds.size.x, playerCol.bounds.size.y);

            return mySize <= playerSize * dashBreakSizeThreshold;
        }
    }

    public void BreakByDash(float damage, float splitForceMultiplier)
    {
        if (!CanBeBrokenByDash)
            return;

        TakeDamage(damage, splitForceMultiplier);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player1 player = Player1.Instance;
            if (player != null && player.IsDashing)
            {
                BreakByDash(player.dashDamage, player.dashSplitForceMultiplier);
            }
            return;
        }

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }
    }
}