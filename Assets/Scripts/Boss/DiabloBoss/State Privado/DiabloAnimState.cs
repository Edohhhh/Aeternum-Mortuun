using UnityEngine;

public class DiabloAnimState : State<EnemyInputs>
{
    private readonly DiabloController ctrl;

    private bool consumed = false;
    private int animId = 1;

    public DiabloAnimState(DiabloController c)
    {
        ctrl = c;
    }

    public override void Awake()
    {
        base.Awake();

        // Nos registramos para que el controller pueda reenviar OnAnimEnd
        ctrl.RegisterAnimState(this);

        consumed = false;

        // Elegimos qué animación de ruleta reproducir (1..AnimCount)
        animId = Mathf.Clamp(ctrl.Roll, 1, ctrl.AnimCount);

        Debug.Log($"[DIABLO/AnimState] ENTER, animId = {animId}");

        if (ctrl.Anim)
        {
            // Apagar todas por las dudas
            for (int i = 1; i <= ctrl.AnimCount; i++)
                ctrl.Anim.SetBool($"Anim{i}", false);

            // Encender sólo la que corresponde
            string flag = $"Anim{animId}";
            ctrl.Anim.SetBool(flag, true);
            Debug.Log($"[DIABLO/AnimState] SetBool({flag}, true)");
        }
    }

    // Llamado por DiabloController.OnAnimEnd() (Animation Event)
    public void OnAnimFinished()
    {
        if (consumed)
        {
            Debug.Log("[DIABLO/AnimState] OnAnimFinished() ya consumido, ignorando");
            return;
        }
        consumed = true;

        Debug.Log($"[DIABLO/AnimState] OnAnimFinished animId={animId} → AttackRouter");

        // Apagar la bool de esta anim
        if (ctrl.Anim)
        {
            string flag = $"Anim{animId}";
            ctrl.Anim.SetBool(flag, false);
            Debug.Log($"[DIABLO/AnimState] SetBool({flag}, false)");
        }

        // Saltar al estado de ataque
        ctrl.Transition(EnemyInputs.SpecialAttack);
    }

    public override void Sleep()
    {
        base.Sleep();

        // Si por algún motivo salimos sin evento, apagamos todas
        if (ctrl.Anim)
        {
            for (int i = 1; i <= ctrl.AnimCount; i++)
                ctrl.Anim.SetBool($"Anim{i}", false);
            Debug.Log("[DIABLO/AnimState] Sleep() -> todas las AnimX en false");
        }
    }
}

//using UnityEngine;

//public class DiabloAnimState : State<EnemyInputs>
//{
//    private readonly DiabloController ctrl;
//    private bool consumed;
//    public DiabloAnimState(DiabloController c) { ctrl = c; }

//    public override void Awake()
//    {
//        base.Awake();
//        ctrl.RegisterAnimState(this);

//        int id = Mathf.Clamp(ctrl.Roll, 1, ctrl.AnimCount);
//        if (ctrl.Anim)
//        {
//            string trig = $"Anim{id}";
//            ctrl.Anim.ResetTrigger(trig);
//            ctrl.Anim.SetTrigger(trig);
//        }
//    }

//    // Llamado por DiabloController.OnAnimEnd() (Animation Event)
//    public void OnAnimFinished()
//    {
//        // Prevenir dobles llamadas
//        if (consumed) return;
//        consumed = true;

//        // Asegura que no haya blending con AttackLoop
//        if (ctrl.Anim) ctrl.Anim.SetBool("isAttacking", false);

//        // Pasa al ataque inmediatamente
//        ctrl.Transition(EnemyInputs.SpecialAttack);
//    }
//}

