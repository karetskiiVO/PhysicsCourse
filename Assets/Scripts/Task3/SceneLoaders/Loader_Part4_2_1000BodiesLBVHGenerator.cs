using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part4_2_1000BodiesLBVHGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part4_2_1000BodiesLBVHGenerator);
        protected override string Name => "4) 1000 Bodies LBVH";
    }
}
