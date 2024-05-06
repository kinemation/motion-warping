// Designed by KINEMATION, 2023

using Kinemation.MotionWarping.Runtime.Core;
using Kinemation.MotionWarping.Runtime.Utility;
using UnityEngine;

namespace Demo.Scripts
{
    public class LandComponent : MonoBehaviour, IWarpPointProvider
    {
        [SerializeField] private MotionWarpingAsset hardLanding;
        [SerializeField] private MotionWarpingAsset softLanding;
        
        [Header("Landing")]
        [SerializeField] [Min(0f)] private float hardLandingHeight;
        [SerializeField] [Min(0f)] private float softLandingHeight;
        [SerializeField] [Min(0f)] private float minAllowedHeight;
        
        [Header("Capsule")]
        [SerializeField] [Min(0f)] private float capsuleRadius;
        
        [SerializeField] [Min(0f)] private float forwardOffset;
        [SerializeField] private LayerMask layerMask;

        public WarpInteractionResult Interact(GameObject instigator)
        {
            WarpInteractionResult result = new WarpInteractionResult()
            {
                asset = null,
                points = null,
                success = false,
            };
            
            if (hardLanding == null)
            {
                return result;
            }
            
            Vector3 origin = transform.position;
            bool bHit = Physics.Raycast(origin, -transform.up, out var hit, hardLandingHeight, layerMask);

            if (!bHit || hit.distance < minAllowedHeight)
            {
                return result;
            }

            float hitHeight = hit.distance;
            Vector3 targetPosition = hit.point;

            origin = hit.point;
            
            Vector3 end = origin;
            end.y += hit.distance;

            origin.y += 0.02f;
            
            bHit = Physics.CapsuleCast(origin, end, capsuleRadius, transform.forward,
                out hit, forwardOffset, layerMask);
            
            if (bHit)
            {
                targetPosition -= transform.forward * forwardOffset;
            }
            else
            {
                targetPosition += transform.forward * forwardOffset;
            }

            WarpPoint targetPoint = new WarpPoint()
            {
                position = targetPosition,
                rotation = transform.rotation
            };
            
            result.asset = hitHeight < softLandingHeight ? softLanding : hardLanding;
            result.points = new[] { targetPoint };
            result.success = true;
            
            return result;
        }
    }
}
