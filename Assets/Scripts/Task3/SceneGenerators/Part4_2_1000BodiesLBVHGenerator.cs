using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Part4_2_1000BodiesLBVHGenerator : SceneGeneratorBase {
        protected override void GenerateScene() {
            var floor = CreateRigidBody(
                position: new Vector3(0f, -10f, 0f),
                rotation: Quaternion.Euler(15f, 0f, 0f),
                size: new Vector3(100f, 1f, 100f),
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: Vector3.zero,
                isStatic: true
            );

            ref var floorRb = ref floor.Get<RigidBody>();
            floorRb.staticFriction = 0.6f;
            floorRb.dynamicFriction = 0.4f;

            floor.Get<Floor>();

            for (int i = 0; i < 1000; i++) {
                var x = i % 10;
                var y = (i / 10) % 10;
                var z = i / 100;

                var sizeRand = Random.Range(0.2f, 1.5f);
                var bodySize = new Vector3(sizeRand, sizeRand, sizeRand);

                var body = CreateRigidBody(
                    position: new Vector3(x * 2.5f - 12.5f, y * 2.5f + 5f, z * 2.5f - 12.5f),
                    rotation: Random.rotation,
                    size: bodySize,
                    mass: sizeRand * sizeRand * sizeRand,
                    angularVelocity: Vector3.zero,
                    linearVelocity: Vector3.zero,
                    addAngularMomentumDisplay: false
                );

                ref var bodyRb = ref body.Get<RigidBody>();
                bodyRb.staticFriction = 0.5f;
                bodyRb.dynamicFriction = 0.3f;
            }

            ecsSystems
                .Add(new GravitySystem())
                .Add(new PhysicsSystemSequentialImpulses())
                .Add(new LBVHCollisionSystem())
                .Add(new CollisionResponseXpbdSystem());
        }
    }
}
