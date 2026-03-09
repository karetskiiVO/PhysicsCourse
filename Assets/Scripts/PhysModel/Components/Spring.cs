using Leopotam.Ecs;

public struct Spring {
    public float k;
    public float relaxedLength;

    public EcsEntity joint1, joint2;
}
