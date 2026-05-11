using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part2_3_TwoBodiesXpbdGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part2_3_TwoBodiesXpbdGenerator);
        protected override string Name => "2) Two Bodies XPBD";
    }
}
