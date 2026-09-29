using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Component on the container's GameObject. Right before each render, it applies queued shape edits and reads the
    /// bones' matrices, so everything that moved or changed earlier in the frame shows up in that frame. It releases every
    /// resource when its GameObject is destroyed, including when the scene unloads.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    internal sealed class MeshProxy : MonoBehaviour
    {
        private readonly List<MeshChunk> _chunks = new();
        private readonly BoneRegistry _bones = new();
        private readonly BoneTexture _boneTexture = new();
        private MaterialPropertyBlock _propertyBlock;
        private WireframeContainer _container;
        private Material[] _materials;
        private bool _ownsMaterials;
        private bool _isShutDown;

        internal IReadOnlyList<MeshChunk> Chunks
        {
            get => _chunks;
        }

        internal BoneRegistry Bones
        {
            get => _bones;
        }

        internal BoneTexture BoneTexture
        {
            get => _boneTexture;
        }

        internal void Initialize(WireframeContainer container, WireframeContainerSettings settings)
        {
            _container = container;
            if (settings.Material != null)
            {
                _materials = new[] { settings.Material };
            }
            else
            {
                // The shader sits in a Resources folder, which keeps it in player builds.
                Shader shader = Shader.Find(WireframeMaterials.ShaderName);
                if (shader == null)
                {
                    throw new InvalidOperationException($"Shader '{WireframeMaterials.ShaderName}' was not found.");
                }
                _materials = WireframeMaterials.Create(shader, settings.Occlusion, settings.UseAlpha);
                _ownsMaterials = true;
            }
            _propertyBlock = new MaterialPropertyBlock();
            _chunks.Add(new MeshChunk(transform, _materials, settings, _bones));
            // Uploads the bone texture and hands it to the renderers.
            Flush();
        }

        /// <summary>Adds <paramref name="shape"/> to a chunk and returns that chunk.</summary>
        internal MeshChunk Attach(Shape shape)
        {
            // One chunk covers every size a device can hold; choosing a chunk with room would go here.
            MeshChunk chunk = _chunks[0];
            chunk.Add(shape);
            return chunk;
        }

        /// <summary>Reads the bones, writes queued shapes and uploads what changed.</summary>
        internal void Flush()
        {
            _bones.ReadMatrices();
            for (int i = 0; i < _chunks.Count; i++)
            {
                _chunks[i].Flush();
            }
            if (_boneTexture.Upload(_bones))
            {
                _propertyBlock.SetTexture(BoneTexture.PropertyId, _boneTexture.Texture);
                for (int i = 0; i < _chunks.Count; i++)
                {
                    _chunks[i].Renderer.SetPropertyBlock(_propertyBlock);
                }
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
            _boneTexture.Dispose();
            if (_ownsMaterials)
            {
                foreach (Material material in _materials)
                {
                    UnityObjects.Destroy(material);
                }
            }
            _materials = null;
            if (_container != null)
            {
                _container.OnProxyShutdown();
            }
        }

        private void OnEnable()
        {
            // Each fires only under its own pipeline, so both stay subscribed and a pipeline switch needs no handling.
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            Camera.onPreCull += OnPreCullCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            Camera.onPreCull -= OnPreCullCamera;
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            FlushBeforeRendering();
        }

        private void OnPreCullCamera(Camera camera)
        {
            FlushBeforeRendering();
        }

        private void FlushBeforeRendering()
        {
            if (_isShutDown)
            {
                return;
            }
            try
            {
                Flush();
            }
            catch (Exception exception)
            {
                // An exception must never escape into Unity's rendering.
                Debug.LogException(exception, this);
            }
        }
    }
}
