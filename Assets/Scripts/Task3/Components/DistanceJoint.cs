using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public struct DistanceJoint {
        public EcsEntity bodyA;
        public EcsEntity bodyB;

        public Vector3 localAnchorA;
        public Vector3 localAnchorB;

        public float restLength;

        public float stiffness;
        public float damping;
        public float compliance;
        public float baumgarte;

        public float lambda;
    }
}
