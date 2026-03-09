using Leopotam.Ecs;

using UnityEngine;

class SpringVisualiser : MonoBehaviour {
    private EcsEntity entity;

    void Update() {
        ref var spring = ref entity.Get<Spring>();

        var pos1 = spring.joint1.Get<Position>().r;
        var pos2 = spring.joint2.Get<Position>().r;

        transform.localScale = new Vector3(1, (pos1 - pos2).magnitude, 1);
        transform.up = pos1 - pos2;
        transform.position = 0.5f * (pos1 + pos2);
    }

    public SpringVisualiser SetEntity(EcsEntity entity) {
        this.entity = entity;
        return this;
    }

    public SpringVisualiser SetColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
        return this;
    }
}
