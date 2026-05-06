using UnityEngine;

namespace Task3.SceneGenerators {
    public class Part1_4_LocalImplicitGyroGenerator : SceneGeneratorBase {
        protected override void GenerateScene() {
            var bodySize = new Vector3(2f, 1f, 0.5f);
            var mass = 1f;
            var angularVelocity = new Vector3(0.2f, 5f, 0.2f);

            CreateRigidBody(
                position: Vector3.zero,
                rotation: Quaternion.identity,
                //rotation: Quaternion.Euler(10f, 0f, 5f),
                size: bodySize,
                mass: mass,
                angularVelocity: angularVelocity,
                linearVelocity: Vector3.zero
            );

            ecsSystems
                .Add(new PhysicsSystemLocalImplicitGyro())
                .Add(new EnergyChartSystem());
        }
    }
}
