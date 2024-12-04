using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace StageObjects
{
    /// <summary>
    /// WayをEditor表示するための
    /// </summary>
    public class EditorWays : MonoBehaviour
    {

        [SerializeField] internal List<EditorWaysPointParent> ways = new List<EditorWaysPointParent>();
        [Tooltip("GridPointの番号表示のGUIテキスト")]
        [SerializeField] private GUIStyle passGuiStyle;


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (ways == null) return;

            foreach(var w in ways)
            {
                if (w == null)
                    continue;
                if (w.pointsParent == null)
                    continue;
                if (!w.enable)
                    continue;

                Gizmos.color = w.color;
                passGuiStyle.normal.textColor = w.color;

                var count = 0;
                Transform old = null;
                foreach (Transform pass in w.pointsParent.transform)
                {
                    if (old != null)
                    {
                        Gizmos.DrawLine(old.position, pass.position);
                        Handles.Label(old.position, count.ToString(), passGuiStyle);
                        Gizmos.DrawLine(old.position, old.position + (old.forward * 0.3f));
                    }

                    old = pass;
                    count++;
                }

                if (old != null)
                {
                    Gizmos.DrawLine(old.position, old.position + (old.forward * 0.3f));
                    Handles.Label(old.position, count.ToString(), passGuiStyle);
                }
            }
        }
#endif
    }

    /// <summary>
    /// 経路の各ポイントを入れた親Objectとその設定を紐づけるためのもの
    /// </summary>
    [Serializable]
    class EditorWaysPointParent
    {
        [SerializeField] internal bool enable = true;
        [Tooltip("wayのポイントを入れたObject")]
        [SerializeField] internal Transform pointsParent;
        [SerializeField] internal Color color = Color.green;
        [Tooltip("一時停止地点とその秒数の設定")]
        [SerializeField] internal List<EditorWayStopPoint> stopPoints = new List<EditorWayStopPoint>();
    }

    /// <summary>
    /// EditorWayPointに一時停止地点のIndexを紐づけるためのもの
    /// </summary>
    [Serializable]
    class EditorWayStopPoint
    {
        [Tooltip("一時停止地点のindex")]
        [SerializeField] internal int waysIndex;
        [Tooltip("一時停止を行う時間 マイナスならずっと停止")]
        [SerializeField] internal float stopTime;
    }
}