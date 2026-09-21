using UnityEngine;

public class BulletEnemy : MonoBehaviour, IAttackable
{
    [SerializeField] private float damageAmount = 15;

    public float knockbackForce = 0f;

    public int absorbHits = 1;
    private int absorbHitsLeft = 1;

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
            IDamageable asteroid = collision.GetComponent<IDamageable>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(DamageAmount);
                Destroy(gameObject);
            }
            return;
        }

        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null && collision.CompareTag("Player"))
        {
            target.TakeDamage(DamageAmount);
            ApplyKnockback(collision);
            Destroy(gameObject);
            return;
        }

        if(!collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
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
