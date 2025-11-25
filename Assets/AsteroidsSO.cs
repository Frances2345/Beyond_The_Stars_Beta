using UnityEngine;

[CreateAssetMenu(fileName = "AsteroideData", menuName = "ScriptableObjects/Asteroide Data", order = 1)]

public class AsteroidsSO : ScriptableObject
{

    public float health = 50f;
    public float damageToPlayer = 10f;
    public float baseSpeed = 5f;
    public int scoreValue = 100;

}
