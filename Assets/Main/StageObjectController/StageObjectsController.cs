using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using Units;

namespace StageObjects
{
    /// <summary>
    /// ステージとして読み込まれるPrefabの最も上流の親オブジェクトにアタッチされるコンポーネント   
    /// 主にステージ固有の情報をPrefabのInspector上で設定するためのもの
    /// </summary>
    public class StageObjectsController : MonoBehaviour
    {
        [Tooltip("CheckPointを入れたObjectの親")]
        [SerializeField] GameObject checkPointsParent;
        [Tooltip("Enemyの移動経路を示すEditorways")]
        [SerializeField] public EditorWays editorWays;

        /// <summary>
        /// Unitがゴールに到達したときにAllUnitsControllerに通知するイベント
        /// </summary>
        internal event EventHandler<Units.UnitActionEventArgs> OnGoalReached;

        /// <summary>
        /// Playerのスポーン地点 CheckPointsの中からPlayerSpawnのCheckPointを取得する
        /// </summary>
        public CheckPoint PlayerSpawnPoint { get; private set; }

        /// <summary>
        /// Enemyのスポーン地点 EditorWaysの中から経路index=0の最初のPointを取得する
        /// </summary>
        public List<Transform> EnemySpawnPoints { get; private set; }

        public CheckPoint GoalCheckPoint { get; private set; }


        /// <summary>
        /// ステージのチェックポイント Playerのスポーン地点やGoalなどの情報を持つ
        /// </summary>
        public List<CheckPoint> CheckPoints { get; private set; }

        private void Awake()
        {
            CheckPoints = new List<CheckPoint>(checkPointsParent.GetComponentsInChildren<CheckPoint>());
            PlayerSpawnPoint = CheckPoints.Find(cp => cp.checkPointType == CheckPointType.PlayerSpawn);
            EnemySpawnPoints = editorWays.ways.ConvertAll(w => w.pointsParent.GetChild(0));
            GoalCheckPoint = CheckPoints.Find(cp => cp.checkPointType == CheckPointType.Goal);
        }


        // Start is called before the first frame update
        void Start()
        {
            GoalCheckPoint.NortifyTrigger.OnTriggerEnterAction += (c =>
            {
                var unitObject = c.gameObject;
                var unitController = unitObject.GetComponent<UnitController>();
                if (unitController != null && unitController.unitType == UnitType.Player)
                    OnGoalReached?.Invoke(this, new UnitActionEventArgs(unitController, UnitAction.Goal));
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            OnGoalReached = null;
        }
    }

}