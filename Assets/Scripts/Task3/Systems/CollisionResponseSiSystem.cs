using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class CollisionResponseSiSystem : IEcsRunSystem {
        private EcsFilter<ContactInfo> filter = null;

        public void Run() {
            var count = filter.GetEntitiesCount();
            int subSteps = 10;
            float baumgarte = 0.2f;
            float dt = Time.fixedDeltaTime;

            var localA = new Vector3[count];
            var localB = new Vector3[count];
            var accumulatedImpulses = new float[count];
            var accumulatedFriction = new Vector3[count];

            for (int i = 0; i < count; i++) {
                ref var manifold = ref filter.Get1(i);
                var trA = manifold.bodyA.Get<Transform>();
                var trB = manifold.bodyB.Get<Transform>();
                localA[i] = Quaternion.Inverse(trA.rotation) * (manifold.point - trA.position);
                localB[i] = Quaternion.Inverse(trB.rotation) * (manifold.point - trB.position);
            }

            for (int s = 0; s < subSteps; s++) {
                for (int i = 0; i < count; i++) {
                    ref var manifold = ref filter.Get1(i);
                    ref var rbA = ref manifold.bodyA.Get<RigidBody>();
                    ref var trA = ref manifold.bodyA.Get<Transform>();
                    ref var rbB = ref manifold.bodyB.Get<RigidBody>();
                    ref var trB = ref manifold.bodyB.Get<Transform>();

                    var rA = trA.rotation * localA[i];
                    var rB = trB.rotation * localB[i];
                    var normal = manifold.normal;

                    var vA = RigidBodyConstraintUtils.GetPointVelocity(rbA, rA);
                    var vB = RigidBodyConstraintUtils.GetPointVelocity(rbB, rB);
                    var relativeVelocity = vB - vA;

                    var velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

                    var wA = rbA.InverseMass + Vector3.Dot(Vector3.Cross(rA, normal), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbA, trA.rotation, Vector3.Cross(rA, normal)));
                    var wB = rbB.InverseMass + Vector3.Dot(Vector3.Cross(rB, normal), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbB, trB.rotation, Vector3.Cross(rB, normal)));

                    var wSum = wA + wB;
                    if (wSum <= 0.0001f) continue;

                    var beta = baumgarte * manifold.penetration / dt;
                    var restitution = 0.3f;
                    var bScale = velocityAlongNormal < -1f ? restitution : 0.0f;
                    beta += bScale * -velocityAlongNormal;

                    var j = -(velocityAlongNormal - beta) / wSum;

                    var oldImpulse = accumulatedImpulses[i];
                    var newImpulse = Mathf.Max(oldImpulse + j, 0f);
                    accumulatedImpulses[i] = newImpulse;

                    var deltaJ = newImpulse - oldImpulse;

                    var impulse = normal * deltaJ;

                    RigidBodyConstraintUtils.ApplyImpulse(ref rbA, trA.rotation, rA, -impulse);
                    RigidBodyConstraintUtils.ApplyImpulse(ref rbB, trB.rotation, rB, impulse);

                    var vA2 = RigidBodyConstraintUtils.GetPointVelocity(rbA, rA);
                    var vB2 = RigidBodyConstraintUtils.GetPointVelocity(rbB, rB);
                    var relVel2 = vB2 - vA2;

                    var tangent = relVel2 - Vector3.Dot(relVel2, normal) * normal;
                    if (tangent.sqrMagnitude > Mathf.Epsilon) {
                        tangent.Normalize();

                        var wA_t = rbA.InverseMass + Vector3.Dot(Vector3.Cross(rA, tangent), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbA, trA.rotation, Vector3.Cross(rA, tangent)));
                        var wB_t = rbB.InverseMass + Vector3.Dot(Vector3.Cross(rB, tangent), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbB, trB.rotation, Vector3.Cross(rB, tangent)));

                        var jt = -Vector3.Dot(relVel2, tangent) / (wA_t + wB_t);

                        var mu = Mathf.Sqrt(rbA.dynamicFriction * rbB.dynamicFriction);
                        var maxFriction = mu * accumulatedImpulses[i];

                        var oldFriction = accumulatedFriction[i];
                        var newFriction = oldFriction + tangent * jt;
                        if (newFriction.magnitude > maxFriction) {
                            newFriction = newFriction.normalized * maxFriction;
                        }
                        accumulatedFriction[i] = newFriction;

                        var deltaFriction = newFriction - oldFriction;

                        RigidBodyConstraintUtils.ApplyImpulse(ref rbA, trA.rotation, rA, -deltaFriction);
                        RigidBodyConstraintUtils.ApplyImpulse(ref rbB, trB.rotation, rB, deltaFriction);
                    }
                }
            }

            for (int i = 0; i < count; i++) filter.GetEntity(i).Destroy();
        }
    }
}
