using Leopotam.Ecs;

using UnityEngine;

using XCharts.Runtime;

public class Simulation : MonoBehaviour {
    [SerializeField]
    LineChart energyChart;
    [SerializeField]
    GameObject pointPrefab, springPrefab;

    EcsWorld world;
    EcsSystems systems;

    void Start() {
        Init();

        systems?.Init();
    }

    void FixedUpdate() {
        systems?.Run();
    }

    public void Init() {
        world = new();
        systems = new(world);

        systems
            .Add(new ImplicitEulerIntegratorSystem(() => Time.fixedDeltaTime))
            .Add(new EnergyBeholderSystem(energyChart));

        Create(
            world,
            new SpringParams() {
                length = 3,
                relaxedLength = 2.5f,
                k = 1,
            },
            new SpringParams() {
                length = 4,
                relaxedLength = 2f,
                k = 0.5f,
            },
            2,
            120
        );
    }

    public struct SpringParams {
        public float length;
        public float relaxedLength;
        public float k;
    }

    void Create(
        EcsWorld world,
        SpringParams spring1Param,
        SpringParams spring2Param,
        float mass,
        float mu
    ) {
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

    void OnDestroy() {
        if (systems != null) {
            systems.Destroy();
            systems = null;
            world.Destroy();
            world = null;
        }
    }
}
