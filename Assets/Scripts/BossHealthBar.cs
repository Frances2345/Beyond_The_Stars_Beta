using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealthBar : MonoBehaviour
{
    public Image HealthFill;
    private BossController bossController;
    private float maxHealth;

    IEnumerator Start()
    {
        yield return null;

        bossController = BossController.Instance;
        if (bossController != null)
        {
            maxHealth = bossController.maxHealth;
            bossController.OnHealthChanged += UpdateHealthBar;
            HealthFill.fillAmount = 1f;
        }
        else
        {
            Debug.LogError("No hay instancia del BossController");
            enabled = false;
        }
    }

    private void UpdateHealthBar(float newHealth)
    {
        HealthFill.fillAmount = newHealth / maxHealth;
    }

    private void OnDestroy()
    {
        if (bossController != null)
        {
            bossController.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
