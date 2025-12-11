using UnityEngine;
using UnityEngine.UI;
using System;

public class HealthBar : MonoBehaviour
{
    public Image HealthFill;
    private Player1 playerController;
    private float maxHealth;

    void Start()
    {
        playerController = Player1.Instance;

        if (playerController != null)
        {
            maxHealth = playerController.maxHealth;

            playerController.OnHealthChanged += UpdateHealthBar;

            HealthFill.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("No se encontró la instancia de Player1. Asegúrate de que el script se inicialice después de Player1.", this);
            enabled = false;
        }
    }

    private void UpdateHealthBar(float newHealth)
    {
        HealthFill.fillAmount = newHealth / maxHealth;
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnHealthChanged -= UpdateHealthBar;
        }
    }
}