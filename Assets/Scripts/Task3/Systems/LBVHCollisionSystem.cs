using Leopotam.Ecs;

using UnityEngine;

using System.Collections.Generic;
using System;

namespace Task3 {
    public class LBVHCollisionSystem : IEcsRunSystem {
        private EcsFilter<RigidBody, Transform> filter = null;
        private EcsWorld world = null;

        private struct LBVHNode {
            public int entityIndex;
            public uint mortonCode;
            public float minX, maxX;
            public float minY, maxY;
            public float minZ, maxZ;
        }

        private List<LBVHNode> nodes = new();

        public void Run() {
            nodes.Clear();
            var count = filter.GetEntitiesCount();

            var sceneMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var sceneMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < count; i++) {
                ref var tr = ref filter.Get2(i);
                sceneMin = Vector3.Min(sceneMin, tr.position);
                sceneMax = Vector3.Max(sceneMax, tr.position);
            }

            var sceneSize = sceneMax - sceneMin;
            if (sceneSize.x == 0) sceneSize.x = 0.01f;
            if (sceneSize.y == 0) sceneSize.y = 0.01f;
            if (sceneSize.z == 0) sceneSize.z = 0.01f;

            for (int i = 0; i < count; i++) {
                ref var rb = ref filter.Get1(i);
                ref var tr = ref filter.Get2(i);

                var radius = rb.size.magnitude * 0.5f;

                var pos = tr.position;
                var morton = ExpandBits(
                    (uint)((pos.x - sceneMin.x) / sceneSize.x * 1023.0f)) |
                    (ExpandBits((uint)((pos.y - sceneMin.y) / sceneSize.y * 1023.0f)) << 1) |
                    (ExpandBits((uint)((pos.z - sceneMin.z) / sceneSize.z * 1023.0f)) << 2
                );

                nodes.Add(new LBVHNode {
                    entityIndex = i,
                    mortonCode = morton,
                    minX = pos.x - radius, maxX = pos.x + radius,
                    minY = pos.y - radius, maxY = pos.y + radius,
                    minZ = pos.z - radius, maxZ = pos.z + radius
                });
            }

            nodes.Sort((a, b) => a.mortonCode.CompareTo(b.mortonCode));

            var searchWindow = Mathf.Min(30, nodes.Count);
            var checkedPairs = new HashSet<ulong>();

            for (int i = 0; i < nodes.Count; i++) {
                var nodeA = nodes[i];

                for (int j = i + 1; j < Mathf.Min(i + searchWindow, nodes.Count); j++) {
                    var nodeB = nodes[j];

                    if (nodeA.minX <= nodeB.maxX && nodeA.maxX >= nodeB.minX &&
                        nodeA.minY <= nodeB.maxY && nodeA.maxY >= nodeB.minY &&
                        nodeA.minZ <= nodeB.maxZ && nodeA.maxZ >= nodeB.minZ) {

                        var idA = nodeA.entityIndex;
                        var idB = nodeB.entityIndex;

                        var minId = Math.Min(idA, idB);
                        var maxId = Math.Max(idA, idB);
                        var pairId = (ulong)minId << 32 | (uint)maxId;

                        if (!checkedPairs.Add(pairId)) continue;

                        ref var rbA = ref filter.Get1(minId);
                        ref var rbB = ref filter.Get1(maxId);

                        if (rbA.isStatic && rbB.isStatic) continue;

                        ref var trA = ref filter.Get2(minId);
                        ref var trB = ref filter.Get2(maxId);

                        if (BoxCollision3DUtils.TestBoxBox(trA, rbA, trB, rbB, out var manifold)) {
                            manifold.bodyA = filter.GetEntity(minId);
                            manifold.bodyB = filter.GetEntity(maxId);
                            var ent = world.NewEntity();
                            ent.Get<ContactInfo>() = manifold;
                        }
                    }
                }
            }
        }

        private uint ExpandBits(uint v) {
            v = (v * 0x00010001u) & 0xFF0000FFu;
            v = (v * 0x00000101u) & 0x0F00F00Fu;
            v = (v * 0x00000011u) & 0xC30C30C3u;
            v = (v * 0x00000005u) & 0x49249249u;
            return v;
        }
    }
}
