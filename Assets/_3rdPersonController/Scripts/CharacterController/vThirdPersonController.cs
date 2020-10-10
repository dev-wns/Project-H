using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Invector.vCharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        public enum ESpawnType
        {
            ATTACH,
            WORLD,
            LOCAL,
        }

        public AnimationClip dodgeAnimationClip;

        private int actionStackCount;

        #region UnityEvent

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            ControlLocomotionType();
            ControlRotationType();

            if ( IsTargeting() == true )
            {
                if ( isBlockedAction == false && isSprinting == false )
                {
                    Vector3 direction = ( currentTarget.transform.position - _rigidbody.position );
                    direction.y = 0.0f;
                    Quaternion target = Quaternion.LookRotation( direction.normalized );
                    _rigidbody.MoveRotation( Quaternion.Slerp( _rigidbody.rotation, target, Time.fixedDeltaTime * strafeSpeed.rotationSpeed ) );
                }

                if ( Vector3.Distance( _rigidbody.position, currentTarget.transform.position ) > TargetingRange )
                {
                    currentTarget = null;
                    Strafe( false );
                }
            }
        }

        #endregion

        #region Control

        public virtual void ControlAnimatorRootMotion()
        {
            if ( this.enabled == false )
            {
                return;
            }

            //if ( inputSmooth == Vector3.zero )
            //{
            //    _rigidbody.position = animator.rootPosition;
            //    _rigidbody.rotation = animator.rootRotation;
            //}

            if ( useRootMotion == true )
            {
                MoveCharacter( moveDirection );
            }
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
            bool sprintConditions = ( input.sqrMagnitude > 0.1f && isGrounded == true ) && strafeSpeed.walkByDefault == false;
            //sprintConditions = sprintConditions == true && ( isStrafing == true && strafeSpeed.walkByDefault == false && ( horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f ) ) == false;

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

        public virtual void Strafe( bool isEnable )
        {
            isStrafing = isEnable;
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

        public virtual void Targeting()
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );

            Debug.DrawRay( ray.origin, TargetingRange * ray.direction, Color.blue, 3.0f );

            RaycastHit rayHit;
            if ( Physics.Raycast( ray, out rayHit, TargetingRange ) == true )
            {
                currentTarget = rayHit.collider.GetComponent<Actor>();
                if ( currentTarget != null )
                {
                    Debug.Log( "Target : " + currentTarget );
                    Strafe( true );
                    return;
                }
            }

            currentTarget = null;
            Strafe( false );
        }

        #endregion

        #region Action

        public virtual void SetActionCancelable()
        {
            isCancelableAction = true;
        }

        public virtual void StartAction( int stateHash )
        {
            ++actionStackCount;

            currentStateHash = stateHash;
            moveSpeed = moveSpeedRate = 0.0f;
            forwardInputAxis = 0.0f;
            isBlockedAction = true;
            isCancelableAction = false;
        }

        // Called from AnimationClip
        public override void EndAction( int stateHash )
        {
            if ( stateHash != 0 )
            {
                --actionStackCount;
                if ( actionStackCount > 0 )
                {
                    return;
                }

                if ( actionStackCount < 0 )
                {
                    Debug.LogError( "[EndAction] actionStackCount is " + actionStackCount );
                }
            }

            base.EndAction( stateHash );
            comboDelay.Reset();
            moveSpeedRate = 1.0f;
            isBlockedAction = false;
            isCancelableAction = false;
            return;
        }

        public override void CancelAction()
        {
            base.CancelAction();
            isBlockedAction = false;
            isCancelableAction = false;
        }

        public override void BasicAttack()
        {
            if ( isBlockedAction == true )
            {
                if ( isCancelableAction == false || comboCount.Current >= comboCount.Max )
                {
                    return;
                }
                CancelAction();
            }

            comboDelay.Reset();
            if ( comboCount.Current >= comboCount.Max )
            {
                comboCount.SetZero();
            }
            ++comboCount.Current;

            base.BasicAttack();
        }

        public override void SecondaryAction()
        {
            if ( isBlockedAction == true )
            {
                if ( isCancelableAction == false )
                {
                    return;
                }
                CancelAction();
            }

            base.SecondaryAction();
            secondaryCooldown.Reset();
        }

        public override void Extra1Action()
        {
            if ( isBlockedAction == true )
            {
                if ( isCancelableAction == false )
                {
                    return;
                }
                CancelAction();
            }

            base.Extra1Action();
            extra1Cooldown.Reset();
        }

        public override void Extra2Action()
        {
            if ( isBlockedAction == true )
            {
                if ( isCancelableAction == false )
                {
                    return;
                }
                CancelAction();
            }

            base.Extra2Action();
            extra2Cooldown.Reset();
        }

        public override void DodgeAction()
        {
            if ( dodgeCooldown.Current > 0.0f )
            {
                return;
            }

            if ( isBlockedAction == true )
            {
                CancelAction();
            }

            base.DodgeAction();
            dodgeCooldown.Reset();
        }

        // Called from AnimationClip
        protected IEnumerator DodgeMove()
        {
            if ( input.sqrMagnitude <= 0.001 )
            {
                moveDirection = transform.forward;
            }
            else
            {
                UpdateMoveDirection( Camera.main.transform );
            }
            transform.rotation = Quaternion.LookRotation( moveDirection );
            Vector3 targetPosition = _rigidbody.position + transform.forward * DodgeDistance;

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

            EndAction( 0 );
        }

        // Called from AnimationClip
        protected void DodgeStop()
        {
            StopCoroutine( "DodgeMove" );
        }

        #endregion

        public override void MoveForward( float power )
        {
            float forwardDot = Vector3.Dot( transform.forward, Camera.main.transform.rotation * input );
            forwardDot = Mathf.Clamp( forwardDot, -1.0f, 1.0f );
            // -1 ~ 1 -> 0 ~ 1
            // 앞 = 1, 중립 = 0.5, 뒤 = 0
            forwardInputAxis = ( forwardDot + 1.0f ) * 0.5f;
            base.MoveForward( forwardInputAxis * power );
        }

        public virtual Vector3 GetProjectileSpawnPosition( Vector3 spawnPosition, float inputDistance )
        {
            // spawnPosition + 전진 거리 + 중심 위치
            return spawnPosition + ( inputDistance * forwardInputAxis * transform.forward ) + ( Vector3.up * colliderHeight * 0.5f );
        }

        public virtual void SpawnProjectile( AnimationEvent param )
        {
            GameObject projectile = param.objectReferenceParameter as GameObject;
            if ( projectile == null )
            {
                Debug.LogError( "projectile reference is null." );
                return;
            }

            Vector3 spawnPosition = Vector3.zero;
            if ( param.stringParameter.Length > 0 )
            {
                spawnPosition = JsonUtility.FromJson<Vector3>( param.stringParameter );
            }

            GameObject newObject = null;
            float inputDistance = param.floatParameter;

            ESpawnType spawnType = ( ESpawnType )param.intParameter;
            switch ( spawnType )
            {
                case ESpawnType.ATTACH:
                {
                    newObject = Instantiate<GameObject>( projectile, GetProjectileSpawnPosition( transform.position + ( transform.rotation * spawnPosition ), inputDistance ), transform.rotation, transform );
                }
                break;

                case ESpawnType.LOCAL:
                {
                    newObject = Instantiate<GameObject>( projectile, GetProjectileSpawnPosition( transform.position + ( transform.rotation * spawnPosition ), inputDistance ), transform.rotation );
                }
                break;

                case ESpawnType.WORLD:
                {
                    newObject = Instantiate<GameObject>( projectile, GetProjectileSpawnPosition( spawnPosition, inputDistance ), transform.rotation );
                }
                break;
            }
            
            newObject.GetComponent<Projectile>().parent = this;
        }
    }
}
