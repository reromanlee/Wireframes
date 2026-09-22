using System;
using UnityEngine;

namespace reromanlee.Wireframes.Common
{
    public class Line : ILine
    {
        private static readonly Vector3 DefaultPosition = Vector3.zero;
        private static readonly Color DefaultColor = Color.white;

        internal event Action<Line> OnPositionChange;
        internal event Action<Line> OnColorChange;
        internal event Action<Line> OnBoneChange;

        private Vector3 _positionA;
        private Vector3 _positionB;

        private Color _colorA;
        private Color _colorB;

        private Transform _boneA;
        private Transform _boneB;

        public Line(Transform rootBone)
        {
            _positionA = DefaultPosition;
            _positionB = DefaultPosition;
            _colorA = DefaultColor;
            _colorB = DefaultColor;
            _boneA = rootBone;
            _boneB = rootBone;
        }

        public Line(Transform rootBone, Vector3 positionA, Vector3 positionB)
        {
            _positionA = positionA;
            _positionB = positionB;
            _colorA = DefaultColor;
            _colorB = DefaultColor;
            _boneA = rootBone;
            _boneB = rootBone;
        }

        public Vector3 PositionA
        {
            get => _positionA;
            set
            {
                _positionA = value;
                OnPositionChange?.Invoke(this);
            }
        }

        public Vector3 PositionB
        {
            get => _positionB;
            set
            {
                _positionB = value;
                OnPositionChange?.Invoke(this);
            }
        }

        public Color ColorA
        {
            get => _colorA;
            set
            {
                _colorA = value;
                OnColorChange?.Invoke(this);
            }
        }

        public Color ColorB
        {
            get => _colorB;
            set
            {
                _colorB = value;
                OnColorChange?.Invoke(this);
            }
        }

        public Transform BoneA
        {
            get => _boneA;
            set
            {
                _boneA = value;
                OnBoneChange?.Invoke(this);
            }
        }

        public Transform BoneB
        {
            get => _boneB;
            set
            {
                _boneB = value;
                OnBoneChange?.Invoke(this);
            }
        }
    }
}