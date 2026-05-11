using System;

using Leopotam.Ecs;
using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part3_1_10BodiesXpbdGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part3_1_10BodiesXpbdGenerator);
        protected override string Name => "3) 10 Bodies XPBD";
    }
}
