using UnityEngine;
using UnityEditor;

namespace VirtualInput
{
    /// <summary>
    /// Joystick editor.
    /// </summary>
    [CustomEditor(typeof(Joystick))]
    public class JoystickEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            Joystick joystick = target as Joystick;
            Undo.RecordObject(joystick, "Undo property");

            if (joystick == null) return;

            EditorGUILayout.LabelField("No." + joystick.ID);

            joystick.DirectionMode = (EDirectionMode)EditorGUILayout.EnumPopup("方向モード", joystick.DirectionMode);

            joystick.MovableRadius = EditorGUILayout.FloatField("Movable radius", joystick.MovableRadius);


            // 指を離した時にスティックが中心に戻るか
            joystick.IsResetPosition = EditorGUILayout.Toggle("Reset stick position", joystick.IsResetPosition);

            // 現在地を表示
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(
                "Position(-1~1)   X : " +
                joystick.Position.x.ToString("F2") + ",  Y : " +
                joystick.Position.y.ToString("F2")
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("デバッグ用");
            EditorGUI.indentLevel++;

            joystick.CurrentInputMode = (EInputMode)EditorGUILayout.EnumPopup("入力モード", joystick.CurrentInputMode);

            EditorGUI.indentLevel--;
        }
    }
}