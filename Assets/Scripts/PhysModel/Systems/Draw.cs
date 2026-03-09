using Leopotam.Ecs;

using UnityEngine;

public class DrawSystem : IEcsRunSystem {
    EcsFilter<MaterialPoint, Position> points = null;
    EcsFilter<Spring> springs = null;

    public void Run() {

        foreach (var i in points) {
            ref var pos = ref points.Get2(i); // компонент Position
            Vector2 p = pos.r;

            // Небольшой крестик в позиции точки
            Debug.DrawLine(new Vector3(p.x - 0.1f, p.y, 0), new Vector3(p.x + 0.1f, p.y, 0), Color.red);
            Debug.DrawLine(new Vector3(p.x, p.y - 0.1f, 0), new Vector3(p.x, p.y + 0.1f, 0), Color.red);
        }

        // Рисуем все пружины (линиями)
        foreach (var i in springs) {
            ref var spring = ref springs.Get1(i); // компонент Spring

            // Проверяем, что обе сущности существуют и имеют позицию
            if (spring.joint1.IsAlive() && spring.joint2.IsAlive() &&
                spring.joint1.Has<Position>() && spring.joint2.Has<Position>()) {
                Vector2 p1 = spring.joint1.Get<Position>().r;
                Vector2 p2 = spring.joint2.Get<Position>().r;

                Debug.DrawLine(new Vector3(p1.x, p1.y, 0), new Vector3(p2.x, p2.y, 0), Color.green);
            }
        }
    }
}
