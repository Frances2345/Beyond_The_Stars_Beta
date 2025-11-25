using UnityEngine;

public class CollisionIgnorer : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("UpdateCollisionRules", 0f, 0.5f);
    }

    void UpdateCollisionRules()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");

        // Enemy con Asteroid
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol == null) continue;

            foreach (GameObject asteroid in asteroids)
            {
                Collider2D asteroidCol = asteroid.GetComponent<Collider2D>();
                if (asteroidCol == null) continue;

                Physics2D.IgnoreCollision(enemyCol, asteroidCol, true);
            }
        }

        // Enemy entre Enemy
        for (int i = 0; i < enemies.Length; i++)
        {
            Collider2D colA = enemies[i].GetComponent<Collider2D>();
            if (colA == null) continue;

            for (int j = i + 1; j < enemies.Length; j++)
            {
                Collider2D colB = enemies[j].GetComponent<Collider2D>();
                if (colB == null) continue;

                Physics2D.IgnoreCollision(colA, colB, true);
            }
        }
    }
}