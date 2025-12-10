using UnityEngine;

public class AnimatorAstroTrooper : MonoBehaviour
{
    // Hacemos esta variable pública para que arrastres el COMPONENTE Animator en el Inspector
    public Animator animatorAstroTrooper;

    // Controladores de enemigos (obtenidos automáticamente)
    private EnemyController controller;
    private EnemyChase chase;

    void Start()
    {
        // 1. Obtener los controladores de comportamiento del mismo GameObject
        controller = GetComponent<EnemyController>();
        chase = GetComponent<EnemyChase>();

        // 2. Verificar si se encontró el Animator (requerido)
        if (animatorAstroTrooper == null)
        {
            Debug.LogError("ERROR: La variable 'Animator Astro Trooper' no está asignada en el Inspector. ¡Arrastra el COMPONENTE Animator nativo aquí!");
            enabled = false;
            return;
        }

        // (Opcional) Advertencia si no se encontró ningún controlador de lógica
        if (controller == null && chase == null)
        {
            Debug.LogWarning("AnimatorAstroTrooper no encontró EnemyController ni EnemyChase. Ninguna animación se actualizará.");
        }
    }

    void Update()
    {
        // El chequeo de null es redundante si se deshabilitó en Start, pero es una buena práctica
        if (animatorAstroTrooper == null) return;

        // Determinar el estado de las animaciones basado en los controladores existentes

        // IsShooting: Del EnemyController
        bool shooting = controller != null && controller.IsShooting;

        // IsCharging: Del EnemyChase (para el Focus/Embestida)
        bool charging = chase != null && chase.IsCharging;

        // --- Enviar los booleanos al Animator ---

        // Para AstroTrooperDisparo
        animatorAstroTrooper.SetBool("IsShooting", shooting);

        // Para AstroTrooperFocus
        animatorAstroTrooper.SetBool("IsCharging", charging);
    }
}