using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// How a <see cref="WireframeContainer"/> is created. It is serializable, so a script can expose it in the Inspector.
    /// </summary>
    [Serializable]
    public sealed class WireframeContainerSettings
    {
        private const string DefaultName = "Wireframes";
        private const int MaxLayer = 31;

        [Tooltip("Name of the container's GameObject in the Hierarchy.")]
        [SerializeField] private string _name = DefaultName;

        [Tooltip("Material to draw with. None uses the package's vertex color material. The container never destroys it.")]
        [SerializeField] private Material _material;

        [Tooltip("Layer of the container's GameObjects, so cameras can include or exclude wireframes with their culling mask.")]
        [SerializeField, Range(0, MaxLayer)] private int _layer;

        [Tooltip("Keeps the container when other scenes load, instead of disposing it with the scene it was created in.")]
        [SerializeField] private bool _persistAcrossScenes;

        [Tooltip("Vertices reserved up front, so a load known in advance never grows the buffers later.")]
        [SerializeField, Min(0)] private int _vertexCapacity;

        [Tooltip("Edges reserved up front, so a load known in advance never grows the buffers later.")]
        [SerializeField, Min(0)] private int _edgeCapacity;

        [Tooltip("What is drawn of lines that other geometry hides. Ignored with a custom material.")]
        [SerializeField] private WireframeOcclusion _occlusion = WireframeOcclusion.Hide;

        [Tooltip("Blends each color by its alpha. Ignored with a custom material.")]
        [SerializeField] private bool _useAlpha;

        /// <summary>Name of the container's GameObject in the Hierarchy. Null or empty uses "Wireframes".</summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// Material to draw with, or null for the package's vertex color material. The container never destroys a
        /// material passed here.
        /// </summary>
        public Material Material
        {
            get => _material;
            set => _material = value;
        }

        /// <summary>
        /// Layer of the container's GameObjects, from 0 to 31, so cameras can include or exclude wireframes with their
        /// culling mask.
        /// </summary>
        public int Layer
        {
            get => _layer;
            set => _layer = value;
        }

        /// <summary>
        /// True keeps the container when other scenes load in Play mode, like <c>Object.DontDestroyOnLoad</c>. False, the
        /// default, disposes it with the scene that was active when it was created.
        /// </summary>
        public bool PersistAcrossScenes
        {
            get => _persistAcrossScenes;
            set => _persistAcrossScenes = value;
        }

        /// <summary>Vertices reserved up front, 0 or more, so a load known in advance never grows the buffers later.</summary>
        public int VertexCapacity
        {
            get => _vertexCapacity;
            set => _vertexCapacity = value;
        }

        /// <summary>Edges reserved up front, 0 or more, so a load known in advance never grows the buffers later.</summary>
        public int EdgeCapacity
        {
            get => _edgeCapacity;
            set => _edgeCapacity = value;
        }

        /// <summary>
        /// What is drawn of lines that other geometry hides: nothing (the default), everything, or a dimmer line. It
        /// applies to the package's material and is ignored with a custom <see cref="Material"/>.
        /// </summary>
        public WireframeOcclusion Occlusion
        {
            get => _occlusion;
            set => _occlusion = value;
        }

        /// <summary>
        /// True blends each color by its alpha, drawing the lines as transparent; false, the default, draws them opaque.
        /// It applies to the package's material and is ignored with a custom <see cref="Material"/>.
        /// </summary>
        public bool UseAlpha
        {
            get => _useAlpha;
            set => _useAlpha = value;
        }

        internal string ResolvedName
        {
            get => string.IsNullOrEmpty(_name) ? DefaultName : _name;
        }

        /// <summary>Throws for settings outside their valid range, naming <paramref name="parameterName"/> as the argument.</summary>
        internal void Validate(string parameterName)
        {
            if (_layer < 0 || _layer > MaxLayer)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName, _layer, $"{nameof(Layer)} must be between 0 and {MaxLayer}.");
            }
            if (_vertexCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName, _vertexCapacity, $"{nameof(VertexCapacity)} can't be negative.");
            }
            if (_edgeCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName, _edgeCapacity, $"{nameof(EdgeCapacity)} can't be negative.");
            }
            if (_occlusion < WireframeOcclusion.Hide || _occlusion > WireframeOcclusion.Fade)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName, _occlusion, $"{nameof(Occlusion)} isn't a {nameof(WireframeOcclusion)} value.");
            }
        }
    }
}
