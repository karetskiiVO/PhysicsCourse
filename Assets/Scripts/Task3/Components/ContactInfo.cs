using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public struct ContactInfo {
        public EcsEntity bodyA;
        public EcsEntity bodyB;
        public Vector3 normal;
        public Vector3 point;
        public float penetration;
        public float initialDist;
    }
}
