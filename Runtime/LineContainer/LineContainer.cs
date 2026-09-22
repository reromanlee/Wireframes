using reromanlee.Wireframes.Common;
using UnityEngine;

namespace reromanlee.Wireframes
{
    public class LineContainer : ILineContainer
    {
        private readonly IMeshProxy _meshProxy;

        public LineContainer()
        {
            _meshProxy = new MeshProxy();
        }

        public ILine CreateLine()
        {
            Line line = new(_meshProxy.RootBone);
            _meshProxy.AddLine(line);
            return line;
        }

        public ILine CreateLine(Vector3 positionA, Vector3 positionB)
        {
            Line line = new(_meshProxy.RootBone, positionA, positionB);
            _meshProxy.AddLine(line);
            return line;
        }

        public IBox CreateBox()
        {
            Box box = new(_meshProxy.RootBone);
            _meshProxy.AddBox(box);
            return box;
        }

        public IBox CreateBox(Vector3 cornerA, Vector3 cornerB)
        {
            Box box = new(_meshProxy.RootBone, cornerA, cornerB);
            _meshProxy.AddBox(box);
            return box;
        }
    }
}