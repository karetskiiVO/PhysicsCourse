using System;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part1_3_LocalExplicitGyroGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part1_3_LocalExplicitGyroGenerator);
        protected override string Name => "3) Local Explicit Gyro";
    }
}
