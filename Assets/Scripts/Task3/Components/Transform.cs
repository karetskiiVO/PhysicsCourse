using UnityEngine;

namespace Task3 {
    public struct Transform {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public struct TransformRef {
        public UnityEngine.Transform transform;
    }
}
