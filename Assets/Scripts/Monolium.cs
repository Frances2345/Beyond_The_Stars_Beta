using UnityEngine;

public class Monolium : MonoBehaviour
{
    public GameObject wallToDestroy;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player1 player = Player1.Instance;

            if (player != null)
            {
                player.CollectMonolium(this);
            }
        }
    }
    public void DestroyWall()
    {
        if (wallToDestroy != null)
        {
            if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.WallDestroyClip != null)
            {
                Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.WallDestroyClip, wallToDestroy.transform.position);
            }


            Debug.Log("¡Muro destruido por recolección final de Monolium! Acceso desbloqueado.");
            Destroy(wallToDestroy);
        }
        else
        {
            Debug.LogWarning("La pared (wallToDestroy) no está asignada en el Inspector.");
        }
    }


}