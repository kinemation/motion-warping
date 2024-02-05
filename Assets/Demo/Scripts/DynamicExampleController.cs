// Designed by KINEMATION, 2023

using Kinemation.MotionWarping.Runtime.Core;
using Kinemation.MotionWarping.Runtime.Examples;
using Kinemation.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Demo.Scripts
{
    public class DynamicExampleController : MonoBehaviour
    {
        private MotionWarping _warpingComponent;
        private MantleComponent _mantleComponent;
        private VaultComponent _vaultComponent;
        private RollComponent _rollComponent;
        private LandComponent _landComponent;
        private HangComponent _hangComponent;
        
        private ExampleCameraController _cameraController;
        
        [Header("Movement")] 
        [SerializeField] private float speed;
        [SerializeField] private float rotationSmoothing;
        [SerializeField] private float gravity = 9.81f;
        
        private CharacterController _characterController;
        private Animator _animator;
        
        private static int MoveVertical = Animator.StringToHash("MoveVertical");
        private static int MoveHorizontal = Animator.StringToHash("MoveHorizontal");
        private static int Moving = Animator.StringToHash("Moving");

        private Vector2 _smoothInput;

        private GameObject _interactionTarget;
        private bool _wasGrounded;
        private bool _slowMoEnabled;

        private bool _isHanging = false;

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            _warpingComponent = GetComponent<MotionWarping>();
            _mantleComponent = GetComponent<MantleComponent>();
            _vaultComponent = GetComponent<VaultComponent>();
            _cameraController = GetComponentInChildren<ExampleCameraController>();
            _rollComponent = GetComponentInChildren<RollComponent>();
            _landComponent = GetComponentInChildren<LandComponent>();
            _hangComponent = GetComponentInChildren<HangComponent>();
            
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }
        
        public void OnWarpStarted()
        {
            _characterController.enabled = false;
        }

        public void OnWarpEnded()
        {
            if (_isHanging) return;
            
            _characterController.enabled = true;
            _characterController.Move(new Vector3(0f, -1f, 0f));

            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        private void TryClimbing()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _warpingComponent.Interact(_vaultComponent);
                return;
            }

            _warpingComponent.Interact(_mantleComponent);
        }

        private void TryInteracting()
        {
            if (_warpingComponent.IsActive()) return;

            if (_interactionTarget == null && _warpingComponent.Interact(_rollComponent))
            {
                return;
            }
            
            _warpingComponent.Interact(_interactionTarget);
        }

        private void OnTriggerEnter(Collider other)
        {
            _interactionTarget = other.gameObject;
        }

        private void OnTriggerExit(Collider other)
        {
            _interactionTarget = null;
        }

        private void UpdateMovement()
        {
            bool isWarping = _warpingComponent.IsActive();
            Vector2 input = Vector2.zero;

            if (!isWarping)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                input.x = horizontal;
                input.y = vertical;
            }

            bool isMoving = !Mathf.Approximately(input.magnitude, 0f);
            
            _smoothInput = Vector2.Lerp(_smoothInput, input,
                WarpingUtility.ExpDecayAlpha(8f, Time.deltaTime));
            
            _animator.SetFloat(MoveVertical, _smoothInput.y);
            _animator.SetFloat(MoveHorizontal, _smoothInput.x);
            _animator.SetBool(Moving, isMoving);
            
            Quaternion cameraRotation = _cameraController.UpdateCameraController(isMoving, isWarping);
            
            if (isWarping || _isHanging) return;

            float warpedSpeed = Mathf.Clamp01(_smoothInput.magnitude) * speed;
            input.Normalize();

            Vector3 aimVector = cameraRotation.eulerAngles;
            aimVector.x = aimVector.z = 0f;
            
            Vector3 forward = Quaternion.Euler(aimVector) * Vector3.forward;
            Vector3 right = Quaternion.Euler(aimVector) * Vector3.right;

            Vector3 movementVector = (right * input.x + forward * input.y) * warpedSpeed;
            movementVector.y -= gravity;
            
            _characterController.Move(Time.deltaTime * movementVector);

            Quaternion desiredRotation = Quaternion.FromToRotation(Vector3.forward, forward);
            desiredRotation = Quaternion.Euler(new Vector3(0f, desiredRotation.eulerAngles.y, 0f));

            if (!Mathf.Approximately(input.magnitude, 0f))
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation,
                    WarpingUtility.ExpDecayAlpha(rotationSmoothing, Time.deltaTime));
            }
            
            float radius = _characterController.radius * 0.8f;
            Vector3 groundPoint = transform.position + transform.up * (radius * 0.7f);
            bool isGrounded = Physics.CheckSphere(groundPoint, radius);
            
            if (isGrounded != _wasGrounded)
            {
                _warpingComponent.Interact(_landComponent);
            }

            _wasGrounded = isGrounded;
        }

        private void UpdateInteraction()
        {
            if (_warpingComponent.IsActive()) return;

            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteracting();
            }

            if (Input.GetKey(KeyCode.Space))
            {
                TryClimbing();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
            
            UpdateInteraction();
            UpdateMovement();

            if (Input.GetKeyDown(KeyCode.N))
            {
                _slowMoEnabled = !_slowMoEnabled;
                Time.timeScale = _slowMoEnabled ? 0.2f : 1f;
            }
        }
    }
}