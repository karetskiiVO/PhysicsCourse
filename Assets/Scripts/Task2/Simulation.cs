using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task2 {
    class Simulation : MonoBehaviour {
        EcsWorld world;
        EcsSystems systems;

        public EcsWorld World => world;

        public Action<EcsWorld, EcsSystems> Create;

        void Start() {
            Init();
        }

        void FixedUpdate() {
            systems?.Run();
        }

        public void Init() {
            world = new();
            systems = new(world);

            // systems
            //     .Add(new EnergyBeholderSystem(energyChart));

            Create(world, systems);

            systems?.Init();
        }

        void OnDestroy() {
            if (systems != null) {
                systems.Destroy();
                systems = null;
                world.Destroy();
                world = null;
            }
        }
    }
}
