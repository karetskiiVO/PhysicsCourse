using System;

using UnityEngine;

namespace Task1 {
    public struct TheoreticalPoint {
        public Func<float, (Vector2, Vector2)> solution; // Time -> Position, Velocity
    }
}
