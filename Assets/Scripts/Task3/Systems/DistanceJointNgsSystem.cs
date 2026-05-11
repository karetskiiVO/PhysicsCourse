using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class DistanceJointNgsSystem : IEcsRunSystem {
        private EcsFilter<DistanceJoint> joints = null;
        private const int ITERATIONS = 8;

        public void Run() {
            var dt = Time.fixedDeltaTime;

            foreach (var idx in joints) {
                ref var joint = ref joints.Get1(idx);

                ref var bodyA = ref joint.bodyA.Get<RigidBody>();
                ref var trA = ref joint.bodyA.Get<Transform>();
                ref var bodyB = ref joint.bodyB.Get<RigidBody>();
                ref var trB = ref joint.bodyB.Get<Transform>();

                for (var iter = 0; iter < ITERATIONS; iter++) {
                    var worldOffsetA = RigidBodyConstraintUtils.GetWorldOffset(trA.rotation, joint.localAnchorA);
                    var worldOffsetB = RigidBodyConstraintUtils.GetWorldOffset(trB.rotation, joint.localAnchorB);

                    var pointA = trA.position + worldOffsetA;
                    var pointB = trB.position + worldOffsetB;
                    var delta = pointB - pointA;
                    var distance = delta.magnitude;
                    if (distance <= Mathf.Epsilon) break;

                    var direction = delta / distance;
                    var stretch = distance - joint.restLength;
                    if (Mathf.Abs(stretch) <= 1e-4f) break;

                    var invMass = ComputeEffectiveMass(bodyA, trA.rotation, worldOffsetA, direction)
                        + ComputeEffectiveMass(bodyB, trB.rotation, worldOffsetB, direction);
                    if (invMass <= Mathf.Epsilon) break;

                    var correctionMagnitude = -stretch / invMass;
                    var correction = correctionMagnitude * direction;

                    RigidBodyConstraintUtils.ApplyPositionCorrection(ref trA, ref bodyA, worldOffsetA, -correction);
                    RigidBodyConstraintUtils.ApplyPositionCorrection(ref trB, ref bodyB, worldOffsetB, correction);
                }

                if (!bodyA.isStatic) {
                    bodyA.linearVelocity = (trA.position - bodyA.prevPosition) / Mathf.Max(Mathf.Epsilon, dt);
                    bodyA.angularVelocity = RigidBodyConstraintUtils.EstimateAngularVelocity(bodyA.prevRotation, trA.rotation, dt);
                }

                if (!bodyB.isStatic) {
                    bodyB.linearVelocity = (trB.position - bodyB.prevPosition) / Mathf.Max(Mathf.Epsilon, dt);
                    bodyB.angularVelocity = RigidBodyConstraintUtils.EstimateAngularVelocity(bodyB.prevRotation, trB.rotation, dt);
                }
            }
        }

        private float ComputeEffectiveMass(RigidBody body, Quaternion rotation, Vector3 worldOffset, Vector3 direction) {
            if (body.isStatic) return 0f;

            var rCrossN = Vector3.Cross(worldOffset, direction);
            var angularResponse = RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(body, rotation, rCrossN);
            return body.InverseMass + Vector3.Dot(rCrossN, angularResponse);
        }
    }
}
