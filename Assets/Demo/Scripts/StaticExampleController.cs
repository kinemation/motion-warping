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
        private MotionWarping _warping;
        private WarpPoint _origin;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _warping = GetComponent<MotionWarping>();
            
            _origin.position = transform.position;
            _origin.rotation = transform.rotation;
            
            StartWarping();
        }

        public void StartWarping()
        {
            transform.position = _origin.position;
            transform.rotation = _origin.rotation;

            if (!_warping.Interact(warpSource)) return;
            
            _animator.Rebind();
            _animator.Play(animName);
        }
    }
}