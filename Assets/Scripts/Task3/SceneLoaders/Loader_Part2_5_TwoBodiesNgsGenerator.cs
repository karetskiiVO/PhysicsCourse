using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part2_5_TwoBodiesNgsGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part2_5_TwoBodiesNgsGenerator);
        protected override string Name => "2) Two Bodies NGS";
    }
}
