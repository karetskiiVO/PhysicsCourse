using System.Runtime.CompilerServices;

using UnityEngine;

namespace Task3 {
    public struct RigidBody {
        /* External */
        public float mass;
        public Vector3 size;
        public bool isStatic;
        public float staticFriction;
        public float dynamicFriction;

        /* Internal */
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;
        public Vector3 forceAccumulator;
        public Vector3 torqueAccumulator;
        public Vector3 prevPosition;
        public Quaternion prevRotation;

        public Vector3 initialAngularMomentum;
        public Vector3 currentAngularMomentum;

        private Vector3 inertiaTensor;

        public readonly float InverseMass => isStatic ? 0f : (mass > 0f ? 1f / mass : 0f);
        public readonly Vector3 InverseInertiaTensor => isStatic ? Vector3.zero : new Vector3(
            inertiaTensor.x > 0f ? 1f / inertiaTensor.x : 0f,
            inertiaTensor.y > 0f ? 1f / inertiaTensor.y : 0f,
            inertiaTensor.z > 0f ? 1f / inertiaTensor.z : 0f
        );

        public void ApplyForce(Vector3 force, Vector3 localPos) {
            forceAccumulator += force;
            torqueAccumulator += Vector3.Cross(localPos, force);
        }

        public static RigidBody FromMassAndSize(float mass, Vector3 size, bool isStatic = false, float staticFriction = 0.5f, float dynamicFriction = 0.3f) {
            var rb = new RigidBody {
                mass = mass,
                size = size,
                isStatic = isStatic,
                staticFriction = staticFriction,
                dynamicFriction = dynamicFriction,
                inertiaTensor = new Vector3(
                    (1f / 12f) * mass * (size.y * size.y + size.z * size.z),
                    (1f / 12f) * mass * (size.x * size.x + size.z * size.z),
                    (1f / 12f) * mass * (size.x * size.x + size.y * size.y)
                ),
            };

            return rb;
        }
    }
}
