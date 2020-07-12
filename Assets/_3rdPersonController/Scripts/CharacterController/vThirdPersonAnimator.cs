
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
    }
}