using UnityEngine;

namespace reromanlee.Wireframes.Samples
{
    /// <summary>
    /// Turns each child around the world's up axis in Play Mode. In the ComponentGallery scene, it shows that shape
    /// components follow their GameObjects with no work of their own.
    /// </summary>
    public sealed class Turntable : MonoBehaviour
    {
        [Tooltip("Degrees per second each child turns around the world's up axis.")]
        [SerializeField] private float _turnSpeed = 30f;

        private void Update()
        {
            float angle = _turnSpeed * Time.deltaTime;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).Rotate(0f, angle, 0f, Space.World);
            }
        }
    }
}
