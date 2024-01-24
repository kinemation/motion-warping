// Designed by KINEMATION, 2023

using Kinemation.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Demo.Scripts
{
    public class ExampleCameraController : MonoBehaviour
    {
        public float DeltaYaw { get; private set; }
        [Header("Controls")]
        [SerializeField, Range(0f, 2f)] private float sensitivity = 1f;
        [SerializeField, Range(0f, 10f)] private float inputSmoothing = 0f;
        
        [Header("Camera Position")]
        [SerializeField] private Transform defaultPose;
        [SerializeField] private Transform movementPose;
        [SerializeField] private Transform warpingPose;
        [SerializeField, Min(0f)] private float blendSpeed;
        [SerializeField] private Transform cameraTransform;

        [Header("Collision")] 
        [SerializeField] [Range(0f, 0.2f)] private float traceRadius;
        [SerializeField] private LayerMask traceLayerMask;
        
        private Quaternion _startRotation;
        private Quaternion _cacheRotation;
        private float _pitch;

        private Vector3 _localCameraPosition;

        private void Start()
        {
            _startRotation = transform.rotation;
        }

        public Quaternion GetAimRotation()
        {
            return _cacheRotation;
        }

        public Quaternion UpdateCameraController(bool moving, bool warping)
        {
            DeltaYaw = Input.GetAxis("Mouse X");
            
            _pitch += Input.GetAxis("Mouse Y") * sensitivity;
            _pitch = Mathf.Clamp(_pitch, -90f, 90f);
            
            _startRotation *= Quaternion.Euler(0f, DeltaYaw * sensitivity, 0f);

            if (Mathf.Approximately(inputSmoothing, 0f))
            {
                transform.rotation = _startRotation * Quaternion.Euler(-_pitch, 0f, 0f);
            }
            else
            {
                float alpha = WarpingUtility.ExpDecayAlpha(inputSmoothing, Time.deltaTime);
                Quaternion desiredRotation = _startRotation * Quaternion.Euler(-_pitch, 0f, 0f);
                desiredRotation = Quaternion.Slerp(transform.rotation, desiredRotation, alpha);
                Vector3 desiredEuler = desiredRotation.eulerAngles;
                desiredRotation = Quaternion.Euler(new Vector3(desiredEuler.x, desiredEuler.y, 0f));
                transform.rotation = desiredRotation;
            }
            
            Vector3 desiredPosition = defaultPose.position;
            desiredPosition = moving ? movementPose.position : desiredPosition;
            desiredPosition = warping ? warpingPose.position : desiredPosition;
            
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, 
                WarpingUtility.ExpDecayAlpha(blendSpeed, Time.deltaTime));
            
            Vector3 traceVector = cameraTransform.position - transform.position;

            bool bHit = Physics.SphereCast(transform.position, traceRadius, traceVector.normalized,
                out var hit, traceVector.magnitude, traceLayerMask);
            
            if (bHit)
            {
                cameraTransform.position -= traceVector.normalized * (traceVector.magnitude - hit.distance);
            }
            
            _cacheRotation = transform.rotation;
            return _cacheRotation;
        }

        public void LateUpdate()
        {
            transform.rotation = _cacheRotation;
        }
    }
}
