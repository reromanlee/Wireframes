using System;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Holds the shapes of a container that can't draw: without a graphics device, as in server builds, or without a
    /// usable shader. Shapes keep working as usual, but nothing is stored for the GPU and nothing is uploaded.
    /// </summary>
    internal sealed class HeadlessHost : IShapeHost, IDisposable
    {
        private const int InitialCapacity = 64;

        private Shape[] _shapes = new Shape[InitialCapacity];
        private int _shapeCount;
        private int _hiddenShapeCount;
        private int _vertexCount;

        internal HeadlessHost(BoneRegistry bones)
        {
            Bones = bones;
        }

        public BoneRegistry Bones { get; }

        internal int ShapeCount
        {
            get => _shapeCount;
        }

        internal int HiddenShapeCount
        {
            get => _hiddenShapeCount;
        }

        /// <summary>Vertices the shapes would use if they were drawn.</summary>
        internal int VertexCount
        {
            get => _vertexCount;
        }

        internal void Add(Shape shape)
        {
            if (_shapeCount == _shapes.Length)
            {
                Array.Resize(ref _shapes, _shapeCount * 2);
            }
            shape.ShapeIndex = _shapeCount;
            _shapes[_shapeCount++] = shape;
            _vertexCount += shape.VertexCount;
            if (shape.IsHidden)
            {
                _hiddenShapeCount++;
            }
        }

        public void Enqueue(Shape shape)
        {
            // Nothing is drawn, so there is nothing to write; the shape stays marked and never queues again.
        }

        public void Remove(Shape shape)
        {
            int last = --_shapeCount;
            Shape moved = _shapes[last];
            _shapes[shape.ShapeIndex] = moved;
            moved.ShapeIndex = shape.ShapeIndex;
            _shapes[last] = null;
            _vertexCount -= shape.VertexCount;
            if (shape.IsHidden)
            {
                _hiddenShapeCount--;
            }
        }

        public void Show(Shape shape)
        {
            _hiddenShapeCount--;
        }

        public void Hide(Shape shape)
        {
            _hiddenShapeCount++;
        }

        public IShapeHost Reattach(Shape shape)
        {
            Add(shape);
            return this;
        }

        public void Dispose()
        {
            for (int i = 0; i < _shapeCount; i++)
            {
                _shapes[i].Detach();
                _shapes[i] = null;
            }
            _shapeCount = 0;
            _hiddenShapeCount = 0;
            _vertexCount = 0;
        }
    }
}
