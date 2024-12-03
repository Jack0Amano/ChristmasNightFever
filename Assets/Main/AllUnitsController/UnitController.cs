using System;
using System.Collections;
using System.Collections.Generic;
using Units.AI;
using UnityEngine;

namespace Units
{
    /// <summary>
    /// UnitのPrefabにコンポーネントとしてつけてUnitの挙動を管理する
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Tooltip("Unitの種類を設定する")]
        [SerializeField] UnitType unitType;  

        /// <summary>
        /// このUnitが敵の場合、そのAIを設定する
        /// </summary>
        EnemyAI enemyAI;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    /// <summary>
    /// Unitの種類
    /// </summary>
    [Serializable] public enum UnitType
    {
        Player,
        Enemy
    }
}
