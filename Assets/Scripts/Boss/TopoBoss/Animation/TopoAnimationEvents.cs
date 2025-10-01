using UnityEngine;

public class TopoAnimationEvents : MonoBehaviour
{
    private TopoController ctrl;
    void Awake() { ctrl = GetComponentInParent<TopoController>(); }

    public void AE_OnEmergeEnd() { ctrl?.AE_OnEmergeEnd(); }
    public void AE_OnAttackFire() { ctrl?.AE_OnAttackFire(); }
    public void AE_OnAttackEnd() { ctrl?.AE_OnAttackEnd(); }
    public void AE_OnBurrowHide() { ctrl?.AE_OnBurrowHide(); }
    public void AE_OnBurrowEnd() { ctrl?.AE_OnBurrowEnd(); }
}