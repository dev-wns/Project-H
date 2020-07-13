using System;
using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region Variables       

        [Header( "Controller Input" )]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;

        [Header( "Camera Input" )]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";
        public string CameraDistanceInput = "CameraDistance";
        public float CameraDistanceSpeed = 1;

        [Header( "Action Input" )]
        public string basicAttackInput = "Attack";
        public string dodgeInput = "Dodge";
        public string targetingInput = "Targeting";

        [HideInInspector] public vThirdPersonController controller;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        #endregion

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            InitilizeController();
            InitializeTpCamera();
        }

        protected virtual void Update()
        {
            InputHandle();                  // update the input methods
            controller.UpdateAnimator();            // updates the Animator Parameters
        }

        public virtual void OnAnimatorMove()
        {
            controller.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            controller = GetComponent<vThirdPersonController>();

            if ( controller != null )
            {
                controller.Init();
            }
        }

        protected virtual void InitializeTpCamera()
        {
            if ( tpCamera != null )
            {
                return;
            }

            tpCamera = FindObjectOfType<vThirdPersonCamera>();
            if ( tpCamera == null )
            {
                return;
            }

            if ( tpCamera )
            {
                tpCamera.SetMainTarget( this.transform );
                tpCamera.Init();
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
            ActionInput();
        }

        public virtual void MoveInput()
        {
            controller.input.x = Input.GetAxis( horizontalInput );
            controller.input.z = Input.GetAxis( verticallInput );
        }

        protected virtual void CameraInput()
        {
            if ( cameraMain == null )
            {
                if ( Camera.main == null )
                {
                    Debug.LogError( "[CameraInput] MainCamera not found." );
                }
                else
                {
                    cameraMain = Camera.main;
                    controller.rotateTarget = cameraMain.transform;
                }
            }

            if ( cameraMain != null )
            {
                controller.UpdateMoveDirection( cameraMain.transform );
            }

            if ( tpCamera == null )
            {
                return;
            }

            float Y = Input.GetAxis( rotateCameraYInput );
            float X = Input.GetAxis( rotateCameraXInput );
            tpCamera.RotateCamera( X, Y );

            float distanceInput = Input.GetAxis( CameraDistanceInput );
            if ( distanceInput != 0.0f )
            {
                const float minDistance = 1.0f;
                const float maxDistance = 50.0f;
                tpCamera.defaultDistance = Mathf.Clamp( tpCamera.defaultDistance - distanceInput * CameraDistanceSpeed, minDistance, maxDistance );
            }
        }

        protected virtual void StrafeInput()
        {
            if ( Input.GetKeyDown( strafeInput ) )
            {
                controller.Strafe();
            }
        }

        protected virtual void SprintInput()
        {
            if ( Input.GetKeyDown( sprintInput ) )
            {
                controller.Sprint( true );
            }
            else if ( Input.GetKeyUp( sprintInput ) )
            {
                controller.Sprint( false );
            }
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return controller.isGrounded == true
                && controller.isJumping == false
                && controller.stopMove == false
                && controller.GroundAngle() < controller.slopeLimit;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            if ( Input.GetKeyDown( jumpInput ) == true && JumpConditions() == true )
            {
                controller.Jump();
            }
        }

        float dodgeInputTime = 0.0f;
        protected virtual void ActionInput()
        {
            if ( JumpConditions() == false )
            {
                return;
            }

            if ( Input.GetButtonDown( basicAttackInput ) == true )
            {
                controller.BasicAttack();
            }
            
            if ( Input.GetButtonUp( dodgeInput ) == true
                && Time.time - dodgeInputTime < 0.2f )
            {
                controller.DodgeAction();
            }

            if ( Input.GetButtonDown( dodgeInput ) == true )
            {
                dodgeInputTime = Time.time;
            }

            if ( Input.GetButtonDown( targetingInput ) == true )
            {
                //controller.TargetingAction();
            }
        }

        #endregion       
    }
}