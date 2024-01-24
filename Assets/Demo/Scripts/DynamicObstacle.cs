using UnityEngine;

namespace Demo.Scripts
{
    public class DynamicObstacle : MonoBehaviour
    {
        [SerializeField] private Transform platform;
        [SerializeField] [Min(0f)] private Vector3 direction;
        [SerializeField] [Min(0f)] private float distance;
        [SerializeField] [Min(0f)] private float speed;
        [SerializeField] [Min(0f)] private float delay;

        private bool _updatePlatform;
        private float _platformPlayback;
        private bool _goUp;

        private void Start()
        {
            _updatePlatform = true;
            _goUp = true;
        }

        private void ResetTimer()
        {
            _updatePlatform = true;
            _goUp = !_goUp;
            _platformPlayback = 0f;
        }

        private void Update()
        {
            if (!_updatePlatform) return;

            float alpha = -(Mathf.Cos(Mathf.PI * _platformPlayback) - 1) / 2f;
            float offset = Mathf.Lerp(0f, distance, _goUp ? alpha : 1f - alpha);
            platform.position = transform.position + direction * offset;

            _platformPlayback += Time.deltaTime * speed;
            _platformPlayback = Mathf.Clamp01(_platformPlayback);

            if (Mathf.Approximately(_platformPlayback, 1f))
            {
                _updatePlatform = false;
                Invoke(nameof(ResetTimer), delay);
            }
        }
    }
}
