using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part2_6_TwoBodiesSoftConstraintGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part2_6_TwoBodiesSoftConstraintGenerator);
        protected override string Name => "2) Two Bodies Soft Constraint";
    }
}
