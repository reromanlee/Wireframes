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

        internal HeadlessHost(BoneRegistry bones)
        {
            Bones = bones;
        }

        public BoneRegistry Bones { get; }

        internal int ShapeCount
        {
            get => _shapeCount;
        }

        internal void Add(Shape shape)
        {
            if (_shapeCount == _shapes.Length)
            {
                Array.Resize(ref _shapes, _shapeCount * 2);
            }
            shape.ShapeIndex = _shapeCount;
            _shapes[_shapeCount++] = shape;
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
        }

        public void Show(Shape shape)
        {
        }

        public void Hide(Shape shape)
        {
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
        }
    }
}
