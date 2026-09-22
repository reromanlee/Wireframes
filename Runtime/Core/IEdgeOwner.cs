namespace reromanlee.Wireframes
{
    /// <summary>Owner of edges in an <see cref="EdgeList"/>, told when one of its edges moves to another slot.</summary>
    internal interface IEdgeOwner
    {
        void OnEdgeMoved(int edge, int slot);
    }
}
