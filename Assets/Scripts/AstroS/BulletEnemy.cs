using UnityEngine;

public class BulletEnemy : MonoBehaviour, IAttackable
{
    [SerializeField] private float damageAmount = 15;

    // Cuánto empujan los fragmentos al romper un asteroide (más bajo = menos vuelo).
    [SerializeField] private float splitForceMultiplier = 0.5f;

    public float knockbackForce = 0f;

    [Header("Interaccion con el escudo del jugador")]
    public float shieldDamage = 5f;
    public bool breakShield = false;
    public bool applyKnockbackOnShield = false;

    public int absorbHits = 1;
    private int absorbHitsLeft = 1;
    private bool consumed = false;

    public float DamageAmount => damageAmount;
    public float TimeDestroy = 5f;

    private void Awake()
    {
        absorbHitsLeft = Mathf.Max(1, absorbHits);
    }

    public void AttackTarget(IDamageable target)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumed) return;

        // Las balas del jugador solo desgastan esta bala, no la destruyen de una
        if (collision.CompareTag("PlayerBullet"))
        {
            absorbHitsLeft--;
            if (absorbHitsLeft <= 0)
            {
                Destroy(gameObject);
            }
            return;
        }

        // Las balas de enemigos tambien dañan asteroides
        if (collision.CompareTag("Asteroid"))
        {
            ISplitForceDamageable splitAsteroid = collision.GetComponent<ISplitForceDamageable>();
            if (splitAsteroid != null)
            {
                splitAsteroid.TakeDamage(DamageAmount, splitForceMultiplier);
                Destroy(gameObject);
                return;
            }

            IDamageable asteroid = collision.GetComponent<IDamageable>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(DamageAmount);
                Destroy(gameObject);
            }
            return;
        }

        // Escudo del jugador: el escudo intercepta la bala
        PlayerShield shield = collision.GetComponentInParent<PlayerShield>();
        if (shield != null && shield.IsBlocking)
        {
            consumed = true;
            shield.OnBulletHit(this);
            Destroy(gameObject);
            return;
        }

        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null && collision.CompareTag("Player"))
        {
            consumed = true;
            target.TakeDamage(DamageAmount);
            ApplyKnockback(collision);
            Destroy(gameObject);
            return;
        }

        if(!collision.CompareTag("Enemy"))
        {
            consumed = true;
            Destroy(gameObject);
        }
    }

    public Vector2 GetTravelDirection(Vector3 fromPosition)
    {
        Rigidbody2D bulletRb = GetComponent<Rigidbody2D>();
        if (bulletRb != null && bulletRb.linearVelocity.sqrMagnitude > 0.001f)
            return bulletRb.linearVelocity.normalized;

        Vector2 toTarget = (Vector2)fromPosition - (Vector2)transform.position;
        if (toTarget.sqrMagnitude > 0.001f) return toTarget.normalized;

        return Vector2.right;
    }

    private void ApplyKnockback(Collider2D collision)
    {
        if (knockbackForce <= 0f) return;

        Player1 player = collision.GetComponent<Player1>();
        Rigidbody2D bulletRb = GetComponent<Rigidbody2D>();
        if (player == null) return;

        Vector2 direction = Vector2.right;
        if (bulletRb != null && bulletRb.linearVelocity.sqrMagnitude > 0.001f)
        {
            direction = bulletRb.linearVelocity.normalized;
        }
        else
        {
            Vector2 toPlayer = (Vector2)collision.transform.position - (Vector2)transform.position;
            if (toPlayer.sqrMagnitude > 0.001f) direction = toPlayer.normalized;
        }

        player.ApplyKnockback(direction, knockbackForce);
    }

    private void Start()
    {
        Destroy(gameObject, TimeDestroy);
    }
}
