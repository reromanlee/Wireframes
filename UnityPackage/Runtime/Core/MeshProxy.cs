using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Component on the container's GameObject. It applies queued shape edits once per frame and releases every
    /// resource when its GameObject is destroyed, including when the scene unloads.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrder)]
    internal sealed class MeshProxy : MonoBehaviour
    {
        // After default-order scripts' LateUpdate, and still before Unity skins meshes later in the frame.
        private const int ExecutionOrder = 32000;
        private const string ShaderName = "reromanlee/Wireframes/VertexColors";

        private readonly List<MeshChunk> _chunks = new();
        private LineContainer _container;
        private Material _material;
        private bool _ownsMaterial;
        private bool _isShutDown;

        internal IReadOnlyList<MeshChunk> Chunks
        {
            get => _chunks;
        }

        internal void Initialize(LineContainer container, Material material)
        {
            _container = container;
            if (material != null)
            {
                _material = material;
            }
            else
            {
                // The shader sits in a Resources folder, which keeps it in player builds.
                Shader shader = Shader.Find(ShaderName);
                if (shader == null)
                {
                    throw new InvalidOperationException($"Shader '{ShaderName}' was not found.");
                }
                _material = new Material(shader) { name = ShaderName };
                _ownsMaterial = true;
            }
            _chunks.Add(new MeshChunk(transform, _material));
        }

        /// <summary>Adds <paramref name="shape"/> to a chunk and returns that chunk.</summary>
        internal MeshChunk Attach(Shape shape)
        {
            // One chunk covers every size a device can hold; choosing a chunk with room would go here.
            MeshChunk chunk = _chunks[0];
            chunk.Add(shape);
            return chunk;
        }

        internal void Flush()
        {
            for (int i = 0; i < _chunks.Count; i++)
            {
                _chunks[i].Flush();
            }
        }

        internal void Shutdown()
        {
            if (_isShutDown)
            {
                return;
            }
            _isShutDown = true;
            for (int i = 0; i < _chunks.Count; i++)
            {
                _chunks[i].Dispose();
            }
            _chunks.Clear();
            if (_ownsMaterial)
            {
                UnityObjects.Destroy(_material);
            }
            _material = null;
            if (_container != null)
            {
                _container.OnProxyShutdown();
            }
        }

        private void LateUpdate()
        {
            Flush();
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
