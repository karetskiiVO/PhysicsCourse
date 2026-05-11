using System;

using Leopotam.Ecs;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part2_2_OffCenterSpringSoftConstraintGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part2_2_OffCenterSpringSoftConstraintGenerator);
        protected override string Name => "2) Spring Soft Constraint";
    }
}
