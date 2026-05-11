using Leopotam.Ecs;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Task1 {
    class SimulationCreatorTask1 : MonoBehaviour {
        [SerializeField]
        InputField l1Rel, l1, k1, l2Rel, l2, k2, mass;
        [SerializeField]
        Dropdown solutionType;

        [SerializeField]
        GameObject pointPrefab, springPrefab;

        float ParseOrDefault(string raw, float defau) {
            if (!float.TryParse(raw, out float res)) {
                res = defau;
            }

            return res;
        }

        public void StartSimulation() {
            var spring1Param = new SpringParams() {
                length = ParseOrDefault(l1.text, 2),
                relaxedLength = ParseOrDefault(l1Rel.text, 1),
                k = ParseOrDefault(k1.text, 1),
            };
            var spring2Param = new SpringParams() {
                length = ParseOrDefault(l2.text, 2),
                relaxedLength = ParseOrDefault(l2Rel.text, 1),
                k = ParseOrDefault(k2.text, 1),
            };
            var m = ParseOrDefault(mass.text, 1);

            Simulation.Create = (world, systems) => Create(world, systems, spring1Param, spring2Param, m, 120, solutionType.value);

            SceneManager.LoadScene("MainTask1");
            SceneManager.UnloadScene(SceneManager.GetActiveScene().name);
        }

        public struct SpringParams {
            public float length;
            public float relaxedLength;
            public float k;
        }

        void Create(
            EcsWorld world,
            EcsSystems systems,
            SpringParams spring1Param,
            SpringParams spring2Param,
            float mass,
            float mu,
            int dropdownIdx
        ) {
            var integratorVariants = new BaseIntegratorSystem[] {
            new TheoreticalSolverIntegratorSystem(() => Time.fixedDeltaTime),
            new ExplicitEulerIntegratorSystem(() => Time.fixedDeltaTime),
            new SymplecticEulerIntegratorSystem(() => Time.fixedDeltaTime),
            new ImplicitEulerIntegratorSystem(() => Time.fixedDeltaTime),
            new VelocityVerletIntegratorSystem(() => Time.fixedDeltaTime),
        };
            systems.Add(integratorVariants[dropdownIdx]);


            var (l1, l2) = (spring1Param.length, spring2Param.length);
            mu *= Mathf.Deg2Rad;
            var (sinmu, cosmu, tanmu) = (Mathf.Sin(mu), Mathf.Cos(mu), Mathf.Tan(mu));

            var pointEntity = world.NewEntity();
            {
                ref var pos = ref pointEntity.Get<Position>();
                pos.r = new(
                    (l2 * cosmu + l1) / tanmu + l2 * sinmu,
                    l1
                );

                ref var matPoint = ref pointEntity.Get<MaterialPoint>();
                matPoint.m = mass;

                Vector2 r0 = pos.r;
                ref var theoretical = ref pointEntity.Get<TheoreticalPoint>();
                theoretical.solution = t => AnalyticalSolution.Solution(t, spring1Param.k, spring2Param.k, mass, spring1Param.relaxedLength, spring2Param.relaxedLength, r0);

                Instantiate(pointPrefab)
                    .GetComponent<PointVisualiser>()
                    .SetEntity(pointEntity)
                    .SetColor(new Color(0, 1, 0, 1))
                    .AddTrack(Color.white);
            }

            var spring1ConnectionEntity = world.NewEntity();
            {
                ref var pos = ref spring1ConnectionEntity.Get<Position>();
                pos.r = new(
                    (l2 * cosmu + l1) / tanmu + l2 * sinmu,
                    0
                );
            }

            var spring2ConnectionEntity = world.NewEntity();
            {
                ref var pos = ref spring2ConnectionEntity.Get<Position>();
                pos.r = new(
                    (l2 * cosmu + l1) / tanmu,
                    l2 * cosmu + l1
                );
            }

            var spring1Entity = world.NewEntity();
            {
                ref var spring = ref spring1Entity.Get<Spring>();
                spring.k = spring1Param.k;
                spring.relaxedLength = spring1Param.relaxedLength;
                spring.joint1 = spring1ConnectionEntity;
                spring.joint2 = pointEntity;

                Instantiate(springPrefab)
                    .GetComponent<SpringVisualiser>()
                    .SetEntity(spring1Entity);
            }

            var spring2Entity = world.NewEntity();
            {
                ref var spring = ref spring2Entity.Get<Spring>();
                spring.k = spring2Param.k;
                spring.relaxedLength = spring2Param.relaxedLength;
                spring.joint1 = spring2ConnectionEntity;
                spring.joint2 = pointEntity;

                Instantiate(springPrefab)
                    .GetComponent<SpringVisualiser>()
                    .SetEntity(spring2Entity);
            }
        }

        public static class AnalyticalSolution {
            public static (Vector2 position, Vector2 velocity) Solution(
                float t,
                float k1,
                float k2,
                float mass,
                float l1,
                float l2,
                Vector2 r0
            ) {
                const float sqrt3 = 1.7320508075688772f;

                float a11 = -0.75f * k2 / mass;
                float a12 = -0.25f * sqrt3 * k2 / mass;
                float a21 = -0.25f * sqrt3 * k2 / mass;
                float a22 = -(k1 + 0.25f * k2) / mass;

                Vector2 c = new Vector2(
                    0.5f * sqrt3 * k2 * l2 / mass,
                    (k1 * l1 + 0.5f * k2 * l2) / mass
                );

                Vector2 qEq = Solve2x2(
                    a11, a12,
                    a21, a22,
                    -c
                );

                Vector2 xi0 = r0 - qEq;

                float m11 = -a11;
                float m12 = -a12;
                float m21 = -a21;
                float m22 = -a22;

                float trace = m11 + m22;
                float det = m11 * m22 - m12 * m21;
                float disc = Mathf.Max(trace * trace - 4f * det, 0f);
                float sqrtDisc = Mathf.Sqrt(disc);

                float lambda1 = 0.5f * (trace + sqrtDisc);
                float lambda2 = 0.5f * (trace - sqrtDisc);

                float omega1 = Mathf.Sqrt(Mathf.Max(lambda1, 0f));
                float omega2 = Mathf.Sqrt(Mathf.Max(lambda2, 0f));

                Vector2 e1 = BuildEigenvector(m11, m12, m21, m22, lambda1);
                Vector2 e2 = BuildEigenvector(m11, m12, m21, m22, lambda2);

                Vector2 z0 = Solve2x2(
                    e1.x, e2.x,
                    e1.y, e2.y,
                    xi0
                );

                Vector2 zDot0 = Solve2x2(
                    e1.x, e2.x,
                    e1.y, e2.y,
                    Vector2.zero
                );

                float z1, z2, z1Dot, z2Dot;

                if (omega1 > 1e-6f) {
                    z1 = z0.x * Mathf.Cos(omega1 * t) + (zDot0.x / omega1) * Mathf.Sin(omega1 * t);
                    z1Dot = -z0.x * omega1 * Mathf.Sin(omega1 * t) + zDot0.x * Mathf.Cos(omega1 * t);
                } else {
                    z1 = z0.x + zDot0.x * t;
                    z1Dot = zDot0.x;
                }

                if (omega2 > 1e-6f) {
                    z2 = z0.y * Mathf.Cos(omega2 * t) + (zDot0.y / omega2) * Mathf.Sin(omega2 * t);
                    z2Dot = -z0.y * omega2 * Mathf.Sin(omega2 * t) + zDot0.y * Mathf.Cos(omega2 * t);
                } else {
                    z2 = z0.y + zDot0.y * t;
                    z2Dot = zDot0.y;
                }

                Vector2 xi = e1 * z1 + e2 * z2;
                Vector2 xiDot = e1 * z1Dot + e2 * z2Dot;

                Vector2 position = qEq + xi;
                Vector2 velocity = xiDot;

                return (position, velocity);
            }

            private static Vector2 Solve2x2(
                float m11, float m12,
                float m21, float m22,
                Vector2 rhs) {
                float det = m11 * m22 - m12 * m21;

                if (Mathf.Abs(det) < 1e-8f) {
                    Debug.LogError("Solve2x2 failed: determinant is too small.");
                    return Vector2.zero;
                }

                float inv11 = m22 / det;
                float inv12 = -m12 / det;
                float inv21 = -m21 / det;
                float inv22 = m11 / det;

                return new Vector2(
                    inv11 * rhs.x + inv12 * rhs.y,
                    inv21 * rhs.x + inv22 * rhs.y
                );
            }

            private static Vector2 BuildEigenvector(
                float m11, float m12,
                float m21, float m22,
                float lambda) {
                float a = m11 - lambda;
                float b = m12;
                float c = m21;
                float d = m22 - lambda;

                Vector2 v;

                if (Mathf.Abs(b) > Mathf.Abs(c)) {
                    v = new Vector2(-b, a);
                } else {
                    v = new Vector2(d, -c);
                }

                if (v.sqrMagnitude < 1e-8f) {
                    v = new Vector2(1f, 0f);
                }

                return v.normalized;
            }
        }
    }
}
