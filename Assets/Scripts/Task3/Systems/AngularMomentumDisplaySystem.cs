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

                    var momentumDrift = display.currentAngularMomentum - display.initialAngularMomentum;
                    var momentumDriftMagnitude = momentumDrift.magnitude;
                    var momentumDriftPercent = display.initialAngularMomentum.magnitude > 0.001f
                        ? (momentumDriftMagnitude / display.initialAngularMomentum.magnitude) * 100f
                        : 0f;

                    var energyDrift = display.currentEnergy - display.initialEnergy;
                    var energyDriftPercent = display.initialEnergy > 0.001f
                        ? (energyDrift / display.initialEnergy) * 100f
                        : 0f;
                }
            }
        }
    }
}
