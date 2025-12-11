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

public interface IDefendable
{
    float MaxShield { get; }
    float CurrentShield { get; }
    bool IsShieldActive { get; }

    event Action OnShieldDepleted;
}

