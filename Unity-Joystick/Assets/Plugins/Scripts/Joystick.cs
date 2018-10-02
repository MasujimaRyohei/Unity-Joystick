using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VirtualInput
{
    /// <summary>
    /// Virtual joystick.
    /// </summary>
    public class Joystick : Graphic, IPointerDownHandler, IPointerUpHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private static List<Joystick> joysticks = new List<Joystick>();
        public ReadOnlyCollection<Joystick> Joysticks
        {
            get { return new ReadOnlyCollection<Joystick>(joysticks); }
        }
        private int id;
        public int ID
        {
            get { return id; }
        }

        [SerializeField]
        private EDirectionMode directionMode;
        public EDirectionMode DirectionMode
        {
            get { return directionMode; }
            set { directionMode = value; }
        }

        private Vector3 direction;
        private const string StickName = "Stick";

        [SerializeField]
        private EInputMode currentInputMode;
        public EInputMode CurrentInputMode
        {
            get { return currentInputMode; }
            set { currentInputMode = value; }
        }

        // Movable joystick part.
        [SerializeField]
        [Header("Movable joystick patr.")]
        private GameObject stick;

        [SerializeField]
        [Range(1,100)]
        private float movableRadius;
        public float MovableRadius
        {
            get { return movableRadius; }
            set { movableRadius = value; }
        }

        private float movableRadiusForStickPosition;

        [SerializeField]
        [Header("When release finger, reset stick position.")]
        private bool isResetPosition = true;
        public bool IsResetPosition
        {
            get { return isResetPosition; }
            set { isResetPosition = value; }
        }

        [SerializeField]
        private Vector2 currentPosition = Vector2.zero;
        public Vector2 Position { get { return currentPosition; } }
        public Vector2 AxisRaw
        {
            get
            {
                switch (directionMode)
                {
                    case EDirectionMode.Direction8:
                        return Get8DirectionAxis();
                    case EDirectionMode.Direction4:
                        return Get4DirectionAxis();
                    case EDirectionMode.DirectionAll:
                    default:
                        return currentPosition;
                }
            }
            private set { currentPosition = value; }
        }

        private Vector3 StickPosition
        {
            set
            {
                stick.transform.localPosition = value;
                currentPosition = new Vector2(
                    stick.transform.localPosition.x * movableRadiusForStickPosition,
                    stick.transform.localPosition.y * movableRadiusForStickPosition
                );
            }
        }
        private bool onJoystickStay = false;

        // Delegates
        public delegate void JoystickEvent(Vector2 axisRaw);
        public JoystickEvent onJoystickUp = delegate { };
        public JoystickEvent onJoystickDown = delegate { };
        public JoystickEvent onJoystick = delegate { };

        public bool GetJoystickStay()
        {
            return onJoystickStay;
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize()
        {
            // If need create stick, do it and set position to center.
            CreateStickIfNeeded();
            StickPosition = Vector3.zero;

            // Get image of stick (if not exist, add component), and change raycastTarget to false.
            Image stickImage = stick.GetComponent<Image>();
            if (stickImage == null)
                stickImage = stick.AddComponent<Image>();
            
            stickImage.raycastTarget = false;

            raycastTarget = true;

            // Don't need color where touchable field.
            color = Color.clear;

            // Add me to joystick list.
            id = joysticks.Count;
            joysticks.Add(this);

            // For optimisation
            movableRadiusForStickPosition = 1.0f / movableRadius;
        }
        static public Joystick GetJoystick(int number)
        {
            return joysticks[number];
        }
        /// <summary>
        /// スティックの自動生成および自動参照取得
        /// Auto generate stick and auto referance.
        /// </summary>
        private void CreateStickIfNeeded()
        {
            if (stick != null) return;

            if (transform.Find(StickName) != null)
            {
                stick = transform.Find(StickName).gameObject;
                return;
            }

            // Generate stick.
            stick = new GameObject(StickName);
            stick.transform.SetParent(gameObject.transform);
            stick.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Ons the pointer down.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            onJoystickDown(AxisRaw);
            onJoystickStay = true;
            // タップした瞬間にドラッグを開始した事にする
            OnDrag(eventData);
        }

      /// <summary>
      /// Ons the pointer up.
      /// </summary>
      /// <param name="eventData">Event data.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            onJoystickUp(AxisRaw);
            onJoystickStay = false;
            // タップした終了した時にドラッグを終了した時と同じ処理をする
            OnEndDrag(eventData);
        }

/// <summary>
/// Ons the end drag.
/// </summary>
/// <param name="eventData">Event data.</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (isResetPosition)
            {
                // スティックを中心に戻す
                StickPosition = Vector3.zero;
            }
            direction = Vector2.zero;
        }

        /// <summary>
        /// Ons the drag.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnDrag(PointerEventData eventData)
        {
            // 他UIを触った時はDrag処理に入らないようにする
            if (!onJoystickStay)
            {
                return;
            }
            // タップ位置を画面内の座標に変換し、スティックを移動
            Vector2 screenPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(),
                  new Vector2(eventData.position.x, eventData.position.y),
                  null,
                  out screenPos
                );
            StickPosition = screenPos;

            // 角度計算
            direction = stick.transform.localPosition.normalized;

            FixStickPosition();
        }

        /// <summary>
        /// Fixs the stick position.
        /// </summary>
        private void FixStickPosition()
        {
            float currentRadius = (Vector3.zero - stick.transform.localPosition).sqrMagnitude;
            if (currentRadius > movableRadius * movableRadius)
            {
                StickPosition = direction * movableRadius;
            }
        }

        /// <summary>
        /// Get 4 directions axis.
        /// </summary>
        /// <returns>The direction vector.</returns>
        public Vector2 Get4DirectionAxis()
        {

            float absX = Mathf.Abs(currentPosition.x);
            float absY = Mathf.Abs(currentPosition.y);

            if (absX >= absY)
            {
                return new Vector2(currentPosition.x, 0);
            }
            else
            {
                return new Vector2(0, currentPosition.y);
            }
        }

        /// <summary>
        /// Get 4 directions.
        /// </summary>
        /// <returns>The direction.</returns>
        public Vector2 Get4Direction()
        {
            return Get4Direction().normalized;
        }

        /// <summary>
        /// Get 8 directions.
        /// </summary>
        /// <returns>The direction.</returns>
        public Vector3 Get8Direction()
        {
            // TODO 要リファクタリング
            if (currentPosition.sqrMagnitude == 0.0f)
            {
                return Direction2DNormalVector.Zero;
            }
            float radian = Mathf.Atan2(currentPosition.y, currentPosition.x);
            float degree = radian * Mathf.Rad2Deg;

            if (degree >= -22.5f && 22.5f > degree)
            {
                return Direction2DNormalVector.Right;
            }
            if (degree >= 22.5f && 67.5f > degree)
            {
                return Direction2DNormalVector.RightUp;
            }
            if (degree >= 67.5f && 112.5f > degree)
            {
                return Direction2DNormalVector.Up;
            }
            if (degree >= 112.5f && 157.5f > degree)
            {
                return Direction2DNormalVector.LeftUp;
            }
            if (degree >= -67.5f && -22.5f > degree)
            {
                return Direction2DNormalVector.RightDown;
            }
            if (degree >= -112.5f && -67.5f > degree)
            {
                return Direction2DNormalVector.Down;
            }
            if (degree >= -157.5f && -112.5f > degree)
            {
                return Direction2DNormalVector.LeftDown;
            }
            return Direction2DNormalVector.Left;
        }
 
        /// <summary>
        /// Get 8 directions enum.
        /// </summary>
        /// <returns>The direction enum.</returns>
        public EDirection2D Get8DirectionEnum()
        {
            // TODO : Need refactoring.
            if (currentPosition.sqrMagnitude == 0.0f)
            {
                return EDirection2D.Zero;
            }
            float radian = Mathf.Atan2(currentPosition.y, currentPosition.x);
            float degree = radian * Mathf.Rad2Deg;

            if (degree >= -22.5f && 22.5f > degree)
            {
                return EDirection2D.Right;
            }
            if (degree >= 22.5f && 67.5f > degree)
            {
                return EDirection2D.RightUp;
            }
            if (degree >= 67.5f && 112.5f > degree)
            {
                return EDirection2D.Up;
            }
            if (degree >= 112.5f && 157.5f > degree)
            {
                return EDirection2D.LeftUp;
            }
            if (degree >= -67.5f && -22.5f > degree)
            {
                return EDirection2D.RightDown;
            }
            if (degree >= -112.5f && -67.5f > degree)
            {
                return EDirection2D.Down;
            }
            if (degree >= -157.5f && -112.5f > degree)
            {
                return EDirection2D.LeftDown;
            }
            return EDirection2D.Left;
        }

        /// <summary>
        /// Get 8 directions axis.
        /// </summary>
        /// <returns>The direction axis.</returns>
        public Vector2 Get8DirectionAxis()
        {
            return Get8Direction() * currentPosition.sqrMagnitude;
        }


        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void FixedUpdate()
        {
            if (onJoystickStay)
            {
                onJoystick(AxisRaw);
            }

            //#if UNITY_EDITOR
            //if (CurrentInputMode != InputMode.KEYBOARD)
            //{
            //    return;
            //}
            //float x = Input.GetAxis("Horizontal");
            //float y = Input.GetAxis("Vertical");

            //StickPosition = new Vector3(x, y) * 100.0f;

            //// 移動場所が設定した半径を超えてる場合は制限内に抑える
            //FixStickPosition();
            //#endif
        }
        //#if UNITY_EDITOR
        //        protected override void OnValidate()
        //        {
        //            base.OnValidate();
        //            Initialize();
        //        }
        //        /// <summary>
        //        /// Callback to draw gizmos that are pickable and always drawn.
        //        /// </summary>
        //        private void OnDrawGizmos()
        //        {
        //            // スティックが移動できる範囲をScene上に表示
        //            Handles.color = Color.green;
        //            Handles.DrawWireDisc(transform.position, transform.forward, movableRadius * 0.5f);
        //        }



        //#endif
    }
}