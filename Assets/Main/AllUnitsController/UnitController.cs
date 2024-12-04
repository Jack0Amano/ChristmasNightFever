using System;
using System.Collections;
using System.Collections.Generic;
using Units.AI;
using Units.TPS;
using UnityEngine;
using static Utility;

namespace Units
{
    /// <summary>
    /// UnitのPrefabにコンポーネントとしてつけてUnitの挙動を管理する
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Tooltip("Unitの種類を設定する")]
        [SerializeField] UnitType unitType;
        [SerializeField] CameraUserController cameraUserController;

        /// <summary>
        /// TPS操作を司り、userの入力を受け付ける    
        /// UnitControllerからはCameraUserControllerの要求と、AIでの自動移動の際に使われる
        /// </summary>
        public ThirdPersonUserControl TPSController { get; private set; }

        /// <summary>
        /// このUnitが敵の場合、そのAIを設定する
        /// </summary>
        EnemyAI enemyAI;

        private void Awake()
        {
            TPSController = GetComponent<ThirdPersonUserControl>();
            if (unitType == UnitType.Enemy)
            {
                enemyAI = GetComponent<EnemyAI>();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Print(cameraUserController, TPSController);
            if (unitType == UnitType.Player)
            {
                // CamerauserControllerにPlayerをの周辺をFollowCameraするように要求する
                StartCoroutine(cameraUserController.ChangeModeFollowTarget(TPSController));
            }
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
