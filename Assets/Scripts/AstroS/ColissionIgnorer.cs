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
        GameObject[] limits = GameObject.FindGameObjectsWithTag("Limits");

        // Enemy con Asteroid
        foreach (GameObject enemy in enemies)
        {
            Collider2D[] enemyCols = enemy.GetComponents<Collider2D>();
            if (enemyCols.Length == 0) continue;

            foreach (GameObject asteroid in asteroids)
            {
                Collider2D[] asteroidCols = asteroid.GetComponents<Collider2D>();
                foreach (Collider2D asteroidCol in asteroidCols)
                {
                    foreach (Collider2D enemyCol in enemyCols)
                    {
                        Physics2D.IgnoreCollision(enemyCol, asteroidCol, true);
                    }
                }
            }
        }

        // Enemy entre Enemy
        for (int i = 0; i < enemies.Length; i++)
        {
            Collider2D[] colAs = enemies[i].GetComponents<Collider2D>();
            if (colAs.Length == 0) continue;

            for (int j = i + 1; j < enemies.Length; j++)
            {
                Collider2D[] colBs = enemies[j].GetComponents<Collider2D>();
                foreach (Collider2D colA in colAs)
                {
                    foreach (Collider2D colB in colBs)
                    {
                        Physics2D.IgnoreCollision(colA, colB, true);
                    }
                }
            }
        }


        foreach (GameObject enemy in enemies)
        {
            Collider2D[] enemyCols = enemy.GetComponents<Collider2D>();
            if (enemyCols.Length == 0) continue;

            foreach (GameObject limit in limits)
            {
                Collider2D[] limitCols = limit.GetComponents<Collider2D>();
                foreach (Collider2D limitCol in limitCols)
                {
                    foreach (Collider2D enemyCol in enemyCols)
                    {
                        Physics2D.IgnoreCollision(enemyCol, limitCol, true);
                    }
                }
            }
        }

    }
}