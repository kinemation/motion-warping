// Designed by KINEMATION, 2023

using Kinemation.MotionWarping.Runtime.Core;
using Kinemation.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Demo.Scripts
{
    public class RollComponent : MonoBehaviour, IWarpPointProvider
    {
        [Header("General")]
        [SerializeField] private float capsuleHeight;
        [SerializeField] private float capsuleRadius;
        [SerializeField] private LayerMask layerMask;
        
        [Header("Rolling")]
        [SerializeField] private MotionWarpingAsset rollingAsset;
        [SerializeField] [Min(0f)] private float rollMaxDistance;
        [SerializeField] [Min(0f)] private float rollMinDistance;
        [SerializeField] [Min(0f)] private float maxFallOff;
        
        public WarpInteractionResult Interact(GameObject instigator)
        {
            WarpInteractionResult result = new WarpInteractionResult();
            result.success = false;

            if (rollingAsset == null) return result;

            Vector3 a = transform.position;
            a.y += 0.02f;

            Vector3 b = a;
            b.y += capsuleHeight;

            bool bHit = Physics.CapsuleCast(a, b, capsuleRadius, transform.forward, out var hit, 
                rollMaxDistance, layerMask);
            
            WarpPoint point = new WarpPoint()
            {
                position = transform.position + transform.forward * rollMaxDistance,
                rotation = transform.rotation
            };

            if (bHit)
            {
                if (hit.distance < rollMinDistance) return result;
                
                a = hit.point;
                a -= transform.forward * capsuleRadius;

                bHit = Physics.SphereCast(a, capsuleRadius, -transform.up, out hit, maxFallOff,
                    layerMask);

                if (!bHit) return result;

                point.position = hit.point;
            }

            result.points = new[] { point };
            
            result.asset = rollingAsset;
            result.success = true;

            return result;
        }
    }
}
