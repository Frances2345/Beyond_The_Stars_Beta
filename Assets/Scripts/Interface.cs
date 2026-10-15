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

// Objetos cuya fuerza de fragmentación al romperse escala con el daño recibido.
// El multiplicador permite que cada fuente (bala de jugador, de enemigo, dash) empuje distinto.
public interface ISplitForceDamageable : IDamageable
{
    void TakeDamage(float amount, float splitForceMultiplier);
}

// Objetos que pueden romperse al ser embestidos con un dash (solo si son pequeños).
public interface IDashBreakable
{
    bool CanBeBrokenByDash { get; }
    void BreakByDash(float damage, float splitForceMultiplier);
}

