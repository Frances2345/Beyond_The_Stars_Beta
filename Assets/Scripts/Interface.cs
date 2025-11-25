using UnityEngine;
using System;

public interface IDamageable
{
    void TakeDamage(float amount);
    void Die();
    event Action OnDied;
}

public interface IAttackable
{
    float DamageAmount { get; }
    void AttackTarget(IDamageable target);
}