using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part3_3_1000BodiesSpatialGridGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part3_3_1000BodiesSpatialGridGenerator);
        protected override string Name => "3) 1000 Bodies Spatial Grid";
    }
}
