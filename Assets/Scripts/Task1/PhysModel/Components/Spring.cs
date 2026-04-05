using Leopotam.Ecs;

namespace Task1 {
    public struct Spring {
        public float k;
        public float relaxedLength;

        public EcsEntity joint1, joint2;
    }
}
