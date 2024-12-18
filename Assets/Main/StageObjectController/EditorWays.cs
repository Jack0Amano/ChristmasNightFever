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

        private void Awake()
        {
            // waysのPointAndStopTimeを自動補完する (一時停止ポイントに指定された場所以外通過ポイントで埋める)
            foreach (var w in ways)
            {
                if (w == null)
                    continue;
                if (w.pointsParent == null)
                    continue;
                if (!w.enable)
                    continue;
                var count = 0;
                foreach (Transform pass in w.pointsParent.transform)
                {
                    var index = w.pointAndStops.FindIndex(p => p.index == count);
                    if (index == -1)
                    {
                        w.pointAndStops.Add(new PointAndStopTime (count, 0, pass));
                    }
                    else
                    {
                        w.pointAndStops[index].pointTransform = pass;
                    }
                    count++;
                }
                w.pointAndStops.Sort((a, b) => a.index - b.index);
                // Inspectorでpointsよりも多い数のpointAndStopがある場合は削除 Inspectorで誤入力している
                w.pointAndStops.Slice(0, count);
            }
        }


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
    /// 経路の各ポイントを入れた親Objectとその設定を紐づけるためのもの  また経路をたどるUnitのAIに渡される
    /// </summary>
    [Serializable]
    public class EditorWaysPointParent
    {
        [SerializeField] internal bool enable = true;
        [Tooltip("wayのポイントを入れたObject")]
        [SerializeField] internal Transform pointsParent;
        [SerializeField] internal Color color = Color.green;
        [Tooltip("一時停止地点とその秒数の設定 一時停止地点のみ書き込めば良い ただの通過地点なら実行時自動補完する")]
        [SerializeField] public List<PointAndStopTime> pointAndStops = new List<PointAndStopTime>();
    }

    /// <summary>
    /// EditorWayPointに一時停止地点のIndexを紐づけるためのもの
    /// </summary>
    [Serializable]
    public class PointAndStopTime
    {
        [Tooltip("一時停止地点のindex")]
        [SerializeField] public int index;
        [Tooltip("一時停止を行う時間 マイナスならずっと停止")]
        [SerializeField] public float stopTime;
        [Tooltip("移動先にこの地点を指定されたときに走るか")]
        [SerializeField] public bool runToThisPoint = false;
        [Tooltip("通過もしくは一時停止地点のTransform ランタイムで自動補完される")]
        public Transform pointTransform;

        internal PointAndStopTime(int waysIndex, float stopTime, Transform pointTransform)
        {
            this.index = waysIndex;
            this.stopTime = stopTime;
            this.pointTransform = pointTransform;

        }
    }
}