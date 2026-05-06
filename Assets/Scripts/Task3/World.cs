using Leopotam.Ecs;
using UnityEngine;

namespace Task3 {
    public class World : MonoBehaviour {

        public EcsWorld world = null;
        public EcsSystems systems = null;

        private void Awake() {
            world = new EcsWorld();
            systems = new EcsSystems(world);
        }

        private void Start() {
            systems
                .Add(new RigidBodyCreateSystem())
                .Add(new TransformSyncSystem());

            systems.Init();
        }

        private void FixedUpdate() {
            systems?.Run();
        }

        private void OnDestroy() {
            systems?.Destroy();
            world?.Destroy();
        }

        public EcsWorld GetWorld() {
            return world;
        }
    }
}
