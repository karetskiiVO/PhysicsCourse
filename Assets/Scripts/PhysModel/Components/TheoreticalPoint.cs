using System;

using UnityEngine;

public struct TheoreticalPoint {
    public Func<float, (Vector2, Vector2)> solution; // Time -> Position, Velocity
}
