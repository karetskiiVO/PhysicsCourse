using UnityEngine;

using System.Runtime.CompilerServices;

namespace Task3 {
    public static class BoxCollision3DUtils {
        public static bool TestBoxBox(Transform trA, RigidBody rbA, Transform trB, RigidBody rbB, out ContactInfo manifold) {
            manifold = default;

            var extA = rbA.size * 0.5f;
            var extB = rbB.size * 0.5f;

            var axisA0 = trA.rotation * Vector3.right;
            var axisA1 = trA.rotation * Vector3.up;
            var axisA2 = trA.rotation * Vector3.forward;

            var axisB0 = trB.rotation * Vector3.right;
            var axisB1 = trB.rotation * Vector3.up;
            var axisB2 = trB.rotation * Vector3.forward;

            var T = trB.position - trA.position;

            var minPenetration = float.MaxValue;
            var bestAxis = Vector3.zero;

            if (!TestAxis(axisA0, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;
            if (!TestAxis(axisA1, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;
            if (!TestAxis(axisA2, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            if (!TestAxis(axisB0, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;
            if (!TestAxis(axisB1, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;
            if (!TestAxis(axisB2, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A0 x B0
            var cross = Vector3.Cross(axisA0, axisB0);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A0 x B1
            cross = Vector3.Cross(axisA0, axisB1);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A0 x B2
            cross = Vector3.Cross(axisA0, axisB2);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A1 x B0
            cross = Vector3.Cross(axisA1, axisB0);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A1 x B1
            cross = Vector3.Cross(axisA1, axisB1);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A1 x B2
            cross = Vector3.Cross(axisA1, axisB2);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A2 x B0
            cross = Vector3.Cross(axisA2, axisB0);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A2 x B1
            cross = Vector3.Cross(axisA2, axisB1);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            // A2 x B2
            cross = Vector3.Cross(axisA2, axisB2);
            if (cross.sqrMagnitude > 1e-5f && !TestAxis(cross.normalized, extA, extB, axisA0, axisA1, axisA2, axisB0, axisB1, axisB2, T, ref minPenetration, ref bestAxis)) return false;

            if (Vector3.Dot(T, bestAxis) < 0) bestAxis = -bestAxis;

            manifold.normal = bestAxis;
            manifold.penetration = minPenetration;
            manifold.initialDist = Vector3.Dot(T, bestAxis);

            Vector3 supportB = trB.position;
            supportB += axisB0 * (Vector3.Dot(axisB0, -bestAxis) > 0 ? extB.x : -extB.x);
            supportB += axisB1 * (Vector3.Dot(axisB1, -bestAxis) > 0 ? extB.y : -extB.y);
            supportB += axisB2 * (Vector3.Dot(axisB2, -bestAxis) > 0 ? extB.z : -extB.z);

            manifold.point = supportB;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TestAxis(
            Vector3 axis,
            Vector3 extA, Vector3 extB,
            Vector3 axisA0, Vector3 axisA1, Vector3 axisA2,
            Vector3 axisB0, Vector3 axisB1, Vector3 axisB2,
            Vector3 T,
            ref float minPenetration, ref Vector3 bestAxis
        ) {
            var rA =
                extA.x * Mathf.Abs(Vector3.Dot(axisA0, axis)) +
                extA.y * Mathf.Abs(Vector3.Dot(axisA1, axis)) +
                extA.z * Mathf.Abs(Vector3.Dot(axisA2, axis));

            var rB =
                extB.x * Mathf.Abs(Vector3.Dot(axisB0, axis)) +
                extB.y * Mathf.Abs(Vector3.Dot(axisB1, axis)) +
                extB.z * Mathf.Abs(Vector3.Dot(axisB2, axis));

            var d = Vector3.Dot(T, axis);
            var pen = rA + rB - Mathf.Abs(d);

            if (pen <= 0) return false;

            if (pen < minPenetration) {
                minPenetration = pen;
                bestAxis = axis;
            }
            return true;
        }
    }
}
