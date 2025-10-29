using System.Collections;
using UnityEngine;

public class MimicoSpecialState : State<EnemyInputs>
{
    private enum Phase { Prep, Sequence, Recover }
    private Phase phase;

    [SerializeField] private float redLeadOffset = 0.5f; 

    private readonly MimicoController ctrl;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

  
    private Transform rightSpawn, leftEdge;
    private Transform[] lanesBlue, lanesRed;
    private GameObject trapPrefab;
    private float prepTime, recoverTime, chargeSpeed, emitterSpeed, trailStep, trapDuration;

    private float savedGravity = 0f;
    private bool gravityChanged = false;
    private bool finished;

    public MimicoSpecialState(MimicoController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        rb = ctrl.GetComponent<Rigidbody2D>();
        anim = ctrl.Animator;
        sr = ctrl.GetComponent<SpriteRenderer>();

        
        rightSpawn = ctrl.RightSpawn;
        leftEdge = ctrl.LeftEdge;
        lanesBlue = ctrl.LanesBlue;
        lanesRed = ctrl.LanesRed;
        trapPrefab = ctrl.SpikeTrapPrefab;

        prepTime = ctrl.PrepTime;
        recoverTime = ctrl.RecoverTime;
        chargeSpeed = ctrl.ChargeSpeed;
        emitterSpeed = ctrl.EmitterSpeed;
        trailStep = ctrl.TrailStep;
        trapDuration = ctrl.TrapDuration;

        finished = false;
        ctrl.MarkSpecialUsed();

        
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            savedGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            gravityChanged = true;
        }

        
        ctrl.SetChargeDamage(false);

       
        ctrl.StartCoroutine(MoveToRightAndRun());
    }

    private IEnumerator MoveToRightAndRun()
    {
        
        Vector3 target = new Vector3(rightSpawn.position.x, rightSpawn.position.y, 0);
        while (Vector2.Distance(ctrl.transform.position, target) > 0.02f)
        {
            ctrl.transform.position = Vector2.MoveTowards(
                ctrl.transform.position, target, (chargeSpeed * 0.75f) * Time.deltaTime);
            yield return null;
        }
        ctrl.transform.position = target;

        
        phase = Phase.Prep;
        yield return PrepFX();

        
        phase = Phase.Sequence;
        yield return RunSequence();

        
        phase = Phase.Recover;
        yield return new WaitForSeconds(recoverTime);

        finished = true;
    }

    private IEnumerator PrepFX()
    {
        float t = 0f;
        Color baseC = sr ? sr.color : Color.white;

        while (t < prepTime)
        {
            t += Time.deltaTime;
            if (sr) sr.color = Color.Lerp(baseC, Color.red, Mathf.PingPong(t * 4f, 1f));
            yield return null;
        }

        if (sr) sr.color = baseC;
    }

    private IEnumerator RunSequence()
    {
      
        for (int i = 0; i < lanesRed.Length; i++)
            yield return EmitRedLane(lanesRed[i].position.y);

        
        for (int i = 0; i < lanesBlue.Length; i++)
            yield return ChargeBlueLane(lanesBlue[i].position.y);
    }

    private IEnumerator EmitRedLane(float yLane)
    {
        ctrl.SetChargeDamage(true); 

        Vector3 start = new Vector3(rightSpawn.position.x, yLane, 0);
        Vector3 end = new Vector3(leftEdge.position.x, yLane, 0);
        Vector3 dir = (end - start).normalized;

        ctrl.transform.position = start;

        float dist = Vector3.Distance(start, end);
        float traveled = 0f;
        float nextDrop = 0f;

       
        DropTrapAt(ctrl.transform.position - dir * redLeadOffset);

        while (traveled < dist)
        {
            float step = emitterSpeed * Time.deltaTime;
            ctrl.transform.position += dir * step;
            traveled += step;

            if (traveled >= nextDrop)
            {
                Vector3 dropPos = ctrl.transform.position - dir * redLeadOffset;
                DropTrapAt(dropPos);
                nextDrop += trailStep;
            }
            yield return null;
        }

        
        DropTrapAt(end - dir * redLeadOffset);
        ctrl.transform.position = end;

        ctrl.SetChargeDamage(false); 
    }

    
    private IEnumerator ChargeBlueLane(float yLane)
    {
        ctrl.transform.position = new Vector3(rightSpawn.position.x, yLane, 0);
        ctrl.SetChargeDamage(true);

        Vector3 end = new Vector3(leftEdge.position.x, yLane, 0);
        while (Vector2.Distance(ctrl.transform.position, end) > 0.02f)
        {
            ctrl.transform.position = Vector2.MoveTowards(ctrl.transform.position, end, chargeSpeed * Time.deltaTime);
            yield return null;
        }

        ctrl.SetChargeDamage(false);
    }

    private void DropTrapAt(Vector3 pos)
    {
        if (!trapPrefab) return;
        var trap = Object.Instantiate(trapPrefab, pos, Quaternion.identity);
        Object.Destroy(trap, trapDuration);
    }

    public override void Execute()
    {
        if (!finished) return;
        ctrl.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        base.Sleep();
        ctrl.SetChargeDamage(false);

        if (rb && gravityChanged)
        {
            rb.gravityScale = savedGravity;
            gravityChanged = false;
        }
    }
}


