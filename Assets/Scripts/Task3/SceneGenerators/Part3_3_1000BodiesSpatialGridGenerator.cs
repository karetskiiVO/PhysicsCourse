using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Part3_3_1000BodiesSpatialGridGenerator : SceneGeneratorBase {
        protected override void GenerateScene() {
            var bodySize = new Vector3(0.5f, 0.5f, 0.5f);

            var floor = CreateRigidBody(
                position: new Vector3(0f, -10f, 0f),
                rotation: Quaternion.identity,
                size: new Vector3(50f, 1f, 50f),
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: Vector3.zero,
                isStatic: true
            );
            floor.Get<Floor>();

            for (int i = 0; i < 1000; i++) {
                int x = i % 10;
                int y = (i / 10) % 10;
                int z = i / 100;
                CreateRigidBody(
                    position: new Vector3(x * 0.8f - 4f, y * 0.8f + 2f, z * 0.8f - 4f),
                    rotation: Random.rotation,
                    size: bodySize,
                    mass: 0.1f,
                    angularVelocity: Vector3.zero,
                    linearVelocity: Vector3.zero,
                    addAngularMomentumDisplay: false
                );
            }

            ecsSystems
                .Add(new GravitySystem())
                .Add(new PhysicsSystemSequentialImpulses())
                .Add(new SpatialGridCollisionSystem())
                .Add(new CollisionResponseSiSystem());
        }
    }
}
