using UnityEngine;

namespace Task3 {
    public class RigidBody : MonoBehaviour {
        void Awake() {
            if (physicsSystem == null) {
                physicsSystem = new GameObject("physics system").AddComponent<PhysicsSystem>();
            }

            physicsSystem.RegisterRigidBody(this);
        }


        static PhysicsSystem physicsSystem = null;
    }
}
