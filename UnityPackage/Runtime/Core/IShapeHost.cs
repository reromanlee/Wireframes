namespace reromanlee.Wireframes
{
    /// <summary>What holds a shape once it is attached: a chunk that draws it, or a host that only keeps it.</summary>
    internal interface IShapeHost
    {
        /// <summary>Registry of the bones that the host's shapes follow.</summary>
        BoneRegistry Bones { get; }

        /// <summary>Queues <paramref name="shape"/> to be written on the next flush.</summary>
        void Enqueue(Shape shape);

        void Remove(Shape shape);
    }
}
