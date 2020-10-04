using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;

public class BasicActionState : StateMachineBehaviour
{
    vThirdPersonController controller;
    
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        if ( controller == null )
        {
            controller = animator.gameObject.GetComponent<vThirdPersonController>();
            if ( controller == null )
            {
                Debug.LogError( "[OnStateEnter] controller is null." );
                return;
            }
        }

        controller.StartAction( stateInfo.shortNameHash );
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ( controller == null )
        {
            controller = animator.gameObject.GetComponent<vThirdPersonController>();
            if ( controller == null )
            {
                Debug.LogError( "[OnStateEnter] controller is null." );
                return;
            }
        }

        controller.EndAction( stateInfo.shortNameHash );
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //}
}
