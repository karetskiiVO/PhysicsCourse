using System;

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
        var begin = spring.joint1.Get<Position>().r;
        var end = spring.joint2.Get<Position>().r;

        return -spring.k * (end - begin);
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
            points.Get1(pointIndex).a = Vector2.zero;
        }
    }

    public abstract void Run();
}

public class ExplicitEulerIntegratorSystem : BaseIntegratorSystem {
    public ExplicitEulerIntegratorSystem(Func<float> DeltaTime) : base(DeltaTime) { }

    public override void Run() {
        var dt = DeltaTime();

        ApplySpringForces();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);

            position.r += point.v * dt;
            point.v += point.a * dt;
        }

        DropAccelerations();
    }
}

public class ImplicitEulerIntegratorSystem : BaseIntegratorSystem {
    public ImplicitEulerIntegratorSystem(Func<float> DeltaTime) : base(DeltaTime) { }

    public override void Run() {
        var dt = DeltaTime();

        ApplySpringForces();

        foreach (var pointIndex in points) {
            ref var point = ref points.Get1(pointIndex);
            ref var position = ref points.Get2(pointIndex);

            point.v += point.a * dt;
            position.r += point.v * dt;
        }

        DropAccelerations();
    }
}

