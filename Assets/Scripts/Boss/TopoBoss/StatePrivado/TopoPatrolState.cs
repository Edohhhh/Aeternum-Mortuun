using System.Collections;
using UnityEngine;

public class TopoPatrolState : State<EnemyInputs>
{
    private readonly TopoController ctrl;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;

    private RigidbodyConstraints2D saved;

    public TopoPatrolState(TopoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        //rb = ctrl.Body;
        //sr = ctrl.SR;
        //col = ctrl.GetComponent<Collider2D>();
        //anim = ctrl.Anim;

        //if (rb)
        //{
        //    saved = rb.constraints;
        //    rb.linearVelocity = Vector2.zero;
        //    rb.constraints = saved | RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        //}

        //if (anim)
        //{
        //    anim.ResetTrigger("Burrow");
        //    anim.SetTrigger("Burrow");
        //}

        //ctrl.StartCoroutine(DoBurrowTravel());
    }

    private IEnumerator DoBurrowTravel()
    {
        if (sr) sr.enabled = true;
        if (col) col.enabled = true;

        
        yield return new WaitForSecondsRealtime(0.35f);

        
        if (sr) sr.enabled = false;
        if (col) col.enabled = false;

        
        var wps = ctrl.Waypoints;
        if (wps == null || wps.Length == 0)
        {
            
            EndAtWaypoint(ctrl.transform.position);
            yield break;
        }

        Transform currentClosest = null;
        float best = float.MaxValue;
        foreach (var t in wps)
        {
            if (!t) continue;
            float d = Vector2.Distance(ctrl.transform.position, t.position);
            if (d < best) { best = d; currentClosest = t; }
        }

       
        Transform target = wps[Random.Range(0, wps.Length)];
        if (wps.Length > 1 && target == currentClosest)
        {
            int idx = System.Array.IndexOf(wps, target);
            target = wps[(idx + 1) % wps.Length];
        }

        
        float speedUG = Mathf.Max(0.01f, ctrl.GetUndergroundSpeedByPhase());
        float travelTime = best / speedUG;
        yield return new WaitForSecondsRealtime(travelTime);

        
        EndAtWaypoint(target.position);
    }

    private void EndAtWaypoint(Vector3 pos)
    {
        ctrl.transform.position = new Vector3(pos.x, pos.y, ctrl.transform.position.z);

        
        if (rb) rb.constraints = saved | RigidbodyConstraints2D.FreezeRotation;

        
        if (sr) sr.enabled = true;
        if (col) col.enabled = true;

        if (anim)
        {
            anim.ResetTrigger("Emerge");
            anim.SetTrigger("Emerge");
        }

        
        ctrl.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        base.Sleep();
        if (rb) rb.constraints = saved | RigidbodyConstraints2D.FreezeRotation;
        if (sr && !sr.enabled) sr.enabled = true;
        if (col && !col.enabled) col.enabled = true;
    }
}
