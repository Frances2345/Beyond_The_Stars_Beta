using UnityEngine;

public class Monolium : MonoBehaviour
{
    private const string Wall = "Wall";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player1 player = Player1.Instance;

            if (player != null)
            {
                player.CollectMonolium();

                if (player.MonoliumCount == player.MaxMonolium)
                {
                    DestroyWall();
                }

                Destroy(gameObject);
            }
        }
    }

    private void DestroyWall()
    {
        GameObject wall = GameObject.FindGameObjectWithTag(Wall);//reemoplzar

        if (wall != null)
        {
            Debug.Log("¡Muro destruido por recolección final de Monolium! Acceso desbloqueado.");
            Destroy(wall);
        }
        else
        {
            Debug.LogWarning("La pared con el Tag '" + Wall + "' no se encontró.");
        }
    }

}
