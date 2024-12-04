using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageObjects
{
    /// <summary>
    /// ステージの各種チェックポイント(Goal, Event, etc.)を設定するためのコンポーネント   
    /// このコンポーネントをつけてstageObjectにアタッチすることで、ステージのチェックポイントとして機能する
    /// </summary>

    public class CheckPoint : MonoBehaviour
    {
        [Tooltip("チェックポイントの種類を設定する")]
        [SerializeField] internal CheckPointType checkPointType;

        [Tooltip("チェックポイントのIDを設定する イベントなどで使われる")]
        [SerializeField] internal int checkPointID;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
#endif
    }

    /// <summary>
    /// チェックポイントがどの様な動きをするものか
    /// </summary>
    [SerializeField] internal enum CheckPointType
    {
        Goal,
        Event,
        PlayerSpawn,
    }
}