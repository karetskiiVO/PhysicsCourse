using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class DistanceJointSpringForceSystem : IEcsRunSystem {
        private EcsFilter<DistanceJoint> joints = null;

        public void Run() {
            foreach (var idx in joints) {
                ref var joint = ref joints.Get1(idx);

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

                var forceMagnitude = joint.stiffness * stretch + joint.damping * relativeVelocity;
                var force = forceMagnitude * direction;

                if (!bodyA.isStatic) bodyA.ApplyForce(force, worldOffsetA);
                if (!bodyB.isStatic) bodyB.ApplyForce(-force, worldOffsetB);
            }
        }
    }
}
