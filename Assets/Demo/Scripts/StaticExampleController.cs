// Designed by KINEMATION, 2023

using Kinemation.MotionWarping.Runtime.Core;
using Kinemation.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Demo.Scripts
{
    public class StaticExampleController : MonoBehaviour
    {
        public GameObject warpSource;
        public string animName;
        
        private Animator _animator;
        private MotionWarping _warpingComponent;
        private WarpPoint _origin;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _warpingComponent = GetComponent<MotionWarping>();
            
            _origin.Position = transform.position;
            _origin.Rotation = transform.rotation;
            
            StartWarping();
        }

        public void StartWarping()
        {
            transform.position = _origin.Position;
            transform.rotation = _origin.Rotation;

            if (!_warpingComponent.Interact(warpSource)) return;
            
            _animator.Rebind();
            _animator.Play(animName);
        }
    }
}