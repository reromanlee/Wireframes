using reromanlee.Wireframes.Common;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal interface IMeshProxy
    {
        Transform RootBone { get; }

        void AddLine(Line line);
        void RemoveLine(Line line);

        void AddBox(Box box);
        void RemoveBox(Box box);
    }
}