using UnityEngine;

public class GolemLaserState : State<EnemyInputs>
{
    private enum Phase { Prepare, Fire, Recover }
    private Phase phase;

    private readonly GolemController golem;
    private GolemBeam beamInstance;
    private RigidbodyConstraints2D saved;
    private float timer;
    
    public GolemLaserState(GolemController g) { golem = g; }

    public override void Awake()
    {
        base.Awake();

        // marcar cooldown y congelar movimiento
        golem.MarkLaserUsed();
        if (golem.Body != null)
        {
            saved = golem.Body.constraints;
            golem.Body.linearVelocity = Vector2.zero;
            golem.Body.constraints = saved
               | RigidbodyConstraints2D.FreezePosition
               | RigidbodyConstraints2D.FreezeRotation;
        }

        // animación de carga (usa un trigger "Laser")
        golem.Animator.ResetTrigger("Laser");
        golem.Animator.SetTrigger("Laser");

        // empezamos en PREPARE; las transiciones de fase las harán los Animation Events
        phase = Phase.Prepare;
        timer = 0f;

        // para recibir los eventos
        golem.RegisterLaserState(this);
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Prepare:
                // mirar al jugador mientras carga (opcional)
                FacePlayer();
                break;

            case Phase.Fire:
                // aquí luego instanciamos el prefab del rayo y/o lo actualizamos
                // de momento, solo apuntamos la cara al jugador
                FacePlayer();
                break;

            case Phase.Recover:
                if (timer >= golem.laserRecoverTime)   // 2s por defecto
                    golem.Transition(EnemyInputs.SeePlayer);
                break;
        }
    }

    public override void Sleep()
    {
        if (golem.Body != null)
            golem.Body.constraints = saved;  // restaurar EXACTAMENTE

        golem.RegisterLaserState(null);
        if (beamInstance) Object.Destroy(beamInstance.gameObject);
        base.Sleep();
    }

    // ---------- llamados por Animation Events del clip ----------

    public void OnChargeEnd()
    {
        if (phase != Phase.Prepare) return;
        phase = Phase.Fire;
        timer = 0f;

        // Instanciar el rayo
        if (golem != null && golem.Transform && golem.GetPlayer() && golem.laserPrefab != null)
        {
            beamInstance = Object.Instantiate(golem.laserPrefab, golem.Transform.position, Quaternion.identity);
            // Inicializar con parámetros del controller
            beamInstance.Initialize(
                golem.Transform,
                golem.GetPlayer(),
                golem.BeamDuration,             // 3s
                golem.BeamDmg,                  // 1f (o lo que uses)
                0.25f,                          // intervalo de daño (cámbialo si querés)
                golem.BeamMaxRange,
                golem.BeamThickness,
                golem.BeamKnockback,
                golem.BeamPlayerMask,
                golem.BeamObstacleMask
            );
        }

        Debug.Log("GOLEM LASER: FIRE");
    }
    //public void OnChargeEnd()
    //{
    //    // termina la fase de carga → empieza el rayo
    //    if (phase != Phase.Prepare) return;
    //    phase = Phase.Fire;
    //    timer = 0f;

    //    // (más adelante: instanciar el prefab aquí)
    //    Debug.Log("GOLEM LASER: FIRE (comienza el rayo)");
    //}

    public void OnLaserFinished()
    {
        // termina el rayo → fase de recuperación
        if (phase == Phase.Recover) return;
        phase = Phase.Recover;
        timer = 0f;

        Debug.Log("GOLEM LASER: RECOVER (termina el rayo)");
    }
    // ------------------------------------------------------------

    private void FacePlayer()
    {
        var p = golem.GetPlayer();
        if (!p) return;
        Vector2 dir = p.position - golem.Transform.position;
        golem.GetComponent<SpriteRenderer>().flipX = dir.x < 0;
    }
}