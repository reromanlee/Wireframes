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

        /// <summary>Draws the edges of <paramref name="shape"/> again after <see cref="Hide"/>.</summary>
        void Show(Shape shape);

        /// <summary>Stops drawing the edges of <paramref name="shape"/>, keeping everything else it has.</summary>
        void Hide(Shape shape);

        /// <summary>
        /// Holds <paramref name="shape"/> again after it was removed from this host to change its size, and returns the
        /// host that holds it now: this one when it has room, another one otherwise.
        /// </summary>
        IShapeHost Reattach(Shape shape);
    }
}
