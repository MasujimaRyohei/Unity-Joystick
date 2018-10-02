using UnityEngine;

namespace VirtualInput
{
    /// <summary>
    /// Unit vector defined class
    /// </summary>
    public struct Direction2DNormalVector
    {
        public static readonly Vector2 Zero = Vector2.zero;
        public static readonly Vector2 Up = Vector2.up;
        public static readonly Vector2 RightUp = (Vector2.right + Vector2.up).normalized;
        public static readonly Vector2 Right = Vector2.right;
        public static readonly Vector2 RightDown = (Vector2.right + Vector2.down).normalized;
        public static readonly Vector2 Down = Vector2.down;
        public static readonly Vector2 LeftDown = (Vector2.left + Vector2.down).normalized;
        public static readonly Vector2 Left = Vector2.left;
        public static readonly Vector2 LeftUp = (Vector2.left + Vector2.up).normalized;
    }
}