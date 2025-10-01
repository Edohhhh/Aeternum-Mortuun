using System.Collections;
using UnityEngine;


public class TopoAttackState : State<EnemyInputs>
{
    private readonly TopoController ctrl;

    private enum AttackPattern
    {
        StraightTriple,   // 3 balas rectas (centro, 25°)
        HomingTriple,     // 3 perseguidoras
        Explosive         // 1 proyectil que explota en 4
    }

    public TopoAttackState(TopoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        ctrl.RegisterAttackState(this);
        // No disparamos acá: los Animation Events del clip "Attack"
        // llaman a FireVolleyByPhase() tantas veces como quieras.
    }

    // Lo llama el evento AE_OnAttackFire() en TopoController (desde el clip)
    public void FireVolleyByPhase()
    {
        var chosen = ChooseRandomPattern();

        switch (chosen)
        {
            case AttackPattern.StraightTriple:
                ShootStraightTriple();
                break;

            case AttackPattern.HomingTriple:
                ShootHomingTriple();
                break;

            case AttackPattern.Explosive:
                ShootExplosive();
                break;
        }
    }

    // --- Selección aleatoria por fase ---
    private AttackPattern ChooseRandomPattern()
    {
        int phase = ctrl.GetPhase();

        // En Fase 1: NO homing (elige entre recto o explosivo)
        if (phase == 1)
        {
            return (Random.value < 0.5f)
                ? AttackPattern.StraightTriple
                : AttackPattern.Explosive;
        }

        // En Fase 2 y 3: los tres patrones, equiprobables
        int pick = Random.Range(0, 3); // 0..2
        return (AttackPattern)pick;
    }

    // ---------- Patrones ----------
    private void ShootStraightTriple()
    {
        var p = ctrl.Player;
        if (!p || !ctrl.StraightPrefab) return;

        Vector2 baseDir = (p.position - ctrl.transform.position).normalized;
        FireStraight(baseDir);
        FireStraight(Rotate(baseDir, 25f));
        FireStraight(Rotate(baseDir, -25f));
    }

    private void ShootHomingTriple()
    {
        var p = ctrl.Player;
        if (!p || !ctrl.HomingPrefab) return;

        for (int i = 0; i < 3; i++)
        {
            var go = Object.Instantiate(ctrl.HomingPrefab, ctrl.transform.position, Quaternion.identity);
            var hb = go.GetComponent<MoleBulletHoming>();
            if (hb)
            {
                hb.Initialize(p, ctrl.HomingSpeed, ctrl.HomingTurn, ctrl.HomingDamage, ctrl.PlayerMask);
            }
        }
    }

    private void ShootExplosive()
    {
        var p = ctrl.Player;
        if (!p || !ctrl.ExplosivePrefab) return;

        Vector2 dir = (p.position - ctrl.transform.position).normalized;
        var go = Object.Instantiate(ctrl.ExplosivePrefab, ctrl.transform.position, Quaternion.identity);
        var eb = go.GetComponent<MoleBulletExplosive>();
        if (eb)
        {
            eb.Initialize(dir, ctrl.ExplosiveSpeed, ctrl.ChildPrefab, ctrl.ChildSpeed, ctrl.ChildDamage, ctrl.PlayerMask);
        }
    }

    private void FireStraight(Vector2 dir)
    {
        var go = Object.Instantiate(ctrl.StraightPrefab, ctrl.transform.position, Quaternion.identity);
        var sb = go.GetComponent<MoleBulletStraight>();
        if (sb)
        {
            sb.Initialize(dir, ctrl.StraightSpeed, ctrl.StraightDamage, ctrl.PlayerMask);
        }
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cs = Mathf.Cos(rad); float sn = Mathf.Sin(rad);
        return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
    }
}
