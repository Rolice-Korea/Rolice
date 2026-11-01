using UnityEngine;

namespace Rolice
{
    /// <summary>
    /// RcDiceDirectionType 보조 함수. 방향 ↔ 벡터 변환, 반대면/축 계산.
    /// </summary>
    public static class RcDiceDirectionTypeExt
    {
        public const int Count = (int)RcDiceDirectionType.Max;

        private static readonly Vector3[] Vectors =
        {
            Vector3.right, Vector3.left,   // PlusX, MinusX
            Vector3.up, Vector3.down,      // PlusY, MinusY
            Vector3.forward, Vector3.back, // PlusZ, MinusZ
        };

        /// <summary>마주보는 면 방향.</summary>
        public static RcDiceDirectionType Opposite(this RcDiceDirectionType dir) => (RcDiceDirectionType)((int)dir ^ 1);

        /// <summary>축 인덱스 (0=X, 1=Y, 2=Z).</summary>
        public static int Axis(this RcDiceDirectionType dir) => (int)dir >> 1;

        public static Vector3 ToVector(this RcDiceDirectionType dir) => Vectors[(int)dir];

        /// <summary>노멀과 가장 가까운 방향. 축 정렬된 노멀 기준.</summary>
        public static RcDiceDirectionType FromNormal(Vector3 normal)
        {
            var best = RcDiceDirectionType.PlusY;
            float bestDot = float.MinValue;
            for (int i = 0; i < Count; i++)
            {
                float dot = Vector3.Dot(normal, Vectors[i]);
                if (dot <= bestDot) continue;
                bestDot = dot;
                best = (RcDiceDirectionType)i;
            }
            return best;
        }
    }
}
