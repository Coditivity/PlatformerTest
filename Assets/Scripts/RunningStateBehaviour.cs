using UnityEngine;
using System.Collections;

public class RunningStateBehaviour : StateMachineBehaviour
{    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ColliderHandler.SetCollider(ColliderHandler.ColliderType.Running);
    }


}
