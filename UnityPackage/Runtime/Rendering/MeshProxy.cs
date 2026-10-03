using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Component on the container's GameObject, in Play and Edit Mode alike. Right before each render, it applies queued
    /// shape edits and reads the bones' matrices, so everything that moved or changed earlier in the frame shows up in
    /// that frame. It releases every resource when its GameObject is destroyed, including when the scene unloads. Where
    /// nothing can draw, it keeps the shapes in a <see cref="HeadlessHost"/> and does no rendering work at all.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    internal sealed class MeshProxy : MonoBehaviour
    {
        private static readonly List<MeshProxy> Live = new();
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

        /// <summary>The proxy of every container that isn't disposed yet, for editor code that manages their lifetime.</summary>
        internal static IReadOnlyList<MeshProxy> LiveProxies
        {
            get => Live;
        }

        internal WireframeContainer Container
        {
            get => _container;
        }

        /// <summary>True for a container created in Edit Mode, which the editor disposes itself.</summary>
        internal bool IsEditMode { get; private set; }

        internal bool PersistsAcrossScenes { get; private set; }

        /// <summary>
        /// The scene the container belongs to. It still identifies that scene after the proxy leaves it, as it does when
        /// Play Mode reloads the scene and the proxy, never saved, stays outside any scene.
        /// </summary>
        internal Scene HomeScene { get; set; }

        /// <summary>True when the container draws nothing, because there is no graphics device or no usable shader.</summary>
        internal bool IsHeadless
        {
            get => _headless != null;
        }

        /// <summary>
        /// False deactivates the container's GameObject: its chunks aren't drawn and, with the render callbacks it drops,
        /// nothing is flushed until it is shown again, when every edit made meanwhile is applied.
        /// </summary>
        internal bool IsVisible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
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

        /// <summary>
        /// Called during a flush whose bones turned out to include destroyed ones, right after they are read, so the
        /// shapes on them can be changed in time for the upload. Containers that shape components share use it.
        /// </summary>
        internal Action<MeshProxy> BonesDestroyed { get; set; }

        internal void Initialize(WireframeContainer container, WireframeContainerSettings settings)
        {
            _container = container;
            IsEditMode = !Application.isPlaying;
            PersistsAcrossScenes = settings.PersistAcrossScenes;
            HomeScene = gameObject.scene;
            Live.Add(this);
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

        /// <summary>
        /// Releases the chunks that stayed empty long enough; see <see cref="ChunkAllocator.ReleaseIdleChunks"/> for where
        /// this may run.
        /// </summary>
        internal void ReleaseIdleChunks(float time)
        {
            if (!_isShutDown)
            {
                _chunks?.ReleaseIdleChunks(time);
            }
        }

        /// <summary>Counts what the container holds by walking its chunks, without allocating.</summary>
        internal WireframeStatistics GetStatistics()
        {
            if (_headless != null)
            {
                return new WireframeStatistics(
                    _headless.ShapeCount, _headless.HiddenShapeCount, _headless.VertexCount, 0, _bones.Count, 0,
                    _bones.CpuMemory, 0);
            }
            int shapes = 0;
            int hiddenShapes = 0;
            int vertices = 0;
            int edges = 0;
            long cpuMemory = _bones.CpuMemory + _boneTexture.Memory;
            long gpuMemory = _boneTexture.Memory;
            IReadOnlyList<MeshChunk> chunks = _chunks.Chunks;
            for (int i = 0; i < chunks.Count; i++)
            {
                MeshChunk chunk = chunks[i];
                shapes += chunk.ShapeCount;
                hiddenShapes += chunk.HiddenShapeCount;
                vertices += chunk.UsedVertexCount;
                edges += chunk.EdgeCount;
                cpuMemory += chunk.CpuMemory;
                gpuMemory += chunk.GpuMemory;
            }
            return new WireframeStatistics(
                shapes, hiddenShapes, vertices, edges, _bones.Count, chunks.Count, cpuMemory, gpuMemory);
        }

        /// <summary>Reads the bones, writes queued shapes and uploads what changed.</summary>
        internal void Flush()
        {
            if (_chunks == null)
            {
                return;
            }
            using (WireframesMarkers.Flush.Auto())
            {
                _bones.ReadMatrices();
                if (_bones.TakeDestroyedBones())
                {
                    BonesDestroyed?.Invoke(this);
                }
                if (_boneTexture.Upload(_bones))
                {
                    _chunks.SetBoneTexture(_boneTexture.Texture);
                }
                _chunks.Flush();
            }
        }

        internal void Shutdown()
        {
            if (_isShutDown)
            {
                return;
            }
            _isShutDown = true;
            Live.Remove(this);
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
            if (_isShutDown)
            {
                return;
            }
            try
            {
                WireframesCounters.Update();
                // Without chunks there is nothing to draw, as in a container whose shapes are all gone for now.
                if (_chunks == null || _chunks.Chunks.Count == 0)
                {
                    return;
                }
                Flush();
                // Play Mode only, where destroying waits for the end of the frame and is allowed in a render callback.
                // In Edit Mode, the editor's update loop releases them instead.
                if (Application.isPlaying)
                {
                    ReleaseIdleChunks(Time.realtimeSinceStartup);
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
