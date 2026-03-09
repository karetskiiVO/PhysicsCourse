using Leopotam.Ecs;

using UnityEngine;

class PointVisualiser : MonoBehaviour {
    private EcsEntity entity;
    private LineRenderer lineRenderer = null;

    private int lastIdx = 0;
    void FixedUpdate() {
        if (lineRenderer != null) {
            if (lineRenderer.positionCount == 0) lineRenderer.positionCount = 1;
            if (lineRenderer.positionCount == lastIdx) lineRenderer.positionCount *= 2;

            lineRenderer.SetPosition(lastIdx, transform.position);
            lastIdx++;
        }
    }

    void Update() {
        ref var position = ref entity.Get<Position>();
        transform.position = position.r;
    }

    public PointVisualiser SetEntity(EcsEntity entity) {
        this.entity = entity;
        return this;
    }

    public PointVisualiser SetColor(Color color) {
        GetComponent<SpriteRenderer>().color = color;
        return this;
    }

    public PointVisualiser AddTrack(Color color) {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.loop = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;
        return this;
    }
}
