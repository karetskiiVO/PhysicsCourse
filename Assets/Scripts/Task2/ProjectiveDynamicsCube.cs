using System;
using System.Collections.Generic;

using UnityEngine;

namespace Task2 {
    public class ProjectionDynamicsCube : MonoBehaviour {
        public enum DemoScene {
            Hanging = 1,
            Drop = 2,
            Overconstrained = 3
        }

        public float texelSize = 0.25f;
        public int particlesPerSide = 4;
        public float mass = 1f;
        public float stiffness = 3000f;
        public float particleSize = 0.2f;

        public float volumeStiffness = 8000f;
        public float complianceScale = 1f;

        public int substeps = 2;
        public int solverIterations = 12;
        public Vector3 gravity = new(0, -9.81f, 0);

        public float floorY = 0f;
        public float friction = 0.75f;
        public float restitution = 0.12f;
        public float selfCollisionRadius = 0.11f;

        public DemoScene startScene = DemoScene.Hanging;
        public Vector3 rigidBallOffset = new(1.8f, 2.8f, 0f);
        public float rigidBallRadius = 0.45f;
        public float rigidBallCompliance = 1e-10f;

        public float grabRayLength = 100f;
        public float grabSelectionRadius = 0.35f;
        public float holdMinDistance = 0.75f;
        public float holdMaxDistance = 8f;

        class Particle {
            public Vector3 position;
            public Vector3 predicted;
            public Vector3 velocity;
            public Vector3 fixedPosition;
            public float invMass;
            public float radius;
            public bool isFixed;
            public bool isBall;
            public GameObject go;
            public Renderer renderer;
        }

        class Tetraider {
            public int i0, i1, i2, i3;
            public Mat3x3 invDm;
            public float restVolume;
            public Vector3 g0, g1, g2, g3;
            public float lambdaHydro;
            public float lambdaDeviatoric;
        }

        class DistanceConstraint {
            public int i0, i1;
            public float restLength;
            public float compliance;
            public float lambda;
        }

        struct Mat3x3 {
            public Vector3 c0, c1, c2;

            public Mat3x3(Vector3 c0, Vector3 c1, Vector3 c2) {
                this.c0 = c0;
                this.c1 = c1;
                this.c2 = c2;
            }

            public static Vector3 operator *(Mat3x3 m, Vector3 v) => m.c0 * v.x + m.c1 * v.y + m.c2 * v.z;
            public static Mat3x3 operator *(Mat3x3 a, Mat3x3 b) => new(a * b.c0, a * b.c1, a * b.c2);
            public static Mat3x3 operator *(Mat3x3 m, float s) => new(m.c0 * s, m.c1 * s, m.c2 * s);

            public readonly float determinant => Vector3.Dot(c0, Vector3.Cross(c1, c2));

            public float frobeniusSqr => c0.sqrMagnitude + c1.sqrMagnitude + c2.sqrMagnitude;

            public readonly Mat3x3 transposed => new(
                    new(c0.x, c1.x, c2.x),
                    new(c0.y, c1.y, c2.y),
                    new(c0.z, c1.z, c2.z)
                );

            public readonly Vector3 GetRow(int r) {
                return r switch {
                    0 => new Vector3(c0.x, c1.x, c2.x),
                    1 => new Vector3(c0.y, c1.y, c2.y),
                    _ => new Vector3(c0.z, c1.z, c2.z),
                };
            }

            public readonly Mat3x3 inversed {
                get {
                    Vector3 r0 = Vector3.Cross(c1, c2);
                    Vector3 r1 = Vector3.Cross(c2, c0);
                    Vector3 r2 = Vector3.Cross(c0, c1);
                    float det = Vector3.Dot(c0, r0);
                    return new Mat3x3(r0, r1, r2).transposed * (1.0f / det);
                }
            }

            public readonly Mat3x3 cofactor => new(Vector3.Cross(c1, c2), Vector3.Cross(c2, c0), Vector3.Cross(c0, c1));
        }

        List<Particle> particles = new();
        List<Tetraider> tets = new();
        List<DistanceConstraint> rigidConstraints = new();
        HashSet<ulong> neighborPairs = new();
        List<GameObject> spawned = new();

        [SerializeField]
        Camera cam;
        [SerializeField]
        Transform grabRayTransform;
        GameObject floorVisual;

