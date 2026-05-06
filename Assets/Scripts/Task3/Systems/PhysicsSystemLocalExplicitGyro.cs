using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class PhysicsSystemLocalExplicitGyro : IEcsRunSystem {
        private EcsFilter<RigidBody, Transform, AngularMomentumDisplay> rigidBodies = null;

        public void Run() {
            var dt = Time.fixedDeltaTime;

            foreach (var idx in rigidBodies) {
                ref var rb = ref rigidBodies.Get1(idx);
                ref var tr = ref rigidBodies.Get2(idx);
                ref var display = ref rigidBodies.Get3(idx);

                if (rb.isStatic) continue;

                var linearAcceleration = rb.forceAccumulator * rb.InverseMass;
                rb.linearVelocity += linearAcceleration * dt;
                tr.position += rb.linearVelocity * dt;

                var torqueLocal = Quaternion.Inverse(tr.rotation) * rb.torqueAccumulator;
                var angularVelocityLocal = Quaternion.Inverse(tr.rotation) * rb.angularVelocity;

                var inertiaTensor = new Vector3(
                    1f / rb.InverseInertiaTensor.x,
                    1f / rb.InverseInertiaTensor.y,
                    1f / rb.InverseInertiaTensor.z
                );

                var angularMomentumLocal = Vector3.Scale(angularVelocityLocal, inertiaTensor);
                var gyroscopicTorque = Vector3.Cross(angularVelocityLocal, angularMomentumLocal);

                var angularAccelerationLocal = Vector3.Scale(
                    torqueLocal - gyroscopicTorque,
                    rb.InverseInertiaTensor
                );

                angularVelocityLocal += angularAccelerationLocal * dt;
                rb.angularVelocity = tr.rotation * angularVelocityLocal;

                var angularVelocityQuat = new Quaternion(
                    rb.angularVelocity.x,
                    rb.angularVelocity.y,
                    rb.angularVelocity.z,
                    0f
                );
                var qDot = ScaleQuaternion(angularVelocityQuat * tr.rotation, 0.5f);
                tr.rotation = new Quaternion(
                    tr.rotation.x + qDot.x * dt,
                    tr.rotation.y + qDot.y * dt,
                    tr.rotation.z + qDot.z * dt,
                    tr.rotation.w + qDot.w * dt
                ).normalized;

                var angularMomentumLocalUpdated = Vector3.Scale(angularVelocityLocal, inertiaTensor);
                display.currentAngularMomentum = tr.rotation * angularMomentumLocalUpdated;
                display.currentEnergy = CalculateEnergy(ref rb);

                rb.forceAccumulator = Vector3.zero;
                rb.torqueAccumulator = Vector3.zero;
            }
        }

        private Quaternion ScaleQuaternion(Quaternion q, float scale) {
            return new(q.x * scale, q.y * scale, q.z * scale, q.w * scale);
        }

        private float CalculateEnergy(ref RigidBody rb) {
            var linearEnergy = 0.5f * rb.mass * rb.linearVelocity.sqrMagnitude;

            var inertiaTensor = new Vector3(
                1f / rb.InverseInertiaTensor.x,
                1f / rb.InverseInertiaTensor.y,
                1f / rb.InverseInertiaTensor.z
            );

            var angularEnergy = 0.5f * (
                inertiaTensor.x * rb.angularVelocity.x * rb.angularVelocity.x +
                inertiaTensor.y * rb.angularVelocity.y * rb.angularVelocity.y +
                inertiaTensor.z * rb.angularVelocity.z * rb.angularVelocity.z
            );

            return linearEnergy + angularEnergy;
        }
    }
}
