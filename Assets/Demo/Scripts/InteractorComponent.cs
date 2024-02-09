using Kinemation.MotionWarping.Runtime.Core;
using Kinemation.MotionWarping.Runtime.Examples;
using UnityEngine;

namespace Demo.Scripts
{
    public class InteractorComponent : MonoBehaviour
    {
        // Motion Warping references.
        private MotionWarping _warpingComponent;
        private MantleComponent _mantleComponent;
        private VaultComponent _vaultComponent;
        private RollComponent _rollComponent;
    
        private CharacterController _characterController;
        private GameObject _interactionTarget;
    
        private void OnTriggerEnter(Collider other)
        {
            _interactionTarget = other.gameObject;
        }

        private void OnTriggerExit(Collider other)
        {
            _interactionTarget = null;
        }

        private void Start()
        {
            _warpingComponent = GetComponent<MotionWarping>();
        
            _mantleComponent = GetComponent<MantleComponent>();
            _vaultComponent = GetComponent<VaultComponent>();
            _rollComponent = GetComponentInChildren<RollComponent>();
        
            _characterController = GetComponent<CharacterController>();
        }

        private void TryInteracting()
        {
            if (_interactionTarget == null && _warpingComponent.Interact(_rollComponent))
            {
                return;
            }
            
            _warpingComponent.Interact(_interactionTarget);
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

        private void Update()
        {
            if (_warpingComponent.IsActive())
            {
                return;
            }
        
            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteracting();
            }

            if (Input.GetKey(KeyCode.Space))
            {
                TryClimbing();
            }
        }
    
        // Disable collision and change movement mode here.
        public virtual void OnWarpStarted()
        {
            _characterController.enabled = false;
        }

        // Enable collision and movement.
        public virtual void OnWarpEnded()
        {
            _characterController.enabled = true;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
    }
}