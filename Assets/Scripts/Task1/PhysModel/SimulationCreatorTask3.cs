using Leopotam.Ecs;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Task1 {
    class SimulationCreatorTask3 : MonoBehaviour {
        [SerializeField] InputField l1Rel, l1, k1;
        [SerializeField] InputField l2Rel, l2, k2;
        [SerializeField] InputField l3Rel, l3, k3;
        [SerializeField] InputField mass1, mass2;
        [SerializeField] Dropdown solutionType;

        [SerializeField] GameObject pointPrefab, springPrefab;

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

            var spring3Param = new SpringParams() {
                length = ParseOrDefault(l3.text, 2),
                relaxedLength = ParseOrDefault(l3Rel.text, 1),
                k = ParseOrDefault(k3.text, 1),
            };

            float m1 = ParseOrDefault(mass1.text, 1);
            float m2 = ParseOrDefault(mass2.text, 1);

            Simulation.Create = (world, systems) => Create(
                world, systems,
                spring1Param, spring2Param, spring3Param,
                m1, m2,
                120f,
                solutionType.value
            );

            SceneManager.LoadScene("main");
            SceneManager.UnloadScene("Menu");
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
            SpringParams spring3Param,
            float mass1,
            float mass2,
            float muDeg,
            int dropdownIdx
        ) {
            var integratorVariants = new BaseIntegratorSystem[] {
            new TheoreticalSolverTask3IntegratorSystem(() => Time.fixedDeltaTime),
            new ExplicitEulerIntegratorSystem(() => Time.fixedDeltaTime),
            new SymplecticEulerIntegratorSystem(() => Time.fixedDeltaTime),
            new ImplicitEulerIntegratorSystem(() => Time.fixedDeltaTime),
            new VelocityVerletIntegratorSystem(() => Time.fixedDeltaTime),
        };
            systems.Add(integratorVariants[dropdownIdx]);

            float mu = muDeg * Mathf.Deg2Rad;
            float cosmu = Mathf.Cos(mu);
            float sinmu = Mathf.Sin(mu);

            Vector2 leftAnchor = Vector2.zero;

            Vector2 p1eq = new Vector2(spring1Param.length, 0f);
            Vector2 p2eq = new Vector2(spring1Param.length + spring2Param.length, 0f);

            Vector2 wallDir = new Vector2(cosmu, sinmu);
            Vector2 wallNormal = new Vector2(-sinmu, cosmu).normalized;

            Vector2 rightAnchor = p2eq - wallNormal * spring3Param.length;

            var point1Entity = world.NewEntity();
            {
                ref var pos = ref point1Entity.Get<Position>();
                pos.r = p1eq;

                ref var matPoint = ref point1Entity.Get<MaterialPoint>();
                matPoint.m = mass1;

                ref var theoretical = ref point1Entity.Get<TheoreticalPoint>();
                theoretical.solution = t => {
                    var state = AnalyticalSolutionTask3.Solution(
                        t,
                        spring1Param.k, spring2Param.k, spring3Param.k,
                        mass1, mass2,
                        spring1Param.relaxedLength,
                        spring2Param.relaxedLength,
                        spring3Param.relaxedLength,
                        muDeg,
                        p1eq,
                        p2eq
                    );
                    return (state.r1, state.v1);
                };

                Instantiate(pointPrefab)
                    .GetComponent<PointVisualiser>()
                    .SetEntity(point1Entity)
                    .SetColor(new Color(0f, 1f, 0f, 1f))
                    .AddTrack(Color.white);
            }

            var point2Entity = world.NewEntity();
            {
                ref var pos = ref point2Entity.Get<Position>();
                pos.r = p2eq;

                ref var matPoint = ref point2Entity.Get<MaterialPoint>();
                matPoint.m = mass2;

                ref var theoretical = ref point2Entity.Get<TheoreticalPoint>();
                theoretical.solution = t => {
                    var state = AnalyticalSolutionTask3.Solution(
                        t,
                        spring1Param.k, spring2Param.k, spring3Param.k,
                        mass1, mass2,
                        spring1Param.relaxedLength,
                        spring2Param.relaxedLength,
                        spring3Param.relaxedLength,
                        muDeg,
                        p1eq,
                        p2eq
                    );
                    return (state.r2, state.v2);
                };

                Instantiate(pointPrefab)
                    .GetComponent<PointVisualiser>()
                    .SetEntity(point2Entity)
                    .SetColor(new Color(0f, 0.6f, 1f, 1f))
                    .AddTrack(Color.yellow);
            }

            var leftAnchorEntity = world.NewEntity();
            leftAnchorEntity.Get<Position>().r = leftAnchor;

            var rightAnchorEntity = world.NewEntity();
            rightAnchorEntity.Get<Position>().r = rightAnchor;

            var spring1Entity = world.NewEntity();
            {
                ref var spring = ref spring1Entity.Get<Spring>();
                spring.k = spring1Param.k;
                spring.relaxedLength = spring1Param.relaxedLength;
                spring.joint1 = leftAnchorEntity;
                spring.joint2 = point1Entity;

                Instantiate(springPrefab)
                    .GetComponent<SpringVisualiser>()
                    .SetEntity(spring1Entity);
            }

            var spring2Entity = world.NewEntity();
            {
                ref var spring = ref spring2Entity.Get<Spring>();
                spring.k = spring2Param.k;
                spring.relaxedLength = spring2Param.relaxedLength;
                spring.joint1 = point1Entity;
                spring.joint2 = point2Entity;

                Instantiate(springPrefab)
                    .GetComponent<SpringVisualiser>()
                    .SetEntity(spring2Entity);
            }

            var spring3Entity = world.NewEntity();
            {
                ref var spring = ref spring3Entity.Get<Spring>();
                spring.k = spring3Param.k;
                spring.relaxedLength = spring3Param.relaxedLength;
                spring.joint1 = rightAnchorEntity;
                spring.joint2 = point2Entity;

                Instantiate(springPrefab)
                    .GetComponent<SpringVisualiser>()
                    .SetEntity(spring3Entity);
            }
        }

        public class TheoreticalSolverTask3IntegratorSystem : BaseIntegratorSystem {
            private float elapsed;
            private readonly System.Func<float> deltaTimeProvider;

            private EcsFilter<TheoreticalPoint, Position> theoreticalPoints = null;
            private EcsFilter<MaterialPoint> materialPoints = null;

            public TheoreticalSolverTask3IntegratorSystem(System.Func<float> deltaTimeProvider) : base(deltaTimeProvider) {
                this.deltaTimeProvider = deltaTimeProvider;
            }

            public override void Run() {
                elapsed += deltaTimeProvider();

                foreach (var i in theoreticalPoints) {
                    ref var theoretical = ref theoreticalPoints.Get1(i);
                    ref var position = ref theoreticalPoints.Get2(i);

                    var (r, v) = theoretical.solution(elapsed);
                    position.r = r;

                    var entity = theoreticalPoints.GetEntity(i);
                    if (entity.Has<MaterialPoint>()) {
                        ref var point = ref entity.Get<MaterialPoint>();
                        point.v = v;
                        point.a = Vector2.zero;
                    }
                }

                foreach (var i in materialPoints) {
                    ref var point = ref materialPoints.Get1(i);
                    point.a = Vector2.zero;
                }
            }
        }

        public static class AnalyticalSolutionTask3 {
            public struct State {
                public Vector2 r1;
                public Vector2 v1;
                public Vector2 r2;
                public Vector2 v2;
            }

            // Аналитическое решение в линейном приближении около выбра��ного равновесия.
            public static State Solution(
                float t,
                float k1, float k2, float k3,
                float m1, float m2,
                float l1Relaxed, float l2Relaxed, float l3Relaxed,
                float muDeg,
                Vector2 r10,
                Vector2 r20
            ) {
                float mu = muDeg * Mathf.Deg2Rad;

                Vector2 n = new Vector2(-Mathf.Sin(mu), Mathf.Cos(mu)).normalized;

                float a11 = -(k1 + k2) / m1;
                float a12 = 0f;
                float a13 = k2 / m1;
                float a14 = 0f;

                float a21 = 0f;
                float a22 = 0f;
                float a23 = 0f;
                float a24 = 0f;

                float nnxx = n.x * n.x;
                float nnxy = n.x * n.y;
                float nnyy = n.y * n.y;

                float a31 = k2 / m2;
                float a32 = 0f;
                float a33 = -(k2 + k3 * nnxx) / m2;
                float a34 = -(k3 * nnxy) / m2;

                float a41 = 0f;
                float a42 = 0f;
                float a43 = -(k3 * nnxy) / m2;
                float a44 = -(k3 * nnyy) / m2;

                Matrix4x4 A = new Matrix4x4();
                A[0, 0] = a11; A[0, 1] = a12; A[0, 2] = a13; A[0, 3] = a14;
                A[1, 0] = a21; A[1, 1] = a22; A[1, 2] = a23; A[1, 3] = a24;
                A[2, 0] = a31; A[2, 1] = a32; A[2, 2] = a33; A[2, 3] = a34;
                A[3, 0] = a41; A[3, 1] = a42; A[3, 2] = a43; A[3, 3] = a44;

                Vector4 q0 = Vector4.zero;
                Vector4 v0 = Vector4.zero;

                q0.x = 0.1f;

                float b11 = (k1 + k2) / m1;
                float b12 = -k2 / m1;
                float b21 = -k2 / m2;
                float b22 = (k2 + k3 * nnxx) / m2;

                Solve2DOscillation(
                    t,
                    b11, b12, b21, b22,
                    new Vector2(q0.x, q0.z),
                    new Vector2(v0.x, v0.z),
                    out Vector2 x,
                    out Vector2 vx
                );

                float omegaY = Mathf.Sqrt(Mathf.Max(k3 * nnyy / m2, 0f));
                float y2 = 0f;
                float vy2 = 0f;

                float y1 = 0f;
                float vy1 = 0f;

                return new State {
                    r1 = r10 + new Vector2(x.x, y1),
                    v1 = new Vector2(vx.x, vy1),
                    r2 = r20 + new Vector2(x.y, y2),
                    v2 = new Vector2(vx.y, vy2)
                };
            }

            private static void Solve2DOscillation(
                float t,
                float b11, float b12, float b21, float b22,
                Vector2 q0,
                Vector2 v0,
                out Vector2 q,
                out Vector2 v
            ) {
                float trace = b11 + b22;
                float det = b11 * b22 - b12 * b21;
                float disc = Mathf.Max(trace * trace - 4f * det, 0f);
                float sqrtDisc = Mathf.Sqrt(disc);

                float lambda1 = 0.5f * (trace + sqrtDisc);
                float lambda2 = 0.5f * (trace - sqrtDisc);

                float omega1 = Mathf.Sqrt(Mathf.Max(lambda1, 0f));
                float omega2 = Mathf.Sqrt(Mathf.Max(lambda2, 0f));

                Vector2 e1 = BuildEigenvector2D(b11, b12, b21, b22, lambda1);
                Vector2 e2 = BuildEigenvector2D(b11, b12, b21, b22, lambda2);

                Vector2 z0 = Solve2x2(e1.x, e2.x, e1.y, e2.y, q0);
                Vector2 zDot0 = Solve2x2(e1.x, e2.x, e1.y, e2.y, v0);

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

                q = e1 * z1 + e2 * z2;
                v = e1 * z1Dot + e2 * z2Dot;
            }

            private static Vector2 Solve2x2(
                float m11, float m12,
                float m21, float m22,
                Vector2 rhs
            ) {
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

            private static Vector2 BuildEigenvector2D(
                float m11, float m12,
                float m21, float m22,
                float lambda
            ) {
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
