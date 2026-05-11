using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part2_4_TwoBodiesBaumgarteGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part2_4_TwoBodiesBaumgarteGenerator);
        protected override string Name => "2) Two Bodies Baumgarte";
    }
}
