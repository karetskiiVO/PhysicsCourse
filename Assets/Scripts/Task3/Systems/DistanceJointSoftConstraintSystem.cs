using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class DistanceJointSoftConstraintSystem : IEcsRunSystem {
        private EcsFilter<DistanceJoint> joints = null;

        public void Run() {
            var dt = Time.fixedDeltaTime;

            foreach (var idx in joints) {
                ref var joint = ref joints.Get1(idx);
                joint.lambda = 0f;

                ref var bodyA = ref joint.bodyA.Get<RigidBody>();
                ref var trA = ref joint.bodyA.Get<Transform>();
                ref var bodyB = ref joint.bodyB.Get<RigidBody>();
                ref var trB = ref joint.bodyB.Get<Transform>();

                var worldOffsetA = RigidBodyConstraintUtils.GetWorldOffset(trA.rotation, joint.localAnchorA);
                var worldOffsetB = RigidBodyConstraintUtils.GetWorldOffset(trB.rotation, joint.localAnchorB);

                var pointA = trA.position + worldOffsetA;
                var pointB = trB.position + worldOffsetB;
                var delta = pointB - pointA;
                var distance = delta.magnitude;
                if (distance <= Mathf.Epsilon) continue;

                var direction = delta / distance;
                var stretch = distance - joint.restLength;

                var relativeVelocity = Vector3.Dot(
                    RigidBodyConstraintUtils.GetPointVelocity(bodyB, worldOffsetB) -
                    RigidBodyConstraintUtils.GetPointVelocity(bodyA, worldOffsetA),
                    direction
                );

                var invMass = ComputeEffectiveMass(bodyA, trA.rotation, worldOffsetA, direction)
                    + ComputeEffectiveMass(bodyB, trB.rotation, worldOffsetB, direction);
                if (invMass <= Mathf.Epsilon) continue;

                var gamma = 1f / Mathf.Max(Mathf.Epsilon, dt * (joint.damping + dt * joint.stiffness));
                var bias = stretch * dt * joint.stiffness * gamma;
                var impulseMagnitude = -(relativeVelocity + bias + gamma * joint.lambda) / (invMass + gamma);
                joint.lambda += impulseMagnitude;

                var impulse = impulseMagnitude * direction;
                RigidBodyConstraintUtils.ApplyImpulse(ref bodyA, trA.rotation, worldOffsetA, -impulse);
                RigidBodyConstraintUtils.ApplyImpulse(ref bodyB, trB.rotation, worldOffsetB, impulse);
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
