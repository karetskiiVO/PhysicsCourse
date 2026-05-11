using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part4_1_1000BodiesSweepAndPruneGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part4_1_1000BodiesSweepAndPruneGenerator);
        protected override string Name => "4) 1000 Bodies Sweep and Prune";
    }
}
