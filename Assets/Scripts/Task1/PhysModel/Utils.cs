using System;

using UnityEngine;

namespace Task1 {
    public static class MathUtils {
        public static Vector4 SimpsonsRule(Func<float, Vector4> f, float a, float b, int n) {
            if (n % 2 != 0) throw new ArgumentException("n должно быть чётным для метода Симпсона.");

            float h = (b - a) / n;
            Vector4 sum = f(a) + f(b);

            for (int i = 1; i < n; i++) {
                float x = a + i * h;
                sum += (i % 2 == 0) ? 2 * f(x) : 4 * f(x);
            }

            return sum * h / 3;
        }
    }

    public static class Matrix4x4Extensions {
        public static Matrix4x4 MatrixExp(this Matrix4x4 m, int terms = 30) {
            Matrix4x4 result = Matrix4x4.identity;
            Matrix4x4 term = Matrix4x4.identity;

            for (int k = 1; k <= terms; k++) {
                term = term * m;
                term = term.MultiplyScalar(1f / k);
                result = result.Add(term);
            }

            return result;
        }

        public static Matrix4x4 MultiplyScalar(this Matrix4x4 m, float scalar) {
            Matrix4x4 result = new Matrix4x4();
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    result[i, j] = m[i, j] * scalar;
                }
            }
            return result;
        }

        public static Matrix4x4 Add(this Matrix4x4 a, Matrix4x4 b) {
            Matrix4x4 result = new Matrix4x4();
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return result;
        }
    }
}
