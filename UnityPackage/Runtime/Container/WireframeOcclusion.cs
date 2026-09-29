namespace reromanlee.Wireframes
{
    /// <summary>What a container draws of lines that other geometry hides.</summary>
    public enum WireframeOcclusion
    {
        /// <summary>Hidden parts are not drawn, like any other object.</summary>
        Hide,

        /// <summary>Lines are drawn over everything, so hidden parts show at full strength.</summary>
        Show,

        /// <summary>Hidden parts are drawn dimmer, like the Scene view's handles. It takes a second draw call.</summary>
        Fade
    }
}
