namespace reromanlee.Wireframes
{
    /// <summary>A container that shape components share, made for one <see cref="SharedContainerKey"/>.</summary>
    internal sealed class SharedContainer
    {
        internal SharedContainer(SharedContainerKey key, WireframeContainer container)
        {
            Key = key;
            Container = container;
        }

        internal SharedContainerKey Key { get; }

        /// <summary>
        /// The container, replaced by a new one when something else disposed it, such as a closing scene.
        /// </summary>
        internal WireframeContainer Container { get; set; }

        /// <summary>Components whose shape is in the container, or waits to go back into it.</summary>
        internal int UserCount { get; set; }
    }
}
