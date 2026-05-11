using Leopotam.Ecs;

using UnityEngine;

using System.Collections.Generic;

namespace Task3 {
    public class SweepAndPruneCollisionSystem : IEcsRunSystem {
        private EcsFilter<RigidBody, Transform> filter = null;
        private EcsWorld world = null;

        private struct SAPBounds {
            public int entityIndex;
            public float minX;
            public float maxX;
            public float minY;
            public float maxY;
            public float minZ;
            public float maxZ;
        }

        private List<SAPBounds> bodies = new();
        private List<int> activeList = new();

        public void Run() {
            bodies.Clear();
            int count = filter.GetEntitiesCount();

            for (int i = 0; i < count; i++) {
                ref var rb = ref filter.Get1(i);
                ref var tr = ref filter.Get2(i);

                float radius = rb.size.magnitude * 0.5f;

                bodies.Add(new SAPBounds {
                    entityIndex = i,
                    minX = tr.position.x - radius,
                    maxX = tr.position.x + radius,
                    minY = tr.position.y - radius,
                    maxY = tr.position.y + radius,
                    minZ = tr.position.z - radius,
                    maxZ = tr.position.z + radius
                });
            }

            bodies.Sort((a, b) => a.minX.CompareTo(b.minX));

            activeList.Clear();

            for (int i = 0; i < bodies.Count; i++) {
                var current = bodies[i];

                activeList.RemoveAll(activeIdx => bodies[activeIdx].maxX < current.minX);

                foreach (var activeIdx in activeList) {
                    var active = bodies[activeIdx];

                    if (
                        current.minY <= active.maxY && current.maxY >= active.minY &&
                        current.minZ <= active.maxZ && current.maxZ >= active.minZ
                    ) {

                        var idA = current.entityIndex;
                        var idB = active.entityIndex;

                        var finalA = idA;
                        var finalB = idB;

                        if (filter.Get1(idB).isStatic) {
                            finalA = idB;
                            finalB = idA;
                        }

                        ref var rbA = ref filter.Get1(finalA);
                        ref var rbB = ref filter.Get1(finalB);

                        if (rbA.isStatic && rbB.isStatic) continue;

                        ref var trA = ref filter.Get2(finalA);
                        ref var trB = ref filter.Get2(finalB);

                        if (BoxCollision3DUtils.TestBoxBox(trA, rbA, trB, rbB, out var manifold)) {
                            manifold.bodyA = filter.GetEntity(finalA);
                            manifold.bodyB = filter.GetEntity(finalB);
                            var ent = world.NewEntity();
                            ent.Get<ContactInfo>() = manifold;
                        }
                    }
                }

                activeList.Add(i);
            }
        }
    }
}
