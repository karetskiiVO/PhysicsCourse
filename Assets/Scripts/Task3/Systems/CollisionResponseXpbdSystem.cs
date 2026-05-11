using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class CollisionResponseXpbdSystem : IEcsRunSystem {
        private EcsFilter<ContactInfo> filter = null;
        private EcsFilter<RigidBody, Transform> bodyFilter = null;

        public void Run() {
            var count = filter.GetEntitiesCount();
            var subSteps = 10;

            var localA = new Vector3[count];
            var localB = new Vector3[count];

            for (int i = 0; i < count; i++) {
                ref var manifold = ref filter.Get1(i);
                var trA_initial = manifold.bodyA.Get<Transform>();
                var trB_initial = manifold.bodyB.Get<Transform>();
                localA[i] = Quaternion.Inverse(trA_initial.rotation) * (manifold.point - trA_initial.position);
                localB[i] = Quaternion.Inverse(trB_initial.rotation) * (manifold.point - trB_initial.position);
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

                    var wA = rbA.InverseMass + (rbA.isStatic ? 0f : Vector3.Dot(Vector3.Cross(rA, manifold.normal), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbA, trA.rotation, Vector3.Cross(rA, manifold.normal))));
                    var wB = rbB.InverseMass + (rbB.isStatic ? 0f : Vector3.Dot(Vector3.Cross(rB, manifold.normal), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbB, trB.rotation, Vector3.Cross(rB, manifold.normal))));
                    var totalInverseMass = wA + wB;

                    if (totalInverseMass <= 0.0001f) continue;

                    var pA = trA.position + rA;
                    var pB = trB.position + rB;
                    var currentDist = Vector3.Dot(pB - pA, manifold.normal);
                    var c = manifold.penetration - currentDist;

                    if (c > 0) {
                        var lambda = c / totalInverseMass;
                        var delta = manifold.normal * lambda;

                        RigidBodyConstraintUtils.ApplyPositionCorrection(ref trA, ref rbA, rA, -delta);
                        RigidBodyConstraintUtils.ApplyPositionCorrection(ref trB, ref rbB, rB, delta);

                        var combinedStatic = Mathf.Sqrt(rbA.staticFriction * rbB.staticFriction);
                        var combinedDynamic = Mathf.Sqrt(rbA.dynamicFriction * rbB.dynamicFriction);

                        var dpA = rbA.isStatic ? Vector3.zero : ((trA.position + rA) - (rbA.prevPosition + rbA.prevRotation * localA[i]));
                        var dpB = rbB.isStatic ? Vector3.zero : ((trB.position + rB) - (rbB.prevPosition + rbB.prevRotation * localB[i]));

                        var deltaP = dpB - dpA;
                        var deltaPt = deltaP - Vector3.Dot(deltaP, manifold.normal) * manifold.normal;

                        var dist = deltaPt.magnitude;
                        if (dist > 0.0001f) {
                            var tangentialAdjustment = dist < combinedStatic * lambda ? deltaPt : deltaPt.normalized * (combinedDynamic * lambda);

                            if (!rbA.isStatic) {
                                var wATangential = rbA.InverseMass + (rbA.isStatic ? 0f : Vector3.Dot(Vector3.Cross(rA, tangentialAdjustment.normalized), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbA, trA.rotation, Vector3.Cross(rA, tangentialAdjustment.normalized))));
                                RigidBodyConstraintUtils.ApplyPositionCorrection(ref trA, ref rbA, rA, tangentialAdjustment * wATangential / totalInverseMass);
                            }
                            if (!rbB.isStatic) {
                                var wBTangential = rbB.InverseMass + (rbB.isStatic ? 0f : Vector3.Dot(Vector3.Cross(rB, tangentialAdjustment.normalized), RigidBodyConstraintUtils.GetWorldInverseInertiaTensor(rbB, trB.rotation, Vector3.Cross(rB, tangentialAdjustment.normalized))));
                                RigidBodyConstraintUtils.ApplyPositionCorrection(ref trB, ref rbB, rB, -tangentialAdjustment * wBTangential / totalInverseMass);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < count; i++) filter.GetEntity(i).Destroy();

            var dt = Time.fixedDeltaTime;
            foreach (var idx in bodyFilter) {
                ref var rb = ref bodyFilter.Get1(idx);
                ref var tr = ref bodyFilter.Get2(idx);

                if (!rb.isStatic) {
                    var v = (tr.position - rb.prevPosition) / dt;

                    var deltaQ = tr.rotation * Quaternion.Inverse(rb.prevRotation);
                    var w = new Vector3(deltaQ.x, deltaQ.y, deltaQ.z) * (2.0f / dt);
                    if (deltaQ.w < 0.0f) w = -w;

                    rb.linearVelocity = v;
                    rb.angularVelocity = w;
                }
            }
        }
    }
}
