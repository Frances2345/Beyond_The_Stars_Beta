using UnityEngine;

public class BulletEnemy : MonoBehaviour, IAttackable
{
    [SerializeField] private float damageAmount = 15;

    public float DamageAmount => damageAmount;
    public float TimeDestroy = 5f;

    public void AttackTarget(IDamageable target)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null && collision.CompareTag("Player"))
        {
            target.TakeDamage(DamageAmount);
            Destroy(gameObject);
            return;
        }

        if(!collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Destroy(gameObject, TimeDestroy);
    }
}
