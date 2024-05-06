// Designed by KINEMATION, 2024.

using Kinemation.MotionWarping.Runtime.Core;
using Kinemation.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Demo.Scripts
{
    public class DynamicExampleController : MonoBehaviour
    {
        [Header("Movement")] 
        [SerializeField] private float speed;
        [SerializeField] private float rotationSmoothing;
        [SerializeField] private float gravity = 9.81f;
        
        private static int MoveVertical = Animator.StringToHash("MoveVertical");
        private static int MoveHorizontal = Animator.StringToHash("MoveHorizontal");
        private static int Moving = Animator.StringToHash("Moving");

        private Vector2 _smoothInput;

        private GameObject _interactionTarget;
        private bool _wasGrounded;
        private bool _slowMoEnabled;
        
        private Animator _animator;
        private ExampleCameraController _cameraController;
        private CharacterController _characterController;
        
        private MotionWarping _warping;
        private LandComponent _landComponent;
        
        protected virtual void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _characterController = GetComponent<CharacterController>();
            _cameraController = GetComponentInChildren<ExampleCameraController>();
            
            _warping = GetComponent<MotionWarping>();
            _landComponent = GetComponent<LandComponent>();
            _animator = GetComponent<Animator>();
        }
        
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
            
            UpdateMovement();

            if (Input.GetKeyDown(KeyCode.N))
            {
                _slowMoEnabled = !_slowMoEnabled;
                Time.timeScale = _slowMoEnabled ? 0.2f : 1f;
            }
        }
        
        private void UpdateMovement()
        {
            bool isWarping = _warping.IsActive();
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
            
            if (isWarping) return;

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
                _warping.Interact(_landComponent);
            }

            _wasGrounded = isGrounded;
        }
    }
}