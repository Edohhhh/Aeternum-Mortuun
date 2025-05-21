using System.Collections;
using UnityEngine;

// Estado de muerte para el slime
public class SlimeDeathState : State<EnemyInputs>
{
    private SlimeController slime;
    private float deathDelay = 0.5f;
    private float timer = 0f;

    public SlimeDeathState(SlimeController slime)
    {
        this.slime = slime;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;
        Debug.Log("Slime entró en DeathState");

        // Acá podrías poner una animación de muerte o efecto visual
        // slime.GetComponent<Animator>().SetTrigger("Die");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        if (timer >= deathDelay)
        {
            slime.Die();
        }
    }

    public override void Sleep()
    {
        // No hace falta limpiar nada porque el slime se destruye
    }
}
