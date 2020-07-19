
using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonAnimator : vThirdPersonMotor
    {
        #region Variables                

        public const float walkSpeed = 0.5f;
        public const float runningSpeed = 1f;
        public const float sprintSpeed = 1.5f;

        #endregion  

        public virtual void UpdateAnimator()
        {
            if ( animator == null || animator.enabled == false )
            {
                return;
            }

            animator.SetBool( vAnimatorParameters.IsStrafing, isStrafing ); ;
            animator.SetBool( vAnimatorParameters.IsSprinting, isSprinting );
            animator.SetBool( vAnimatorParameters.IsGrounded, isGrounded );
            animator.SetFloat( vAnimatorParameters.GroundDistance, groundDistance );

            if ( isStrafing == true )
            {
                animator.SetFloat( vAnimatorParameters.InputHorizontal, stopMove == true ? 0 : horizontalSpeed, strafeSpeed.animationSmooth, Time.deltaTime );
                animator.SetFloat( vAnimatorParameters.InputVertical, stopMove == true ? 0 : verticalSpeed, strafeSpeed.animationSmooth, Time.deltaTime );
            }
            else
            {
                animator.SetFloat( vAnimatorParameters.InputVertical, stopMove == true ? 0 : verticalSpeed, freeSpeed.animationSmooth, Time.deltaTime );
            }

            animator.SetFloat( vAnimatorParameters.InputMagnitude, stopMove == true ? 0f : inputMagnitude, isStrafing == true ? strafeSpeed.animationSmooth : freeSpeed.animationSmooth, Time.deltaTime );
        }

        public virtual void SetAnimatorMoveSpeed( vMovementSpeed speed )
        {
            Vector3 relativeInput = transform.InverseTransformDirection( moveDirection );

            float additionalSpeed = ( isSprinting == true ? 0.5f : 0.0f );
            verticalSpeed = relativeInput.z + additionalSpeed;
            horizontalSpeed = relativeInput.x + additionalSpeed;

            var newInput = new Vector2( verticalSpeed, horizontalSpeed );

            inputMagnitude = ( speed.walkByDefault == true ? walkSpeed : runningSpeed ) + additionalSpeed;
            inputMagnitude = Mathf.Clamp( newInput.magnitude, 0, inputMagnitude );
        }

        #region Action

        public virtual void EndAction()
        {
            animator.SetTrigger( vAnimatorParameters.EndAction );
        }

        public virtual void CancelAction()
        {
            //animator.SetTrigger( vAnimatorParameters.CancelAction );
        }

        public virtual void BasicAttack()
        {
            animator.SetInteger( vAnimatorParameters.ComboCount, comboCount.Current );
            animator.SetTrigger( vAnimatorParameters.Attack );
        }

        public virtual void DodgeAction()
        {
            animator.SetFloat( vAnimatorParameters.DodgeActionSpeed, DodgeActionSpeed );
            animator.SetTrigger( vAnimatorParameters.DodgeAction );
        }

        #endregion
    }

    public static partial class vAnimatorParameters
    {
        public static int InputHorizontal = Animator.StringToHash( "InputHorizontal" );
        public static int InputVertical = Animator.StringToHash( "InputVertical" );
        public static int InputMagnitude = Animator.StringToHash( "InputMagnitude" );
        public static int IsGrounded = Animator.StringToHash( "IsGrounded" );
        public static int IsStrafing = Animator.StringToHash( "IsStrafing" );
        public static int IsSprinting = Animator.StringToHash( "IsSprinting" );
        public static int GroundDistance = Animator.StringToHash( "GroundDistance" );
        public static int Attack = Animator.StringToHash( "Attack" );
        public static int ComboCount = Animator.StringToHash( "ComboCount" );
        public static int EndAction = Animator.StringToHash( "EndAction" );
        public static int CancelAction = Animator.StringToHash( "CancelAction" );
        public static int DodgeAction = Animator.StringToHash( "DodgeAction" );
        public static int DodgeActionSpeed = Animator.StringToHash( "DodgeActionSpeed" );
    }
}