using System;

using Leopotam.Ecs;
using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part3_2_10BodiesSiGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part3_2_10BodiesSiGenerator);
        protected override string Name => "3) 10 Bodies SI";
    }
}
