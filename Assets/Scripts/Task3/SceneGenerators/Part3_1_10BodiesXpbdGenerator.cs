using Leopotam.Ecs;
using UnityEngine;

namespace Task3.SceneGenerators {
    public class Part3_1_10BodiesXpbdGenerator : SceneGeneratorBase {
        protected override void GenerateScene() {
            var bodySize = new Vector3(1f, 1f, 1f);

            var floor = CreateRigidBody(
                position: new Vector3(0f, -5f, 0f),
                rotation: Quaternion.identity,
                size: new Vector3(20f, 1f, 20f),
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: Vector3.zero,
                isStatic: true
            );
            floor.Get<Floor>();
            
            for (int i = 0; i < 10; i++) {
                CreateRigidBody(
                    position: new Vector3(Random.Range(-2f, 2f), i * 1.5f, Random.Range(-2f, 2f)),
                    rotation: Random.rotation,
                    size: bodySize,
                    mass: 1f,
                    angularVelocity: Vector3.zero,
                    linearVelocity: Vector3.zero
                );
            }

            ecsSystems
                .Add(new GravitySystem())
                .Add(new PhysicsSystemLocalImplicitGyro())
                .Add(new BoxCollisionSystem())
                .Add(new CollisionResponseXpbdSystem());
        }
    }
}