        Material particleRuntimeMaterial;
        Material floorRuntimeMaterial;

        int cubeStart;
        int cubeCount;

        int grabbed = -1;
        float holdDistance = 2f;

        DemoScene currentScene;

        void Start() {
            if (Loader.TryConsumeSettings(out var settings)) {
                ApplyLoadedSettings(settings);
            }

            if (cam == null)
                cam = Camera.main;

            if (grabRayTransform == null && cam != null)
                grabRayTransform = cam.transform;

            EnsureRuntimeMaterials();
            BuildScene(startScene);
        }

        void EnsureRuntimeMaterials() {
            if (particleRuntimeMaterial == null)
                particleRuntimeMaterial = CreateCompatibleMaterial("PD_ParticleMat");

            if (floorRuntimeMaterial == null)
                floorRuntimeMaterial = CreateCompatibleMaterial("PD_FloorMat");
        }

        Material CreateCompatibleMaterial(string materialName) {
            var hasRenderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;

            string[] candidates = hasRenderPipeline
                ? new[] {
                    "Universal Render Pipeline/Lit",
                    "Universal Render Pipeline/Simple Lit",
                    "HDRP/Lit",
                    "Sprites/Default",
                    "Unlit/Color",
                    "Standard",
                    "Legacy Shaders/Diffuse"
                }
                : new[] {
                    "Standard",
                    "Legacy Shaders/Diffuse",
                    "Sprites/Default",
                    "Unlit/Color"
                };

            for (int i = 0; i < candidates.Length; i++) {
                var shader = Shader.Find(candidates[i]);
                if (shader != null) {
                    var material = new Material(shader);
                    material.name = materialName;
                    return material;
                }
            }

            Debug.LogWarning("No compatible runtime shader was found. Falling back to internal error shader.");
            return new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        void ApplyLoadedSettings(Loader.RunSettings settings) {
            startScene = settings.demoScene;
            friction = Mathf.Clamp01(settings.collisionFriction);
            stiffness = Mathf.Max(1e-3f, settings.constraintStiffness);
            solverIterations = Mathf.Clamp(settings.solverIterations, 1, 256);
        }

        void Update() {
            HandleHotkeys();
            HandleMouse();

            StepSimulation(Time.deltaTime);

            SyncVisuals();
        }

        void HandleHotkeys() {
            if (Input.GetKeyDown(KeyCode.Alpha1)) BuildScene(DemoScene.Hanging);
            if (Input.GetKeyDown(KeyCode.Alpha2)) BuildScene(DemoScene.Drop);
            if (Input.GetKeyDown(KeyCode.Alpha3)) BuildScene(DemoScene.Overconstrained);
            if (Input.GetKeyDown(KeyCode.R)) BuildScene(currentScene);
        }

        void HandleMouse() {
            if (!TryGetGrabRay(out var ray)) return;

            if (Input.GetMouseButtonDown(0)) {
                if (TryPickParticleFromRay(ray, out var idx, out var distanceAlongRay)) {
                    grabbed = idx;
                    holdDistance = Mathf.Clamp(distanceAlongRay, holdMinDistance, holdMaxDistance);
                }
            }

            if (Input.GetMouseButtonUp(0)) grabbed = -1;
        }

        bool TryPickParticleFromRay(Ray ray, out int pickedIndex, out float distanceAlongRay) {
            pickedIndex = -1;
            distanceAlongRay = 0f;

            var dir = ray.direction;
            var dirLen = dir.magnitude;
            if (dirLen < Mathf.Epsilon)
                return false;

            dir /= dirLen;

            float bestRadialSqr = float.PositiveInfinity;
            float bestT = 0f;

            for (int i = 0; i < particles.Count; i++) {
                var p = particles[i];
                if (p.isFixed) continue;

                var toParticle = p.position - ray.origin;
                var t = Vector3.Dot(toParticle, dir);
                if (t < 0f || t > grabRayLength)
                    continue;

                var closestPoint = ray.origin + dir * t;
                var radialSqr = (p.position - closestPoint).sqrMagnitude;
                var pickRadius = Mathf.Max(grabSelectionRadius, p.radius * 1.25f);
                if (radialSqr > pickRadius * pickRadius)
                    continue;

                if (radialSqr < bestRadialSqr || (Mathf.Approximately(radialSqr, bestRadialSqr) && t < bestT)) {
                    bestRadialSqr = radialSqr;
                    bestT = t;
                    pickedIndex = i;
                }
            }

            if (pickedIndex < 0)
                return false;

            distanceAlongRay = bestT;
            return true;
        }

        Vector3 GetGrabPoint() {
            if (grabbed < 0) return Vector3.zero;
            if (!TryGetGrabRay(out var ray)) return Vector3.zero;
            return ray.GetPoint(holdDistance);
        }

        bool TryGetGrabRay(out Ray ray) {
            if (grabRayTransform == null) {
                if (cam == null)
                    cam = Camera.main;

                if (cam != null)
                    grabRayTransform = cam.transform;
            }

            if (grabRayTransform == null) {
                ray = default;
                return false;
            }

            ray = new Ray(grabRayTransform.position, grabRayTransform.forward);
            return true;
        }

        int FindParticleByCollider(Collider c) {
            for (int i = 0; i < particles.Count; i++) {
                if (particles[i].go != null && particles[i].go.GetComponent<Collider>() == c) return i;
            }
            return -1;
        }

        void BuildScene(DemoScene scene) {
            ClearScene();
            currentScene = scene;
            grabbed = -1;

            CreateFloorVisual();

            Vector3 cubeCenter = transform.position + ((scene == DemoScene.Drop) ? new Vector3(0, 4.2f, 0) : new Vector3(0, 2.6f, 0));
            CreateCube(cubeCenter);
            CreateRigidBall(transform.position + rigidBallOffset);

            ConfigureScene(scene);
            SyncVisuals();
        }

        void ClearScene() {
            foreach (var p in particles) {
                if (p.go != null) Destroy(p.go);
            }

            foreach (var go in spawned) {
                if (go != null) Destroy(go);
            }

            if (floorVisual != null) Destroy(floorVisual);

            particles.Clear();
            tets.Clear();
            rigidConstraints.Clear();
            neighborPairs.Clear();
        }

        void CreateFloorVisual() {
            floorVisual = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorVisual.name = "Floor";
            floorVisual.transform.position = new Vector3(0, floorY, 0);
            floorVisual.transform.localScale = Vector3.one * 1.5f;

            var floorCollider = floorVisual.GetComponent<Collider>();
            if (floorCollider != null)
                floorCollider.enabled = false;

            var r = floorVisual.GetComponent<Renderer>();
            var floorMat = new Material(floorRuntimeMaterial);
            floorMat.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            r.material = floorMat;

            spawned.Add(floorVisual);
        }

        void CreateCube(Vector3 center) {
            cubeStart = particles.Count;
            var cubeSize = (particlesPerSide - 1) * texelSize;
            var startPos = center - 0.5f * cubeSize * Vector3.one;

            for (int x = 0; x < particlesPerSide; x++) {
                for (int y = 0; y < particlesPerSide; y++) {
                    for (int z = 0; z < particlesPerSide; z++) {
                        var pos = startPos + new Vector3(x, y, z) * texelSize;
                        CreateParticle(pos, Color.red, false, false);
                    }
                }
            }

            cubeCount = particles.Count - cubeStart;
            CreateCubeTets();
        }

        void CreateCubeTets() {
            for (int x = 0; x < particlesPerSide - 1; x++) {
                for (int y = 0; y < particlesPerSide - 1; y++) {
                    for (int z = 0; z < particlesPerSide - 1; z++) {
                        var p000 = Idx(x, y, z);
                        var p100 = Idx(x + 1, y, z);
                        var p010 = Idx(x, y + 1, z);
                        var p110 = Idx(x + 1, y + 1, z);
                        var p001 = Idx(x, y, z + 1);
                        var p101 = Idx(x + 1, y, z + 1);
                        var p011 = Idx(x, y + 1, z + 1);
                        var p111 = Idx(x + 1, y + 1, z + 1);

                        AddTet(p000, p100, p110, p111);
                        AddTet(p000, p110, p010, p111);
                        AddTet(p000, p010, p011, p111);
                        AddTet(p000, p011, p001, p111);
                        AddTet(p000, p001, p101, p111);
                        AddTet(p000, p101, p100, p111);
                    }
                }
            }
        }

        int Idx(int x, int y, int z) => cubeStart + x + particlesPerSide * (y + particlesPerSide * z);

        int CreateParticle(Vector3 position, Color color, bool fixedPoint, bool ballPoint) {
            var p = new Particle {
                position = position,
                predicted = position,
                velocity = Vector3.zero,
                fixedPosition = position,
                isFixed = fixedPoint,
                isBall = ballPoint,
                invMass = fixedPoint ? 0f : 1f / Mathf.Max(1e-6f, mass),
                radius = selfCollisionRadius > 0f ? selfCollisionRadius : particleSize * 0.5f
            };

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * particleSize;
            go.name = $"Particle_{particles.Count}";
            var renderer = go.GetComponent<Renderer>();

            var particleMat = new Material(particleRuntimeMaterial);
            particleMat.color = color;
            renderer.material = particleMat;

            p.go = go;
            p.renderer = renderer;

            particles.Add(p);
            spawned.Add(go);

            return particles.Count - 1;
        }

        void SetFixed(int index, bool value, Vector3? target = null) {
            var p = particles[index];
            p.isFixed = value;
            p.invMass = value ? 0f : 1f / Mathf.Max(1e-6f, mass);
            if (target.HasValue) {
                p.fixedPosition = target.Value;
                p.position = target.Value;
                p.predicted = target.Value;
            }
        }

        void AddTet(int i0, int i1, int i2, int i3) {
            var x0 = particles[i0].position;
            var x1 = particles[i1].position;
            var x2 = particles[i2].position;
            var x3 = particles[i3].position;

            var Dm = new Mat3x3(x1 - x0, x2 - x0, x3 - x0);
            float det = Dm.determinant;

            if (Mathf.Abs(det) < 1e-9f)
                return;

            if (det < 0f) {
                (i3, i2) = (i2, i3);
                x2 = particles[i2].position;
                x3 = particles[i3].position;
                Dm = new Mat3x3(x1 - x0, x2 - x0, x3 - x0);
                det = Dm.determinant;
            }

            var invDm = Dm.inversed;

            var t = new Tetraider {
                i0 = i0,
                i1 = i1,
                i2 = i2,
                i3 = i3,
                invDm = invDm,
                restVolume = Mathf.Abs(det) / 6f,
                g1 = invDm.GetRow(0),
                g2 = invDm.GetRow(1),
                g3 = invDm.GetRow(2)
            };
            t.g0 = -(t.g1 + t.g2 + t.g3);
            tets.Add(t);

            AddNeighbor(i0, i1);
            AddNeighbor(i0, i2);
            AddNeighbor(i0, i3);
            AddNeighbor(i1, i2);
            AddNeighbor(i1, i3);
            AddNeighbor(i2, i3);
        }

        void AddNeighbor(int a, int b) {
            neighborPairs.Add(PairKey(a, b));
        }

        ulong PairKey(int a, int b) {
            var x = (uint)Mathf.Min(a, b);
            var y = (uint)Mathf.Max(a, b);
            return ((ulong)x << 32) | y;
        }

        void CreateRigidBall(Vector3 center) {
            int start = particles.Count;
            var ids = new List<int> {
                CreateParticle(center, Color.green, false, true),
                CreateParticle(center + Vector3.right * rigidBallRadius, Color.green, false, true),
                CreateParticle(center - Vector3.right * rigidBallRadius, Color.green, false, true),
                CreateParticle(center + Vector3.up * rigidBallRadius, Color.green, false, true),
                CreateParticle(center - Vector3.up * rigidBallRadius, Color.green, false, true),
                CreateParticle(center + Vector3.forward * rigidBallRadius, Color.green, false, true),
                CreateParticle(center - Vector3.forward * rigidBallRadius, Color.green, false, true)
            };

            for (int i = 0; i < ids.Count; i++) {
                for (int j = i + 1; j < ids.Count; j++) {
                    var rest = Vector3.Distance(particles[ids[i]].position, particles[ids[j]].position);
                    rigidConstraints.Add(new DistanceConstraint {
                        i0 = ids[i],
                        i1 = ids[j],
                        restLength = rest,
                        compliance = rigidBallCompliance,
                        lambda = 0f
                    });
                    AddNeighbor(ids[i], ids[j]);
                }
            }
        }

        void ConfigureScene(DemoScene scene) {
            int s = particlesPerSide - 1;

            if (scene == DemoScene.Hanging) {
                SetFixed(Idx(0, s, 0), true);
                SetFixed(Idx(s, s, 0), true);
                SetFixed(Idx(0, s, s), true);
                SetFixed(Idx(s, s, s), true);
            } else if (scene == DemoScene.Drop) {
            } else if (scene == DemoScene.Overconstrained) {
                var a = Idx(0, s, 0);
                var b = Idx(s, s, 0);
                var c = Idx(0, s, s);
                var d = Idx(s, s, s);

                SetFixed(a, true, particles[a].position + new Vector3(-texelSize * 2f, 0f, 0f));
                SetFixed(b, true, particles[b].position + new Vector3(+texelSize * 2f, texelSize, 0f));
                SetFixed(c, true, particles[c].position + new Vector3(0f, 0f, -texelSize * 2f));
                SetFixed(d, true, particles[d].position + new Vector3(0f, texelSize, +texelSize * 2f));
            }
        }

        void StepSimulation(float dt) {
            dt = Mathf.Min(dt, 1f / 20f);
            var ss = Mathf.Max(1, substeps);
            var h = dt / ss;

            for (int sub = 0; sub < ss; sub++) {
                for (int i = 0; i < tets.Count; i++) {
                    tets[i].lambdaHydro = 0f;
                    tets[i].lambdaDeviatoric = 0f;
                }

                for (int i = 0; i < rigidConstraints.Count; i++) rigidConstraints[i].lambda = 0f;


                for (int i = 0; i < particles.Count; i++) {
                    var p = particles[i];

                    if (p.isFixed) {
                        p.predicted = p.fixedPosition;
                        p.velocity = Vector3.zero;
                        continue;
                    }

                    p.velocity += gravity * h;
                    p.predicted = p.position + p.velocity * h;
                }

                for (int iter = 0; iter < solverIterations; iter++) {
                    EnforceAnchors();

                    for (int i = 0; i < tets.Count; i++) {
                        SolveTetDeviatoric(tets[i], h);
                        SolveTetHydro(tets[i], h);
                    }

                    for (int i = 0; i < rigidConstraints.Count; i++) SolveDistance(rigidConstraints[i], h);

                    SolveSelfCollisions();
                    SolveFloorPositions();
                    EnforceAnchors();
                }

                for (int i = 0; i < particles.Count; i++) {
                    var p = particles[i];

                    if (p.isFixed) {
                        p.position = p.fixedPosition;
                        p.predicted = p.fixedPosition;
                        p.velocity = Vector3.zero;
                        continue;
                    }

                    if (i == grabbed) {
                        p.position = p.predicted;
                        p.velocity = Vector3.zero;
                        continue;
                    }

                    var oldPos = p.position;
                    p.position = p.predicted;
                    p.velocity = (p.position - oldPos) / Mathf.Max(1e-6f, h);
                    ApplyFloorVelocity(p);
                }
            }
        }

        void EnforceAnchors() {
            for (int i = 0; i < particles.Count; i++) {
                if (particles[i].isFixed) particles[i].predicted = particles[i].fixedPosition;
            }

            if (grabbed >= 0 && grabbed < particles.Count && !particles[grabbed].isFixed) {
                particles[grabbed].predicted = GetGrabPoint();
            }
        }

        void SolveTetDeviatoric(Tetraider t, float h) {
            var x0 = particles[t.i0].predicted;
            var x1 = particles[t.i1].predicted;
            var x2 = particles[t.i2].predicted;
            var x3 = particles[t.i3].predicted;

            var Ds = new Mat3x3(x1 - x0, x2 - x0, x3 - x0);
            var F = Ds * t.invDm;

            float cd = Mathf.Sqrt(Mathf.Max(Mathf.Epsilon, F.frobeniusSqr));
            var dCdF = F * (1.0f / cd);

            float compliance = complianceScale / Mathf.Max(1e-6f, stiffness * t.restVolume);
            ProjectTetScalar(t, ref t.lambdaDeviatoric, cd, dCdF, compliance, h);
        }

        void SolveTetHydro(Tetraider t, float h) {
            var x0 = particles[t.i0].predicted;
            var x1 = particles[t.i1].predicted;
            var x2 = particles[t.i2].predicted;
            var x3 = particles[t.i3].predicted;

            var Ds = new Mat3x3(x1 - x0, x2 - x0, x3 - x0);
            var F = Ds * t.invDm;

            var gamma = 1f + stiffness / Mathf.Max(Mathf.Epsilon, volumeStiffness);
            var detF = F.determinant;
            var ch = detF - gamma;
            var dCdF = F.cofactor;

            var compliance = complianceScale / Mathf.Max(Mathf.Epsilon, volumeStiffness * t.restVolume);
            ProjectTetScalar(t, ref t.lambdaHydro, ch, dCdF, compliance, h);
        }

        void ProjectTetScalar(Tetraider t, ref float lambda, float C, Mat3x3 dCdF, float compliance, float h) {
            var p0 = particles[t.i0];
            var p1 = particles[t.i1];
            var p2 = particles[t.i2];
            var p3 = particles[t.i3];

            var g0 = dCdF * t.g0;
            var g1 = dCdF * t.g1;
            var g2 = dCdF * t.g2;
            var g3 = dCdF * t.g3;

            var w =
                p0.invMass * g0.sqrMagnitude +
                p1.invMass * g1.sqrMagnitude +
                p2.invMass * g2.sqrMagnitude +
                p3.invMass * g3.sqrMagnitude;

            var alphaTilde = compliance / Mathf.Max(Mathf.Epsilon, h * h);
            var dl = (-C - alphaTilde * lambda) / Mathf.Max(Mathf.Epsilon, w + alphaTilde);

            p0.predicted += p0.invMass * dl * g0;
            p1.predicted += p1.invMass * dl * g1;
            p2.predicted += p2.invMass * dl * g2;
            p3.predicted += p3.invMass * dl * g3;

            lambda += dl;
        }

        void SolveDistance(DistanceConstraint c, float h) {
            var p0 = particles[c.i0];
            var p1 = particles[c.i1];

            var d = p1.predicted - p0.predicted;
            var len = d.magnitude;
            if (len < Mathf.Epsilon) return;

            var n = d / len;
            var C = len - c.restLength;

            var w = p0.invMass + p1.invMass;
            var alphaTilde = c.compliance / Mathf.Max(Mathf.Epsilon, h * h);
            var dl = (-C - alphaTilde * c.lambda) / Mathf.Max(Mathf.Epsilon, w + alphaTilde);

            p0.predicted -= p0.invMass * dl * n;
            p1.predicted += p1.invMass * dl * n;
            c.lambda += dl;
        }

        void SolveSelfCollisions() {
            for (int i = 0; i < particles.Count; i++) {
                for (int j = i + 1; j < particles.Count; j++) {
                    if (neighborPairs.Contains(PairKey(i, j)))
                        continue;

                    var p0 = particles[i];
                    var p1 = particles[j];

                    var d = p1.predicted - p0.predicted;
                    var len = d.magnitude;
                    var target = p0.radius + p1.radius;

                    if (len < Mathf.Epsilon || len >= target)
                        continue;

                    var w = p0.invMass + p1.invMass;
                    if (w <= 0f) continue;

                    var n = d / len;
                    var penetration = target - len;
                    var corr = n * (penetration / w);

                    p0.predicted -= corr * p0.invMass;
                    p1.predicted += corr * p1.invMass;
                }
            }
        }

        void SolveFloorPositions() {
            for (int i = 0; i < particles.Count; i++) {
                var p = particles[i];
                var minY = floorY + p.radius;
                if (p.predicted.y < minY)
                    p.predicted.y = minY;
            }
        }

        void ApplyFloorVelocity(Particle p) {
            var minY = floorY + p.radius;
            if (p.position.y > minY + Mathf.Epsilon) return;

            p.position.y = minY;

            if (p.velocity.y < 0f) p.velocity.y = -p.velocity.y * restitution;

            var tangential = Vector3.ProjectOnPlane(p.velocity, Vector3.up);
            p.velocity -= tangential * Mathf.Clamp01(friction);
        }

        void SyncVisuals() {
            for (int i = 0; i < particles.Count; i++) {
                var p = particles[i];
                if (p.go == null) continue;

                p.go.transform.position = p.position;
                p.go.transform.localScale = Vector3.one * (p.radius * 2f);

                if (i == grabbed) p.renderer.material.color = Color.yellow;
                else if (p.isFixed) p.renderer.material.color = Color.cyan;
                else if (p.isBall) p.renderer.material.color = Color.green;
                else p.renderer.material.color = Color.red;
            }
        }
    }
}
