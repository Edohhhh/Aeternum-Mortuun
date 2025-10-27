using UnityEngine;

public class PlaySoundStop : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.Stop();
    }
}
