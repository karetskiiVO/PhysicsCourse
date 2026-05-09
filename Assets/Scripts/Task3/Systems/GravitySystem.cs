using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class GravitySystem : IEcsRunSystem {
        private EcsFilter<RigidBody> bodies = null;

        public void Run() {
            var g = new Vector3(0f, -9.81f, 0f);

            foreach (var idx in bodies) {
                ref var rb = ref bodies.Get1(idx);
                if (rb.isStatic) continue;

                rb.forceAccumulator += g * rb.mass;
            }
        }
    }
}
