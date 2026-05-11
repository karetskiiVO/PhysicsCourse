using System;
using System.Globalization;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Task2 {
    public class Loader : MonoBehaviour {
        [Serializable]
        public struct RunSettings {
            public ProjectionDynamicsCube.DemoScene demoScene;
            public float collisionFriction;
            public float constraintStiffness;
            public int solverIterations;
            public int particlesPerSide;
        }

        [SerializeField]
        Dropdown simType;
        [SerializeField]
        InputField muInputField;
        [SerializeField]
        InputField kInputField;
        [SerializeField]
        InputField itersIntputField;
        [SerializeField]
        InputField particlesInputField;

        const int DefaultSolverIterations = 8;
        const float DefaultConstraintStiffness = 2000f;
        const float DefaultCollisionFriction = 0.25f;

        static RunSettings pendingSettings;
        static bool hasPendingSettings;

        public static bool TryConsumeSettings(out RunSettings settings) {
            if (hasPendingSettings) {
                settings = pendingSettings;
                hasPendingSettings = false;
                return true;
            }

            settings = default;
            return false;
        }

        public void Run() {
            pendingSettings = new RunSettings {
                demoScene = ParseDemoScene(),
                collisionFriction = Mathf.Clamp01(ParseFloat(muInputField, DefaultCollisionFriction)),
                constraintStiffness = Mathf.Max(1e-3f, ParseFloat(kInputField, DefaultConstraintStiffness)),
                solverIterations = Mathf.Clamp(ParseInt(itersIntputField, DefaultSolverIterations), 1, 256),
            };
            hasPendingSettings = true;

            SceneManager.LoadScene("MainTask2", LoadSceneMode.Single);
        }

        ProjectionDynamicsCube.DemoScene ParseDemoScene() {
            if (simType == null)
                return ProjectionDynamicsCube.DemoScene.Hanging;

            if (simType.options != null && simType.value >= 0 && simType.value < simType.options.Count) {
                var optionText = simType.options[simType.value].text;
                if (Enum.TryParse(optionText, true, out ProjectionDynamicsCube.DemoScene sceneFromText))
                    return sceneFromText;
            }

            var enumValue = simType.value + 1;
            if (Enum.IsDefined(typeof(ProjectionDynamicsCube.DemoScene), enumValue))
                return (ProjectionDynamicsCube.DemoScene)enumValue;

            return ProjectionDynamicsCube.DemoScene.Hanging;
        }

        static int ParseParticlesPerSide(int rawValue) {
            rawValue = Mathf.Max(2, rawValue);

            if (rawValue <= 32)
                return rawValue;

            var side = Mathf.RoundToInt(Mathf.Pow(rawValue, 1f / 3f));
            return Mathf.Clamp(side, 2, 32);
        }

        static int ParseInt(InputField field, int defaultValue) {
            if (field == null || string.IsNullOrWhiteSpace(field.text))
                return defaultValue;

            return int.TryParse(field.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : defaultValue;
        }

        static float ParseFloat(InputField field, float defaultValue) {
            if (field == null || string.IsNullOrWhiteSpace(field.text))
                return defaultValue;

            var text = field.text.Trim();

            if (float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsed))
                return parsed;

            if (float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            if (float.TryParse(text.Replace(',', '.'), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            return defaultValue;
        }
    }
}
