using UnityEngine;

namespace Task3 {
    internal static class RigidBodyConstraintUtils {
        public static Vector3 GetWorldOffset(Quaternion rotation, Vector3 localAnchor) {
            return rotation * localAnchor;
        }

        public static Vector3 GetWorldPoint(Transform tr, Vector3 localAnchor) {
            return tr.position + GetWorldOffset(tr.rotation, localAnchor);
        }

        public static Vector3 GetWorldInverseInertiaTensor(RigidBody rb, Quaternion rotation, Vector3 worldVector) {
            var localVector = Quaternion.Inverse(rotation) * worldVector;
            var localResponse = Vector3.Scale(localVector, rb.InverseInertiaTensor);
            return rotation * localResponse;
        }

        public static Vector3 GetPointVelocity(RigidBody rb, Vector3 worldOffset) {
            return rb.linearVelocity + Vector3.Cross(rb.angularVelocity, worldOffset);
        }

        public static void ApplyImpulse(ref RigidBody rb, Quaternion rotation, Vector3 worldOffset, Vector3 impulse) {
            if (rb.isStatic) return;

            rb.linearVelocity += impulse * rb.InverseMass;
            rb.angularVelocity += GetWorldInverseInertiaTensor(rb, rotation, Vector3.Cross(worldOffset, impulse));
        }

        public static void ApplyPositionCorrection(ref Transform tr, ref RigidBody rb, Vector3 worldOffset, Vector3 correction) {
            if (rb.isStatic)  return;

            tr.position += correction * rb.InverseMass;
            var angularDelta = GetWorldInverseInertiaTensor(rb, tr.rotation, Vector3.Cross(worldOffset, correction));
            ApplyAngularDelta(ref tr.rotation, angularDelta);
        }

        public static void ApplyAngularDelta(ref Quaternion rotation, Vector3 angularDelta) {
            var delta = new Quaternion(angularDelta.x, angularDelta.y, angularDelta.z, 0f);
            var qDot = delta * rotation;
            rotation = new Quaternion(
                rotation.x + 0.5f * qDot.x,
                rotation.y + 0.5f * qDot.y,
                rotation.z + 0.5f * qDot.z,
                rotation.w + 0.5f * qDot.w
            ).normalized;
        }

        public static Vector3 EstimateAngularVelocity(Quaternion prevRotation, Quaternion currentRotation, float dt) {
            if (dt <= Mathf.Epsilon) return Vector3.zero;

            var delta = currentRotation * Quaternion.Inverse(prevRotation);
            if (delta.w < 0f) delta = new Quaternion(-delta.x, -delta.y, -delta.z, -delta.w);

            delta.ToAngleAxis(out var angleDeg, out var axis);
            if (axis.sqrMagnitude <= Mathf.Epsilon || float.IsNaN(axis.x)) return Vector3.zero;
            return axis.normalized * (angleDeg * Mathf.Deg2Rad / dt);
        }
    }
}
