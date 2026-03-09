using System;
using System.Collections.Generic;

using Leopotam.Ecs;

using UnityEngine;

public abstract class BaseIntegratorSystem : IEcsRunSystem {
    protected EcsFilter<MaterialPoint, Position> points = null;
    protected EcsFilter<Spring> springs = null;

    protected Func<float> DeltaTime;

    protected BaseIntegratorSystem(Func<float> DeltaTime) {
        this.DeltaTime = DeltaTime;
    }

    static protected Vector2 SpringForce(ref Spring spring) {
        var pos1 = spring.joint1.Get<Position>().r;
        var pos2 = spring.joint2.Get<Position>().r;
        var delta = pos2 - pos1;
        float distance = delta.magnitude;

        if (distance < Mathf.Epsilon) return Vector2.zero;

        float stretch = distance - spring.relaxedLength;
        return -spring.k * stretch * (delta / distance);
    }

    static protected void ApplyForce(EcsEntity entity, Vector2 force) {
        if (!entity.Has<MaterialPoint>()) return;

        ref var materialPoint = ref entity.Get<MaterialPoint>();
        materialPoint.a += force / materialPoint.m;
    }

    protected void ApplySpringForces() {
        foreach (var springIndex in springs) {
            ref var spring = ref springs.Get1(springIndex);

            var force = SpringForce(ref spring);
            ApplyForce(spring.joint1, -force);
            ApplyForce(spring.joint2, force);
        }
    }

    protected void DropAccelerations() {
        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            point.a = Vector2.zero;
        }
    }

    public abstract void Run();
}

public class ExplicitEulerIntegratorSystem : BaseIntegratorSystem {
    public ExplicitEulerIntegratorSystem(Func<float> DeltaTime) : base(DeltaTime) { }

    public override void Run() {
        var dt = DeltaTime();

        DropAccelerations();
        ApplySpringForces();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);

            position.r += point.v * dt;
            point.v += point.a * dt;
        }
    }
}

public class SymplecticEulerIntegratorSystem : BaseIntegratorSystem {
    public SymplecticEulerIntegratorSystem(Func<float> DeltaTime) : base(DeltaTime) { }

    public override void Run() {
        var dt = DeltaTime();

        DropAccelerations();
        ApplySpringForces();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);

            point.v += point.a * dt;
            position.r += point.v * dt;
        }
    }
}

public class VelocityVerletIntegratorSystem : BaseIntegratorSystem {
    public VelocityVerletIntegratorSystem(Func<float> DeltaTime) : base(DeltaTime) { }

    public override void Run() {
        float dt = DeltaTime();

        DropAccelerations();
        ApplySpringForces();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);

            position.r += point.v * dt + 0.5f * point.a * dt * dt;
            point.v += 0.5f * dt * point.a;
        }

        DropAccelerations();
        ApplySpringForces();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            point.v += 0.5f * dt * point.a;
        }
    }
}

public class ImplicitEulerIntegratorSystem : BaseIntegratorSystem {
    private readonly int iterations;

    public ImplicitEulerIntegratorSystem(Func<float> DeltaTime, int iterations = 8) : base(DeltaTime) {
        this.iterations = Mathf.Max(1, iterations);
    }

    public override void Run() {
        float dt = DeltaTime();

        DropAccelerations();

        var pointEntities = new List<EcsEntity>();
        var oldPositions = new List<Vector2>();
        var oldVelocities = new List<Vector2>();
        var predictedPositions = new List<Vector2>();
        var predictedVelocities = new List<Vector2>();

        foreach (var pointIndex in points) {
            var entity = points.GetEntity(pointIndex);
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);

            pointEntities.Add(entity);
            oldPositions.Add(position.r);
            oldVelocities.Add(point.v);

            predictedPositions.Add(position.r);
            predictedVelocities.Add(point.v);
        }

        DropAccelerations();
        ApplySpringForces();

        for (int i = 0; i < pointEntities.Count; i++) {
            var entity = pointEntities[i];
            ref var point = ref entity.Get<MaterialPoint>();

            predictedVelocities[i] = oldVelocities[i] + dt * point.a;
            predictedPositions[i] = oldPositions[i] + dt * predictedVelocities[i];
        }

        for (int iter = 0; iter < iterations; iter++) {
            for (int i = 0; i < pointEntities.Count; i++) {
                var entity = pointEntities[i];
                ref var point = ref entity.Get<MaterialPoint>();
                ref var position = ref entity.Get<Position>();

                position.r = predictedPositions[i];
                point.v = predictedVelocities[i];
            }

            DropAccelerations();
            ApplySpringForces();

            for (int i = 0; i < pointEntities.Count; i++) {
                var entity = pointEntities[i];
                ref var point = ref entity.Get<MaterialPoint>();

                predictedVelocities[i] = oldVelocities[i] + dt * point.a;
                predictedPositions[i] = oldPositions[i] + dt * predictedVelocities[i];
            }
        }

        for (int i = 0; i < pointEntities.Count; i++) {
            var entity = pointEntities[i];
            ref var point = ref entity.Get<MaterialPoint>();
            ref var position = ref entity.Get<Position>();

            position.r = predictedPositions[i];
            point.v = predictedVelocities[i];
        }
    }
}

public class TheoreticalSolverIntegratorSystem : BaseIntegratorSystem {
    new EcsFilter<TheoreticalPoint, Position, MaterialPoint> points = null;
    float time = 0;

    public TheoreticalSolverIntegratorSystem(Func<float> DeltaTime) : base(DeltaTime) { }

    public override void Run() {
        time += DeltaTime();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);
            ref var materialPoint = ref points.Get3(pointIndex);

            (position.r, materialPoint.v) = point.solution(time);
        }
    }
}
