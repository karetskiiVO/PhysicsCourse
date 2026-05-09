using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Part2_1_OffCenterSpringForceGenerator : SceneGeneratorBase {
        protected override void GenerateScene() {
            var bodySize = new Vector3(1, 1f, 1);
            var mass = 1f;

            var dynamicBody = CreateRigidBody(
                position: new Vector3(0f, 0f, 0f),
                rotation: Quaternion.identity,
                size: bodySize,
                mass: mass,
                angularVelocity: Vector3.zero,
                linearVelocity: new Vector3(0.35f, 0.15f, 0f)
            );

            var anchorBody = CreateRigidBody(
                position: new Vector3(-3.5f, 1f, 0f),
                rotation: Quaternion.identity,
                size: new Vector3(0.25f, 0.25f, 0.25f),
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: Vector3.zero,
                isStatic: true,
                addAngularMomentumDisplay: false
            );

            var localAnchor = new Vector3(0.5f, 0.3f, 0.1f);

            ref var dynamicTransform = ref dynamicBody.Get<Transform>();
            ref var anchorTransform = ref anchorBody.Get<Transform>();

            var restLength = Vector3.Distance(
                dynamicTransform.position + dynamicTransform.rotation * localAnchor,
                anchorTransform.position
            );

            var jointEntity = ecsWorld.NewEntity();
            ref var joint = ref jointEntity.Get<DistanceJoint>();
            joint = new() {
                bodyA = anchorBody,
                bodyB = dynamicBody,
                localAnchorA = Vector3.zero,
                localAnchorB = localAnchor,
                restLength = restLength,
                stiffness = 5f,
                damping = 7f,
                compliance = 0f,
                baumgarte = 0f,
                lambda = 0f
            };

            var vis = new GameObject("SpringVisualiser").AddComponent<Task3.Visualisers.SpringVisualiser>();
            vis.SetEntity(jointEntity).SetColor(Color.cyan);

            ecsSystems
                .Add(new DistanceJointSpringForceSystem())
                .Add(new GravitySystem())
                .Add(new PhysicsSystemSequentialImpulses());
        }
    }
}
