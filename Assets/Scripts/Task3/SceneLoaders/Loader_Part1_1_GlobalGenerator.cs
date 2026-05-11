using System;

using UnityEngine;

namespace Task3.SceneGenerators {
    public class Loader_Part1_1_GlobalGenerator : Loader {
        protected override Type sceneGenerator => typeof(Part1_1_GlobalGenerator);
        protected override string Name => "1) Global";
    }
}
