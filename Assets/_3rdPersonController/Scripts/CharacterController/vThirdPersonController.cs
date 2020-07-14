using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Invector.vCharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        public AnimationClip dodgeAnimationClip;

        public virtual void ControlAnimatorRootMotion()
        {
            if ( this.enabled == false )
            {
                return;
            }

            if ( inputSmooth == Vector3.zero )
            {
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if ( useRootMotion == true )
            {
                MoveCharacter( moveDirection );
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            ControlLocomotionType();
            ControlRotationType();
        }

        public virtual void ControlLocomotionType()
        {
            if ( lockMovement == true )
            {
                return;
            }

            if ( locomotionType.Equals( LocomotionType.FreeWithStrafe ) && !isStrafing || locomotionType.Equals( LocomotionType.OnlyFree ) )
            {
                SetControllerMoveSpeed( freeSpeed );
                SetAnimatorMoveSpeed( freeSpeed );
            }
            else if ( locomotionType.Equals( LocomotionType.OnlyStrafe ) || locomotionType.Equals( LocomotionType.FreeWithStrafe ) && isStrafing )
            {
                isStrafing = true;
                SetControllerMoveSpeed( strafeSpeed );
                SetAnimatorMoveSpeed( strafeSpeed );
            }

            if ( useRootMotion == false )
            {
                MoveCharacter( moveDirection );
            }
        }

        public virtual void ControlRotationType()
        {
            if ( lockRotation == true )
            {
                return;
            }

            bool validInput = input != Vector3.zero || ( isStrafing == true ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera );

            if ( validInput == true )
            {
                // calculate input smooth
                inputSmooth = Vector3.Lerp( inputSmooth, input, ( isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth ) * Time.deltaTime );

                Vector3 dir = ( isStrafing && ( !isSprinting || sprintOnlyFree == false ) || ( freeSpeed.rotateWithCamera && input == Vector3.zero ) ) && rotateTarget ? rotateTarget.forward : moveDirection;
                RotateToDirection( dir );
            }
        }

        public virtual void UpdateMoveDirection( Transform referenceTransform = null )
        {
            if ( input.magnitude <= 0.01 )
            {
                moveDirection = Vector3.Lerp( moveDirection, Vector3.zero, ( isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth ) * Time.deltaTime );
                return;
            }

            if ( referenceTransform && !rotateByWorld )
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis( -90, Vector3.up ) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = ( inputSmooth.x * right ) + ( inputSmooth.z * forward );
            }
            else
            {
                moveDirection = new Vector3( inputSmooth.x, 0, inputSmooth.z );
            }
        }

        public virtual void Sprint( bool value )
        {
            bool sprintConditions = ( input.sqrMagnitude > 0.1f && isGrounded == true );
            sprintConditions = sprintConditions == true && ( isStrafing == true && strafeSpeed.walkByDefault == false && ( horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f ) ) == false;

            if ( value == true && sprintConditions == true )
            {
                if ( input.sqrMagnitude > 0.1f )
                {
                    if ( isGrounded == true && useContinuousSprint == true )
                    {
                        isSprinting = !isSprinting;
                    }
                    else if ( isSprinting == false )
                    {
                        isSprinting = true;
                    }
                }
                else if ( useContinuousSprint == false && isSprinting == true )
                {
                    isSprinting = false;
                }
            }
            else if ( isSprinting == true )
            {
                isSprinting = false;
            }
        }

        public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }

        public virtual void Jump()
        {
            if ( isBlockedAction == true )
            {
                return;
            }

            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping = true;

            // trigger jump animations
            if ( input.sqrMagnitude < 0.1f )
            {
                animator.CrossFadeInFixedTime( "Jump", 0.1f );
            }
            else
            {
                animator.CrossFadeInFixedTime( "JumpMove", .2f );
            }
        }

        // Called from AnimationClip
        public override void EndAction()
        {
            base.EndAction();
            remainComboDelay = AllowComboDelay;
            moveSpeedRate = 1.0f;
            isBlockedAction = false;
        }

        public override void BasicAttack()
        {
            if ( isBlockedAction == true )
            {
                return;
            }

            if ( remainComboDelay <= 0.0f )
            {
                currentComboCount = 0;
            }
            else
            {
                ++currentComboCount;
                if ( currentComboCount > MaxComboCount )
                {
                    currentComboCount = 0;
                }
            }

            moveSpeed = moveSpeedRate = 0.0f;
            isBlockedAction = true;
            base.BasicAttack();
        }

        public override void DodgeAction()
        {
            if ( remainDodgeCooldown > 0.0f )
            {
                return;
            }

            if ( isBlockedAction == true )
            {
                // 기존 액션 캔슬
                EndAction();
            }

            moveSpeed = moveSpeedRate = 0.0f;
            isBlockedAction = true;

            base.DodgeAction();
            remainDodgeCooldown = DodgeCooldown;
        }

        // Called from AnimationClip
        protected IEnumerator DodgeMove()
        {
            if ( input.sqrMagnitude <= 0.001 )
            {
                moveDirection = _rigidbody.transform.forward;
            }
            else
            {
                UpdateMoveDirection( Camera.main.transform );
            }
            _rigidbody.transform.rotation = Quaternion.LookRotation( moveDirection );
            Vector3 targetPosition = _rigidbody.position + _rigidbody.transform.forward * DodgeDistance;
            
            WaitForFixedUpdate waitUpdate = new WaitForFixedUpdate();
            // 선후딜이 있어 Clip 길이와 정확히 일치하진 않음
            // Clip에서 EndAction() 호출하고 있어서 적당히 해도 될듯
            float maxMoveTime = dodgeAnimationClip.length / DodgeActionSpeed;
            
            // ex) 0.5초안에 10m을 가야한다면, actionSpeed == 10.0 / 0.5 / 10.0 == 2.0
            float dodgeSpeedRate = DodgeDistance / dodgeAnimationClip.length / DodgeDistance;
            while ( maxMoveTime > 0.0f )
            {
                _rigidbody.MovePosition( Vector3.Lerp( _rigidbody.position, targetPosition, Time.fixedDeltaTime * dodgeSpeedRate ) );

                maxMoveTime -= Time.fixedDeltaTime;
                yield return waitUpdate;
            }

            EndAction();
        }

        // Called from AnimationClip
        protected void DodgeStop()
        {
            StopCoroutine( "DodgeMove" );
        }
    }
}