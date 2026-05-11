using Leopotam.Ecs;

using UnityEngine;

using System.Collections.Generic;
using System;

namespace Task3 {
    public class LBVHCollisionSystem : IEcsRunSystem {
        private EcsFilter<RigidBody, Transform> filter = null;
        private EcsWorld world = null;

        private struct Leaf {
            public int entityIndex;
            public uint mortonCode;
            public Vector3 min, max;
        }

        private struct TreeNode {
            public Vector3 min, max;
            public int leftChild, rightChild;
            public int entityIndex;
            public bool IsLeaf => leftChild == -1 && rightChild == -1;
        }

        private List<Leaf> leaves = new();
        private TreeNode[] nodes = new TreeNode[0];
        private int nodeCount = 0;

        public void Run() {
            leaves.Clear();
            var count = filter.GetEntitiesCount();
            if (count == 0) return;

            var sceneMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var sceneMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < count; i++) {
                ref var tr = ref filter.Get2(i);
                sceneMin = Vector3.Min(sceneMin, tr.position);
                sceneMax = Vector3.Max(sceneMax, tr.position);
            }

            var sceneSize = sceneMax - sceneMin;
            if (sceneSize.x <= 0f) sceneSize.x = 0.01f;
            if (sceneSize.y <= 0f) sceneSize.y = 0.01f;
            if (sceneSize.z <= 0f) sceneSize.z = 0.01f;

            for (int i = 0; i < count; i++) {
                ref var rb = ref filter.Get1(i);
                ref var tr = ref filter.Get2(i);

                var radius = rb.size.magnitude * 0.5f;

                var pos = tr.position;
                var min = new Vector3(pos.x - radius, pos.y - radius, pos.z - radius);
                var max = new Vector3(pos.x + radius, pos.y + radius, pos.z + radius);

                var morton = ExpandBits(
                    (uint)((pos.x - sceneMin.x) / sceneSize.x * 1023.0f)) |
                    (ExpandBits((uint)((pos.y - sceneMin.y) / sceneSize.y * 1023.0f)) << 1) |
                    (ExpandBits((uint)((pos.z - sceneMin.z) / sceneSize.z * 1023.0f)) << 2
                );

                leaves.Add(new Leaf {
                    entityIndex = i,
                    mortonCode = morton,
                    min = min,
                    max = max
                });
            }

            leaves.Sort((a, b) => a.mortonCode.CompareTo(b.mortonCode));

            if (nodes.Length < count * 2) {
                nodes = new TreeNode[count * 2];
            }
            nodeCount = 0;

            var root = BuildTree(0, leaves.Count - 1);

            var checkedPairs = new HashSet<ulong>();
            var stack = new int[64];

            for (int i = 0; i < leaves.Count; i++) {
                var leaf = leaves[i];
                var idA = leaf.entityIndex;
                var minA = leaf.min;
                var maxA = leaf.max;

                var stackPtr = 0;
                stack[stackPtr++] = root;

                while (stackPtr > 0) {
                    var current = stack[--stackPtr];
                    ref var node = ref nodes[current];

                    if (minA.x > node.max.x || maxA.x < node.min.x ||
                        minA.y > node.max.y || maxA.y < node.min.y ||
                        minA.z > node.max.z || maxA.z < node.min.z) {
                        continue;
                    }

                    if (node.IsLeaf) {
                        var idB = node.entityIndex;
                        if (idB > idA) {
                            var pairId = (ulong)idA << 32 | (uint)idB;
                            if (checkedPairs.Add(pairId)) {
                                ref var rbA = ref filter.Get1(idA);
                                ref var rbB = ref filter.Get1(idB);

                                if (!rbA.isStatic || !rbB.isStatic) {
                                    ref var trA = ref filter.Get2(idA);
                                    ref var trB = ref filter.Get2(idB);

                                    if (BoxCollision3DUtils.TestBoxBox(trA, rbA, trB, rbB, out var manifold)) {
                                        manifold.bodyA = filter.GetEntity(idA);
                                        manifold.bodyB = filter.GetEntity(idB);
                                        var ent = world.NewEntity();
                                        ent.Get<ContactInfo>() = manifold;
                                    }
                                }
                            }
                        }
                    } else {
                        stack[stackPtr++] = node.leftChild;
                        stack[stackPtr++] = node.rightChild;
                    }
                }
            }
        }

        private int AllocateNode(Vector3 min, Vector3 max, int left, int right, int entityIndex) {
            var index = nodeCount++;
            nodes[index] = new TreeNode {
                min = min, max = max,
                leftChild = left, rightChild = right,
                entityIndex = entityIndex
            };
            return index;
        }

        private int BuildTree(int first, int last) {
            if (first == last) {
                var leaf = leaves[first];
                return AllocateNode(leaf.min, leaf.max, -1, -1, leaf.entityIndex);
            }

            var split = FindSplit(first, last);

            var left = BuildTree(first, split);
            var right = BuildTree(split + 1, last);

            var min = Vector3.Min(nodes[left].min, nodes[right].min);
            var max = Vector3.Max(nodes[left].max, nodes[right].max);

            return AllocateNode(min, max, left, right, -1);
        }

        private int FindSplit(int first, int last) {
            var firstCode = leaves[first].mortonCode;
            var lastCode = leaves[last].mortonCode;

            if (firstCode == lastCode) return (first + last) >> 1;

            var commonPrefix = CountLeadingZeros(firstCode ^ lastCode);
            var split = first;
            var step = last - first;

            do {
                step = (step + 1) >> 1;
                var newSplit = split + step;

                if (newSplit < last) {
                    var splitCode = leaves[newSplit].mortonCode;
                    var splitPrefix = CountLeadingZeros(firstCode ^ splitCode);
                    if (splitPrefix > commonPrefix) split = newSplit;
                }
            } while (step > 1);

            return split;
        }

        private int CountLeadingZeros(uint x) {
            int n = 0;
            if (x == 0) return 32;
            if ((x & 0xFFFF0000) == 0) { n += 16; x <<= 16; }
            if ((x & 0xFF000000) == 0) { n += 8; x <<= 8; }
            if ((x & 0xF0000000) == 0) { n += 4; x <<= 4; }
            if ((x & 0xC0000000) == 0) { n += 2; x <<= 2; }
            if ((x & 0x80000000) == 0) { n += 1; }
            return n;
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
