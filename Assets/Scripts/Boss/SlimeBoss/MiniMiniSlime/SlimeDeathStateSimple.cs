using UnityEngine;

public class SlimeDeathStateSimple : State<EnemyInputs>
{
    private MiniMiniSlimeController slime;

    public SlimeDeathStateSimple(MiniMiniSlimeController slime)
    {
        this.slime = slime;
    }

    public override void Awake()
    {
        base.Awake();
        Debug.Log("MiniMiniSlime murió.");
        slime.Die();
    }
}
