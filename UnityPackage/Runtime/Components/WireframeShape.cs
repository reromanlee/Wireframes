using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of the components that draw a shape on their GameObject, the same in Edit Mode, Play Mode and builds. The
    /// shape follows the GameObject's Transform like a mesh, and every change made to the component, in the Inspector,
    /// by undo, animation or a script, shows up in the next render. Disabling the component or its GameObject hides the
    /// shape, which then costs nothing, and enabling it again allocates nothing.
    /// </summary>
    /// <remarks>
    /// Components share containers, one per combination of <see cref="Occlusion"/>, transparency, layer and
    /// <see cref="DrawAsGizmo"/>, so many shapes draw in a few draw calls. A shape draws on its GameObject's layer, and a
    /// <see cref="Color"/> with alpha below 1 draws it transparent. Main thread only; shape components are made by the
    /// package only.
    /// </remarks>
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public abstract class WireframeShape : MonoBehaviour
    {
        internal const string HelpUrl = "https://github.com/reromanlee/Wireframes/blob/main/Documentation/USAGE.md#components";

        [Tooltip("Color of the shape's lines. Alpha below 1 draws them transparent.")]
        [SerializeField] private Color _color = Color.white;

        [Tooltip("What is drawn of the lines that other geometry hides: nothing, everything, or a dimmer line.")]
        [SerializeField] private WireframeOcclusion _occlusion = WireframeOcclusion.Hide;

        [Tooltip("Draws the shape like a gizmo: in the Scene view, and in the Game view only while its Gizmos button is on. "
                 + "Builds leave it out.")]
        [SerializeField] private bool _drawAsGizmo;

        private Shape _shape;
        private SharedContainer _container;
        private int _builtSignature;
        private bool _hasReportedProblem;

        private protected WireframeShape()
        {
        }

        /// <summary>Color of the shape's lines. Alpha below 1 draws them transparent. White by default.</summary>
        public Color Color
        {
            get => _color;
            set
            {
                // Crossing alpha 1 moves the shape between an opaque and a transparent container.
                bool keepsContainer = (_color.a < 1f) == (value.a < 1f);
                _color = value;
                if (!keepsContainer)
                {
                    Refresh();
                }
                else if (TryGetShape(out Shape shape))
                {
                    shape.SetColor(value);
                }
            }
        }

        /// <summary>
        /// What is drawn of the lines that other geometry hides: nothing, everything, or a dimmer line.
        /// <see cref="WireframeOcclusion.Hide"/> by default.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeOcclusion"/>.</exception>
        public WireframeOcclusion Occlusion
        {
            get => _occlusion;
            set
            {
                if (!IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value, $"{nameof(Occlusion)} isn't a {nameof(WireframeOcclusion)} value.");
                }
                _occlusion = value;
                Refresh();
            }
        }

        /// <summary>
        /// True draws the shape like a gizmo: in Scene views, and in the Game view only while its Gizmos button is on. Other
        /// cameras never draw it, and builds create nothing for it. False, the default, draws it like any other object.
        /// </summary>
        public bool DrawAsGizmo
        {
            get => _drawAsGizmo;
            set
            {
                _drawAsGizmo = value;
                Refresh();
            }
        }

        /// <summary>Position in the list of enabled components, or -1 while the component is disabled.</summary>
        internal int EnabledIndex { get; set; } = -1;

        /// <summary>True while a refresh that OnValidate put off waits for the editor's next update.</summary>
        internal bool IsDeferred { get; set; }

        /// <summary>True when the GameObject's layer changed since the shape went into its container.</summary>
        internal bool IsOnStaleLayer
        {
            get => _container != null && _container.Key.Layer != gameObject.layer;
        }

        /// <summary>The runtime shape that draws the component, or null when there is none yet.</summary>
        internal Shape Shape
        {
            get => _shape;
        }

        /// <summary>The container the shape is in or goes back into, or null when there is none yet.</summary>
        internal SharedContainer SharedContainer
        {
            get => _container;
        }

        /// <summary>
        /// A number that changes whenever the fields need a new runtime shape, such as when a count that shapes fix at
        /// creation changes.
        /// </summary>
        internal virtual int BuildSignature
        {
            get => 0;
        }

        /// <summary>
        /// False while the fields can't make a shape, such as a polyline with too few points; nothing is drawn then.
        /// </summary>
        internal virtual bool CanCreateShape
        {
            get => true;
        }

        /// <summary>
        /// False when nothing is drawn: for fields that can't make a shape, or for a gizmo outside the Editor.
        /// </summary>
        private bool IsDrawn
        {
#if UNITY_EDITOR
            get => CanCreateShape;
#else
            get => !_drawAsGizmo && CanCreateShape;
#endif
        }

        /// <summary>
        /// Brings the shape in line with the fields while the component is enabled: creates it, moves it into another
        /// container, creates it again after a count changed, or writes the fields into it. A disabled component does
        /// nothing and catches up once it is enabled. A failure is logged once and leaves nothing drawn.
        /// </summary>
        internal void Refresh()
        {
            if (EnabledIndex < 0)
            {
                return;
            }
            try
            {
                RefreshEnabled();
                _hasReportedProblem = false;
                ShapeComponents.RequestRepaint();
            }
            catch (Exception exception)
            {
                DisposeShape();
                ReportProblem(exception);
            }
        }

        /// <summary>
        /// Writes the fields into the drawn shape, for callbacks where creating and destroying objects is forbidden, such
        /// as rendering. A refresh that needs more waits for the editor's next update, or for the next refresh in builds.
        /// </summary>
        internal void RefreshInPlace()
        {
            if (EnabledIndex < 0)
            {
                return;
            }
            if (!NeedsStructuralRefresh())
            {
                ApplyFields();
                return;
            }
#if UNITY_EDITOR
            ShapeComponents.Defer(this);
#endif
        }

        /// <summary>
        /// Returns the drawn shape for an edit that can go straight into it. Otherwise refreshes, which catches up a
        /// component that lost its shape and does nothing while it is disabled, and returns false.
        /// </summary>
        internal bool TryGetShape<T>(out T shape) where T : Shape
        {
            if (_shape is T drawn && !drawn.IsDisposed && !drawn.IsSuspended)
            {
                shape = drawn;
                ShapeComponents.RequestRepaint();
                return true;
            }
            shape = null;
            Refresh();
            return false;
        }

        /// <summary>
        /// Creates the runtime shape from the fields, following <paramref name="bone"/>, the GameObject's Transform.
        /// </summary>
        internal abstract Shape CreateShape(Transform bone);

        /// <summary>
        /// Writes every field except the color into <paramref name="shape"/>, made by <see cref="CreateShape"/>.
        /// </summary>
        internal abstract void ApplyTo(Shape shape);

        /// <summary>
        /// Brings fields that the runtime shape can't take, such as counts out of range, to their nearest valid value.
        /// </summary>
        internal virtual void Sanitize()
        {
        }

        private void OnEnable()
        {
            ShapeComponents.Add(this);
            Refresh();
        }

        private void OnDisable()
        {
            ShapeComponents.Remove(this);
            if (_shape != null && !_shape.IsDisposed && !_shape.IsSuspended)
            {
                _shape.Suspend();
            }
        }

        private void OnDestroy()
        {
            DisposeShape();
        }

        private void OnValidate()
        {
            SanitizeFields();
            if (EnabledIndex < 0)
            {
                return;
            }
            // Unity forbids creating and destroying objects here, which moving the shape or creating it may do.
            if (NeedsStructuralRefresh())
            {
                ShapeComponents.Defer(this);
                return;
            }
            ApplyFields();
        }

        private void OnDidApplyAnimationProperties()
        {
            if (EnabledIndex < 0)
            {
                return;
            }
            if (!NeedsStructuralRefresh())
            {
                ApplyFields();
                return;
            }
#if UNITY_EDITOR
            // Previewing an animation in Edit Mode samples it where destroying objects may be forbidden.
            if (!Application.isPlaying)
            {
                ShapeComponents.Defer(this);
                return;
            }
#endif
            Refresh();
        }

        private void RefreshEnabled()
        {
            SanitizeFields();
            if (_shape != null && _shape.IsDisposed)
            {
                // Its container went away under it, as when its scene closed, so it is created again.
                _shape = null;
            }
            if (!IsDrawn)
            {
                _shape?.Dispose();
                _shape = null;
                return;
            }

            SharedContainerKey key = CurrentKey();
            if (_container == null || !_container.Key.Equals(key) || _container.Container.IsDisposed)
            {
                // Acquired before the old one is released, so a container that both use is never disposed in between.
                SharedContainer next = SharedContainers.Acquire(key);
                if (_shape != null && !_shape.IsSuspended)
                {
                    _shape.Suspend();
                }
                if (_container != null)
                {
                    SharedContainers.Release(_container);
                }
                _container = next;
            }

            if (_shape != null && _builtSignature != BuildSignature)
            {
                _shape.Dispose();
                _shape = null;
            }
            MeshProxy proxy = _container.Container.Proxy;
            if (_shape == null)
            {
                _builtSignature = BuildSignature;
                _shape = proxy.Add(CreateShape(transform));
            }
            else if (_shape.IsSuspended)
            {
                _shape.Resume(proxy);
            }
            _shape.SetColor(_color);
            ApplyTo(_shape);
        }

        /// <summary>
        /// Writes the fields into the drawn shape, for changes that need neither a new shape nor another container.
        /// </summary>
        private void ApplyFields()
        {
            try
            {
                _shape.SetColor(_color);
                ApplyTo(_shape);
                ShapeComponents.RequestRepaint();
            }
            catch (Exception exception)
            {
                ReportProblem(exception);
#if UNITY_EDITOR
                // OnValidate forbids destroying objects, so the refresh that clears the shape away comes later.
                ShapeComponents.Defer(this);
#else
                DisposeShape();
#endif
            }
        }

        /// <summary>True when the fields need more than writing them into the drawn shape.</summary>
        private bool NeedsStructuralRefresh()
        {
            return _shape == null || _shape.IsDisposed || _shape.IsSuspended || !IsDrawn
                   || _container.Container.IsDisposed || !_container.Key.Equals(CurrentKey())
                   || _builtSignature != BuildSignature;
        }

        private SharedContainerKey CurrentKey()
        {
            Scene stage = SharedContainers.StageOf(gameObject);
            return new SharedContainerKey(stage, _occlusion, _color.a < 1f, gameObject.layer, _drawAsGizmo);
        }

        private void SanitizeFields()
        {
            if (!IsDefined(_occlusion))
            {
                _occlusion = WireframeOcclusion.Hide;
            }
            Sanitize();
        }

        private void DisposeShape()
        {
            Shape shape = _shape;
            SharedContainer container = _container;
            _shape = null;
            _container = null;
            shape?.Dispose();
            if (container != null)
            {
                SharedContainers.Release(container);
            }
        }

        private void ReportProblem(Exception exception)
        {
            if (_hasReportedProblem)
            {
                return;
            }
            _hasReportedProblem = true;
            WireframesLog.Error($"The {GetType().Name} on '{name}' failed to update, so it isn't drawn.", exception, this);
        }

        private static bool IsDefined(WireframeOcclusion occlusion)
        {
            return occlusion >= WireframeOcclusion.Hide && occlusion <= WireframeOcclusion.Fade;
        }
    }
}
