using Leopotam.Ecs;

using UnityEngine;

namespace Task3 {
    public class RigidBodyCreateSystem : IEcsInitSystem {
        EcsFilter<RigidBody, Transform, TransformRef> rigidBodies = null;

        public void Init() {
            foreach (var idx in rigidBodies) {
                ref var tr = ref rigidBodies.Get2(idx);
                ref var trRef = ref rigidBodies.Get3(idx);

                trRef.transform = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                trRef.transform.SetPositionAndRotation(tr.position, tr.rotation);
                trRef.transform.localScale = tr.scale;
            }
        }
    }
}
