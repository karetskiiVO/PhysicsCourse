using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public abstract class SceneGeneratorBase : MonoBehaviour {
        protected World world;
        protected EcsWorld ecsWorld => world.world;
        protected EcsSystems ecsSystems => world.systems;

        protected virtual void Start() {
            world = gameObject.AddComponent<World>();
            GenerateScene();
        }

        protected abstract void GenerateScene();

        protected EcsEntity CreateRigidBody(
            Vector3 position,
            Quaternion rotation,
            Vector3 size,
            float mass,
            Vector3 angularVelocity,
            Vector3 linearVelocity = default,
            bool isStatic = false,
            bool addAngularMomentumDisplay = true
        ) {
            var entity = ecsWorld.NewEntity();

            ref var tr = ref entity.Get<Transform>();
            tr.position = position;
            tr.rotation = rotation;
            tr.scale = size;

            ref var rb = ref entity.Get<RigidBody>();
            rb = RigidBody.FromMassAndSize(mass, size, isStatic);
            rb.angularVelocity = angularVelocity;
            rb.linearVelocity = linearVelocity;

            if (addAngularMomentumDisplay) {
                ref var display = ref entity.Get<AngularMomentumDisplay>();
            }

            ref var trRef = ref entity.Get<TransformRef>();

            return entity;
        }
    }
}
