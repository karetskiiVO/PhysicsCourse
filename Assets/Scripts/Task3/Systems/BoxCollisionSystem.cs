using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class BoxCollisionSystem : IEcsRunSystem {
        private EcsFilter<RigidBody, Transform> filter = null;
        private EcsWorld world = null;

        public void Run() {
            var count = filter.GetEntitiesCount();
            for (int i = 0; i < count; i++) {
                for (int j = i + 1; j < count; j++) {
                    ref var rbA = ref filter.Get1(i);
                    ref var trA = ref filter.Get2(i);
                    ref var rbB = ref filter.Get1(j);
                    ref var trB = ref filter.Get2(j);

                    if (rbA.isStatic && rbB.isStatic) continue;

                    if (BoxCollision3DUtils.TestBoxBox(trA, rbA, trB, rbB, out var manifold)) {
                        manifold.bodyA = filter.GetEntity(i);
                        manifold.bodyB = filter.GetEntity(j);
                        var ent = world.NewEntity();
                        ent.Get<ContactInfo>() = manifold;
                    }
                }
            }
        }
    }
}
