using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class PhysicsSystemLocalNoGyro : IEcsRunSystem {
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
                var angularAccelerationLocal = Vector3.Scale(torqueLocal, rb.InverseInertiaTensor);

                angularVelocityLocal += angularAccelerationLocal * dt;
                rb.angularVelocity = tr.rotation * angularVelocityLocal;

                var angVelMag = rb.angularVelocity.magnitude;
                if (angVelMag > 0.001f) {
                    var angle = angVelMag * dt;
                    var halfAngle = angle * 0.5f;
                    var axis = rb.angularVelocity / angVelMag;
                    var sinHalf = Mathf.Sin(halfAngle);
                    var deltaRotation = new Quaternion(
                        axis.x * sinHalf,
                        axis.y * sinHalf,
                        axis.z * sinHalf,
                        Mathf.Cos(halfAngle)
                    );
                    tr.rotation = (deltaRotation * tr.rotation).normalized;
                }


                var angularMomentumLocal = Vector3.Scale(
                    angularVelocityLocal,
                    new Vector3(
                        1f / rb.InverseInertiaTensor.x,
                        1f / rb.InverseInertiaTensor.y,
                        1f / rb.InverseInertiaTensor.z
                    )
                );
                display.currentAngularMomentum = tr.rotation * angularMomentumLocal;
                display.currentEnergy = CalculateEnergy(ref rb);

                rb.forceAccumulator = Vector3.zero;
                rb.torqueAccumulator = Vector3.zero;
            }
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
