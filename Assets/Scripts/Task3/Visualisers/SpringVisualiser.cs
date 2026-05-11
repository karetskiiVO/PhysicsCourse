using Leopotam.Ecs;

using UnityEngine;

namespace Task3.Visualisers {
    public class SpringVisualiser : MonoBehaviour {
        private EcsEntity entity;
        private LineRenderer lr;

        [SerializeField] int coils = 12;
        [SerializeField] float coilRadius = 0.12f;
        [SerializeField] int segmentsPerCoil = 6;
        [SerializeField] float lineWidth = 0.05f;

        void Awake() {
            lr = gameObject.GetComponent<LineRenderer>();
            if (lr == null) lr = gameObject.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.positionCount = 0;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.numCapVertices = 4;
        }

        void Update() {
            if (entity.IsNull()) return;

            ref var joint = ref entity.Get<DistanceJoint>();

            ref var trA = ref joint.bodyA.Get<Transform>();
            ref var trB = ref joint.bodyB.Get<Transform>();

            var pA = trA.position + trA.rotation * joint.localAnchorA;
            var pB = trB.position + trB.rotation * joint.localAnchorB;

            var dir = pB - pA;
            var len = dir.magnitude;
            if (len <= Mathf.Epsilon) {
                lr.positionCount = 0;
                return;
            }

            dir /= len;

            // build a stable perpendicular basis
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.9f) up = Vector3.right;
            var ortho = Vector3.Cross(dir, up).normalized;
            var binorm = Vector3.Cross(dir, ortho).normalized;

            int totalSegments = Mathf.Max(4, coils * segmentsPerCoil);
            lr.positionCount = totalSegments + 1;

            for (int i = 0; i <= totalSegments; i++) {
                float t = (float)i / totalSegments;
                float x = t * len;
                float angle = t * coils * Mathf.PI * 2f;
                float offset = Mathf.Sin(angle) * coilRadius;
                // combine ortho and binorm to make circular cross-section
                var offsetVec = ortho * Mathf.Cos(angle) * (coilRadius * 0.6f) + binorm * Mathf.Sin(angle) * (coilRadius * 0.6f);
                var pt = pA + dir * x + offsetVec;
                lr.SetPosition(i, pt);
            }
        }

        public SpringVisualiser SetEntity(EcsEntity entity) {
            this.entity = entity;
            return this;
        }

        public SpringVisualiser SetColor(Color color) {
            if (lr == null) Awake();
            lr.startColor = color;
            lr.endColor = color;
            return this;
        }
    }
}
