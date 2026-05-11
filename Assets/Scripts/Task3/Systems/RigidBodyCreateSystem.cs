using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class RigidBodyCreateSystem : IEcsInitSystem {
        EcsFilter<RigidBody, Transform, TransformRef> rigidBodies = null;

        public void Init() {
            var material = CreateCompatibleMaterial();

            foreach (var idx in rigidBodies) {
                ref var tr = ref rigidBodies.Get2(idx);
                ref var trRef = ref rigidBodies.Get3(idx);

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (material != null && go.GetComponent<Renderer>() != null) {
                    go.GetComponent<Renderer>().material = material;
                }

                var renderer = go.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.material = material ?? renderer.material;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }

                if (rigidBodies.GetEntity(idx).Has<Floor>()) {
                    go.GetComponent<Renderer>().material.color = Color.gray;
                }

                trRef.transform = go.transform;
                trRef.transform.SetPositionAndRotation(tr.position, tr.rotation);
                trRef.transform.localScale = tr.scale;
            }
        }

        private Material CreateCompatibleMaterial() {
            var hasRenderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            string[] candidates = hasRenderPipeline
                ? new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit", "HDRP/Lit", "Sprites/Default", "Standard" }
                : new[] { "Standard", "Mobile/Bumped Diffuse", "Sprites/Default", "Unlit/Color" };

            foreach (var s in candidates) {
                var shader = Shader.Find(s);
                if (shader != null) return new Material(shader) { color = Color.red };
            }
            return null;
        }
    }
}
