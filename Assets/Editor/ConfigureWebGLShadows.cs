using UnityEditor;

using UnityEngine;

namespace Task3.Editor {
    public static class ConfigureWebGLShadows {
        [MenuItem("Tools/Configure WebGL Shadows")]
        public static void Configure() {
            // Quality settings (applies to current active quality level)
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 50f;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowCascades = 2;

            // Enable shadows on all lights in open scenes
            var lights = Object.FindObjectsOfType<Light>();
            foreach (var l in lights) {
                l.shadows = LightShadows.Soft;
                EditorUtility.SetDirty(l);
            }

            // Enable shadow casting/receiving for all MeshRenderers
            var rends = Object.FindObjectsOfType<MeshRenderer>();
            foreach (var r in rends) {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
                EditorUtility.SetDirty(r);
            }

            // Check render pipeline asset
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (rp != null) {
                EditorUtility.DisplayDialog("Configure WebGL Shadows",
                    "Проект использует SRP: " + rp.GetType().Name + ". Проверьте URP Asset: включите Main/Additional Light Shadows и настройте Max Shadow Distance.",
                    "OK");
            } else {
                EditorUtility.DisplayDialog("Configure WebGL Shadows",
                    "Настройки применены: Quality, Light и MeshRenderer обновлены. Если тени не видны — проверьте материалы и Light в сцене.",
                    "OK");
            }

            AssetDatabase.SaveAssets();
            SceneView.RepaintAll();
        }
    }
}
