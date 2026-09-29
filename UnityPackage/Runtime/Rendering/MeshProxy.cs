using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Component on the container's GameObject. Right before each render, it applies queued shape edits and reads the
    /// bones' matrices, so everything that moved or changed earlier in the frame shows up in that frame. It releases every
    /// resource when its GameObject is destroyed, including when the scene unloads. Where nothing can draw, it keeps the
    /// shapes in a <see cref="HeadlessHost"/> and does no rendering work at all.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    internal sealed class MeshProxy : MonoBehaviour
    {
        private static bool _hasWarnedAboutShader;

        private readonly BoneRegistry _bones = new();
        private readonly BoneTexture _boneTexture = new();
        private ChunkAllocator _chunks;
        private HeadlessHost _headless;
        private WireframeContainer _container;
        private Material[] _materials;
        private bool _ownsMaterials;
        private bool _isShutDown;
        private bool _hasLoggedFlushFailure;

        /// <summary>Makes new containers act as they do without a graphics device, so tests can cover server builds.</summary>
        internal static bool SimulateNoGraphics { get; set; }

        /// <summary>True when the container draws nothing, because there is no graphics device or no usable shader.</summary>
        internal bool IsHeadless
        {
            get => _headless != null;
        }

        internal IReadOnlyList<MeshChunk> Chunks
        {
            get => _chunks != null ? _chunks.Chunks : Array.Empty<MeshChunk>();
        }

        internal ChunkAllocator ChunkAllocator
        {
            get => _chunks;
        }

        internal HeadlessHost HeadlessHost
        {
            get => _headless;
        }

        /// <summary>The materials every chunk draws with; null when headless.</summary>
        internal Material[] Materials
        {
            get => _materials;
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
            if (!TryCreateMaterials(settings))
            {
                _headless = new HeadlessHost(_bones);
                return;
            }
            _chunks = new ChunkAllocator(transform, _materials, settings.Layer, _bones);
            _chunks.Reserve(settings.VertexCapacity, settings.EdgeCapacity);
            // Uploads the bone texture and hands it to the chunks.
            Flush();
        }

        /// <summary>Attaches a newly built <paramref name="shape"/> and returns it. If that fails, nothing of it remains.</summary>
        internal T Add<T>(T shape) where T : Shape
        {
            shape.Attach(this);
            return shape;
        }

        /// <summary>Hands <paramref name="shape"/> to a chunk, or to the headless host, and returns it.</summary>
        internal IShapeHost Attach(Shape shape)
        {
            if (_headless != null)
            {
                _headless.Add(shape);
                return _headless;
            }
            return _chunks.Attach(shape);
        }

        /// <summary>Reads the bones, writes queued shapes and uploads what changed.</summary>
        internal void Flush()
        {
            if (_chunks == null)
            {
                return;
            }
            _bones.ReadMatrices();
            if (_boneTexture.Upload(_bones))
            {
                _chunks.SetBoneTexture(_boneTexture.Texture);
            }
            _chunks.Flush();
        }

        internal void Shutdown()
        {
            if (_isShutDown)
            {
                return;
            }
            _isShutDown = true;
            // The container learns first, so it reads as disposed even if releasing something below throws.
            _container?.OnProxyShutdown();
            _chunks?.Dispose();
            _headless?.Dispose();
            _boneTexture.Dispose();
            if (_ownsMaterials)
            {
                foreach (Material material in _materials)
                {
                    UnityObjects.Destroy(material);
                }
            }
            _materials = null;
        }

        /// <summary>
        /// Picks the materials to draw with, or returns false when nothing can draw: without a graphics device, silently,
        /// as that is expected in server builds, and without a usable shader, with one warning per session.
        /// </summary>
        private bool TryCreateMaterials(WireframeContainerSettings settings)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || SimulateNoGraphics)
            {
                return false;
            }
            if (settings.Material != null)
            {
                _materials = new[] { settings.Material };
                return true;
            }
            // The shader sits in a Resources folder, which keeps it in player builds.
            Shader shader = Shader.Find(WireframeMaterials.ShaderName);
            if (shader == null || !shader.isSupported)
            {
                WarnAboutShader(shader == null
                    ? $"The shader '{WireframeMaterials.ShaderName}' isn't in this build"
                    : $"The shader '{WireframeMaterials.ShaderName}' isn't supported on {SystemInfo.graphicsDeviceType}");
                return false;
            }
            _materials = WireframeMaterials.Create(shader, settings.Occlusion, settings.UseAlpha);
            _ownsMaterials = true;
            return true;
        }

        private void WarnAboutShader(string problem)
        {
            if (_hasWarnedAboutShader)
            {
                return;
            }
            _hasWarnedAboutShader = true;
            WireframesLog.Warning($"{problem}, so wireframes won't be drawn. Their shapes still work.", this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetWarnings()
        {
            // Needed when domain reload is disabled, so each Play Mode session warns again.
            _hasWarnedAboutShader = false;
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
            if (_isShutDown || _chunks == null)
            {
                return;
            }
            try
            {
                Flush();
                // Play Mode only, where destroying waits for the end of the frame and is allowed in a render callback.
                if (Application.isPlaying)
                {
                    _chunks.ReleaseIdleChunks(Time.realtimeSinceStartup);
                }
            }
            catch (Exception exception)
            {
                // An exception must never escape into Unity's rendering, nor flood the console once every frame.
                if (!_hasLoggedFlushFailure)
                {
                    _hasLoggedFlushFailure = true;
                    WireframesLog.Error("Updating the wireframes before a render failed.", exception, this);
                }
            }
        }
    }
}
