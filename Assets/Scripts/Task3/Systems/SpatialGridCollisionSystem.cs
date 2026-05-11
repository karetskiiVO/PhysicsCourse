using Leopotam.Ecs;

using UnityEngine;

using System.Collections.Generic;

namespace Task3 {
    public class SpatialGridCollisionSystem : IEcsRunSystem {
        private EcsFilter<RigidBody, Transform> filter = null;
        private EcsWorld world = null;

        private Dictionary<long, List<int>> grid = new();
        private Stack<List<int>> listPool = new();
        private HashSet<ulong> checkedPairs = new();

        private float cellSize = 1.5f;

        private const int CellOffset = 10000;

        public void Run() {
            foreach (var list in grid.Values) {
                list.Clear();
                listPool.Push(list);
            }
            grid.Clear();
            checkedPairs.Clear();

            int count = filter.GetEntitiesCount();

            for (int i = 0; i < count; i++) {
                ref var rb = ref filter.Get1(i);
                ref var tr = ref filter.Get2(i);

                float radius = rb.size.magnitude * 0.5f;

                int minX = Mathf.FloorToInt((tr.position.x - radius) / cellSize);
                int minY = Mathf.FloorToInt((tr.position.y - radius) / cellSize);
                int minZ = Mathf.FloorToInt((tr.position.z - radius) / cellSize);
                int maxX = Mathf.FloorToInt((tr.position.x + radius) / cellSize);
                int maxY = Mathf.FloorToInt((tr.position.y + radius) / cellSize);
                int maxZ = Mathf.FloorToInt((tr.position.z + radius) / cellSize);

                for (int x = minX; x <= maxX; x++) {
                    for (int y = minY; y <= maxY; y++) {
                        for (int z = minZ; z <= maxZ; z++) {
                            long cellKey = PackCell(x, y, z);
                            if (!grid.TryGetValue(cellKey, out var list)) {
                                list = listPool.Count > 0 ? listPool.Pop() : new List<int>();
                                grid[cellKey] = list;
                            }
                            list.Add(i);
                        }
                    }
                }
            }

            foreach (var cellList in grid.Values) {
                if (cellList.Count < 2)
                    continue;

                for (int n = 0; n < cellList.Count; n++) {
                    int i = cellList[n];
                    for (int m = n + 1; m < cellList.Count; m++) {
                        int j = cellList[m];

                        int minId = i;
                        int maxId = j;
                        if (i > j) {
                            minId = j;
                            maxId = i;
                        }
                        ulong pairId = (ulong)minId << 32 | (uint)maxId;

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

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static long PackCell(int x, int y, int z) =>
            (long)(x + CellOffset) | ((long)(y + CellOffset) << 20) | ((long)(z + CellOffset) << 40);
    }
}
