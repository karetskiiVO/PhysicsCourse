using System;
using System.Linq;

using UnityEngine;

// public class Task1 : MonoBehaviour {
//     [Serializable]
//     public struct SpringParams {
//         public float length;
//         public float relaxedLength;
//         public float k;
//     }

//     private class Point {
//         public Vector2 position = new();
//         public Vector2 velocity = new();
//         public float revMass = new();

//         public float energy => 0.5f * velocity.sqrMagnitude / revMass;
//     }

//     private class Spring {
//         public float relaxedLength;
//         public float k;
//         public Point begin, end;

//         public float energy => 0.5f * k * (end.position - begin.position).sqrMagnitude;
//         public Vector2 ForceAt(Vector2 point) {
//             return k * (begin.position + end.position - 2 * point);
//         }
//     }

//     public void Create() {
//         var (l1, l2) = (spring1Param.length, spring2Param.length);
//         var (sinmu, cosmu, tanmu) = (Mathf.Sin(mu), Mathf.Cos(mu), Mathf.Tan(mu));
//         points = new Point[1];
//         springs = new Spring[2];
//         points[0] = new() {
//             position = new(
//                 (l2 * cosmu + l1) / tanmu + l2 * sinmu,
//                 l1
//             ),
//             revMass = 1f / mass,
//         };
//         var spring1Conn = new Point() {
//             position = new(
//                 (l2 * cosmu + l1) / tanmu + l2 * sinmu,
//                 0
//             )
//         };
//         var spring2Conn = new Point() {
//             position = new(
//                 (l2 * cosmu + l1) / tanmu,
//                 l2 * cosmu + l1
//             )
//         };
//         springs[0] = new() {
//             k = spring1Param.k,
//             begin = spring1Conn,
//             end = point,
//             relaxedLength = spring1Param.relaxedLength,
//         };
//         springs[1] = new() {
//             k = spring2Param.k,
//             begin = spring2Conn,
//             end = point,
//             relaxedLength = spring2Param.relaxedLength,
//         };
//     }

//     public float energy => points.Sum(point => point.energy) + springs.Sum(spring => spring.energy);

//     private Spring[] springs;
//     private Point[] points;

//     public SpringParams spring1Param, spring2Param;
//     public float mass;
//     public float mu;
// }
