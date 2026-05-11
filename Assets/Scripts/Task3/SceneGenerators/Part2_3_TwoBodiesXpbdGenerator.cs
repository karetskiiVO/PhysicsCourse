using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Part2_3_TwoBodiesXpbdGenerator : SceneGeneratorBase {
        protected override void GenerateScene() {
            var bodySize = new Vector3(1f, 1f, 1f);

            var anchor = CreateRigidBody(
                position: new Vector3(0f, 2.5f, 0f),
                rotation: Quaternion.identity,
                size: new Vector3(0.25f, 0.25f, 0.25f),
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: Vector3.zero,
                isStatic: true,
                addAngularMomentumDisplay: false
            );

            var bodyA = CreateRigidBody(
                position: new Vector3(-0.9f, 0.8f, 0f),
                rotation: Quaternion.identity,
                size: bodySize,
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: new Vector3(0.2f, 0f, 0f)
            );

            var bodyB = CreateRigidBody(
                position: new Vector3(0.9f, -0.2f, 0f),
                rotation: Quaternion.identity,
                size: bodySize,
                mass: 1f,
                angularVelocity: Vector3.zero,
                linearVelocity: new Vector3(0.05f, 0f, 0f)
            );

            var topOfA = new Vector3(0.4f, 0.5f, 0.2f);
            var bottomOfA = new Vector3(0.4f, -0.5f, 0.2f);
            var topOfB = new Vector3(0.5f, 0.5f, 0.2f);

            ref var trAnchor = ref anchor.Get<Transform>();
            ref var trA = ref bodyA.Get<Transform>();
            ref var trB = ref bodyB.Get<Transform>();

            var jointEntity1 = ecsWorld.NewEntity();
            ref var joint1 = ref jointEntity1.Get<DistanceJoint>();
            var rest1 = Vector3.Distance(trAnchor.position + trAnchor.rotation * Vector3.zero, trA.position + trA.rotation * topOfA);
            joint1 = new() {
                bodyA = anchor,
                bodyB = bodyA,
                localAnchorA = Vector3.zero,
                localAnchorB = topOfA,
                restLength = rest1,
                stiffness = 0f,
                damping = 0f,
                compliance = 1e-6f,
                baumgarte = 0f,
                lambda = 0f
            };

            var vis1 = new GameObject("SpringVisualiser").AddComponent<Task3.Visualisers.SpringVisualiser>();
            vis1.SetEntity(jointEntity1).SetColor(Color.yellow);

            var jointEntity2 = ecsWorld.NewEntity();
            ref var joint2 = ref jointEntity2.Get<DistanceJoint>();
            var rest2 = Vector3.Distance(trA.position + trA.rotation * bottomOfA, trB.position + trB.rotation * topOfB);
            joint2 = new() {
                bodyA = bodyA,
                bodyB = bodyB,
                localAnchorA = bottomOfA,
                localAnchorB = topOfB,
                restLength = rest2,
                stiffness = 0f,
                damping = 0f,
                compliance = 1e-6f,
                baumgarte = 0f,
                lambda = 0f
            };

            var vis2 = new GameObject("SpringVisualiser").AddComponent<Task3.Visualisers.SpringVisualiser>();
            vis2.SetEntity(jointEntity2).SetColor(Color.magenta);

            ecsSystems
                .Add(new GravitySystem())
                .Add(new PhysicsSystemSequentialImpulses())
                .Add(new DistanceJointXpbdSystem());
        }
    }
}
