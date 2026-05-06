using Leopotam.Ecs;

namespace Task3 {
    public class TransformSyncSystem : IEcsRunSystem {
        EcsFilter<RigidBody, Transform, TransformRef> rigidBodies = null;

        public void Run() {
            foreach (var idx in rigidBodies) {
                ref var tr = ref rigidBodies.Get2(idx);
                ref var trRef = ref rigidBodies.Get3(idx);

                trRef.transform.SetPositionAndRotation(tr.position, tr.rotation);
            }
        }
    }
}
