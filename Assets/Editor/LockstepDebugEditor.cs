// ==============================================
// Author：qiuyukun
// Date:2020-12-22 17:44:31
// ==============================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace Lockstep.Editor
{
    [CustomEditor(typeof(LockstepDebug))]
    public class LockstepDebugEditor : UnityEditor.Editor
    {
        private LockstepDebug _debug;
        private FieldInfo _timeOffestField;
        private FieldInfo _excuteTimeListField;
        private FieldInfo _rollbackTimeListField;
        private MethodInfo _predictTimeMethod;
        private bool _pause;

        public void OnEnable()
        {
            _debug = (LockstepDebug)target;
            var engine = _debug.engine;
            var engineType = _debug.engine.GetType();
            var debugType = _debug.GetType();
            _timeOffestField = engineType.GetField("_timeOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            _predictTimeMethod = engineType.GetMethod("GetPredictTime", BindingFlags.Instance | BindingFlags.NonPublic);
            _excuteTimeListField = debugType.GetField("_excuteTimeList", BindingFlags.Instance | BindingFlags.NonPublic);
            _rollbackTimeListField = debugType.GetField("_rollbackTimeList", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            var engine = _debug.engine;
            var engineType = engine.GetType();
            var timeOffset = (long)_timeOffestField.GetValue(engine);
            EditorGUILayout.LabelField("时间校准 : ", engine.timeAdjust.ToString());
            EditorGUILayout.LabelField("已确认帧 : ", engine.confirmedFrameIndex.ToString());
            EditorGUILayout.LabelField("预测数量 : ", (engine.predictiveFrameIndex - engine.confirmedFrameIndex).ToString());
            EditorGUILayout.LabelField("时间差 : ", timeOffset.ToString());
            EditorGUILayout.LabelField("落后确认帧 : ", ((long)_predictTimeMethod.Invoke(engine, null) / engine.frameDeltaTime - engine.confirmedFrameIndex).ToString());

            #region Performance
            DrawPerformance(_excuteTimeListField.GetValue(_debug) as IList);
            DrawPerformance(_rollbackTimeListField.GetValue(_debug) as IList);
            #endregion

            #region Button
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(_pause ? "Resume" : "Pause"))
            {
                _pause = !_pause;
                if (_pause)
                {
                    engine.Pause();
                }
                else
                {
                    engine.Resume();
                }
            }
            if (_pause)
            {
                if (GUILayout.Button("Next Step"))
                {
                    engine.NextStep();
                }
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            Repaint();
        }

        private void DrawPerformance(IList dataList)
        {
            if (dataList.Count == 0) return;
            var max = 0.0f;
            var avg = 0;
            for (int i = 0; i < dataList.Count; ++i)
            {
                var value = (int)dataList[i];
                if (value > max)
                    max = value;
                avg += value;
            }

            avg /= dataList.Count;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box("Avg " + (int)avg);
            GUILayout.Box("Max " + (int)max);
            GUILayout.Box("Count " + dataList.Count);
            EditorGUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(150));
            var showCount = dataList.Count > 15 ? 15 : dataList.Count;
            var last = dataList.Count - showCount;
            var width = rect.width / showCount;
            var height = rect.height - 20;
            var pos = new Vector2(rect.x, rect.y);

            for (int i = 0; i < dataList.Count && showCount > 0; i++, showCount--)
            {
                var data = (int)dataList[i + last];
                var dataHeight = (1 - data / max) * height;
                var start = new Vector2(i * width, height) + pos;
                var middle = new Vector2(i * width + width / 2, dataHeight) + pos;
                var end = new Vector2(i * width + width, height) + pos;
                Handles.DrawLine(start, middle);
                Handles.DrawLine(middle, end);

                var labalPos = middle;
                labalPos.y -= 10;
                labalPos.x -= 5;
                Handles.Label(labalPos, data.ToString(), EditorStyles.boldLabel);
            }
            Handles.EndGUI();
        }
    }
}

