using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerController ctx;
    private StateMachine sm;
    private float elapsed;
    private Vector2 attackDir;
    private GameObject currentAttack;

    public AttackState(PlayerController context, StateMachine stateMachine)
    {
        ctx = context;
        sm = stateMachine;
    }

    public void Enter()
    {
        elapsed = 0f;
        if (Input.GetKeyDown(KeyCode.UpArrow)) attackDir = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) attackDir = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) attackDir = Vector2.left;
        else attackDir = Vector2.right;

        currentAttack = Object.Instantiate(ctx.attackPrefab,
            ctx.transform.position + (Vector3)attackDir,
            Quaternion.identity);
        currentAttack.transform.parent = ctx.transform;
        float angle = attackDir == Vector2.up ? 0f : attackDir == Vector2.down ? 180f :
            attackDir == Vector2.left ? 90f : -90f;
        currentAttack.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void HandleInput() { }
    public void LogicUpdate()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= ctx.attackDuration)
        {
            sm.ChangeState(ctx.IdleState);
        }
    }
    public void PhysicsUpdate() { }
    public void Exit()
    {
        if (currentAttack != null) Object.Destroy(currentAttack);
    }
}