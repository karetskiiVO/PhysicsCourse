using Leopotam.Ecs;
using UnityEngine;

namespace Task3 {
    public class AngularMomentumDisplaySystem : IEcsRunSystem {
        private EcsFilter<RigidBody, AngularMomentumDisplay> rigidBodies = null;
        private float displayTimer = 0f;
        private const float DISPLAY_INTERVAL = 0.5f;

        public void Run() {
            displayTimer += Time.fixedDeltaTime;

            if (displayTimer >= DISPLAY_INTERVAL) {
                displayTimer = 0f;

                foreach (var idx in rigidBodies) {
                    ref var rb = ref rigidBodies.Get1(idx);
                    ref var display = ref rigidBodies.Get2(idx);

                    Vector3 momentumDrift = display.currentAngularMomentum - display.initialAngularMomentum;
                    float momentumDriftMagnitude = momentumDrift.magnitude;
                    float momentumDriftPercent = display.initialAngularMomentum.magnitude > 0.001f
                        ? (momentumDriftMagnitude / display.initialAngularMomentum.magnitude) * 100f
                        : 0f;

                    float energyDrift = display.currentEnergy - display.initialEnergy;
                    float energyDriftPercent = display.initialEnergy > 0.001f
                        ? (energyDrift / display.initialEnergy) * 100f
                        : 0f;
                }
            }
        }
    }
}
