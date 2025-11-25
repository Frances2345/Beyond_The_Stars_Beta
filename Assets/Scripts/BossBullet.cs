using UnityEngine;

public class BossBullet : MonoBehaviour, IAttackable
{
    [SerializeField] private float damageAmount = 50f;
    public float timeToDestroy = 5f;

    public float DamageAmount => damageAmount;

    public void AttackTarget(IDamageable target)
    {
        target.TakeDamage(damageAmount);
    }

    private void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IDamageable target = collision.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damageAmount);
            }
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Limits") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            return;
        }
    }
}