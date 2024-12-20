using StageObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Utility;

namespace Units
{
    /// <summary>
    /// Unitの設置や除去、Unit同士のアクションを管理する
    public class AllUnitsController : MonoBehaviour
    {

        [SerializeField] CameraUserController cameraUserController;
        [Tooltip("PlayerのAddressable IDを設定する")]
        [SerializeField] private string playerUnitID;
        [Tooltip("EnemyのAddressable IDを設定する")]
        [SerializeField] private List<string> enemyUnitIDList;

        List<AsyncOperationHandle<GameObject>> asyncOperationHandles = new List<AsyncOperationHandle<GameObject>>();

        StageObjectsController stageObjectsController;

        /// <summary>
        /// PlayerがScene上に設置されている場合このUnitController
        /// </summary>
        public UnitController PlayerUnitController { get; private set; }
        /// <summary>
        /// Scene上に設置されているすべてのEnemyのUnitController
        /// </summary>
        public List<UnitController> EnemyUnitControllers { get; private set; }　= new List<UnitController>();

        /// <summary>
        /// Unitの読み込みと設置が終わった際に呼び出すAction
        /// </summary>
        public Action onUnitsLoadedAction;

        /// <summary>
        /// ゲームが終了した際に呼び出すHandler
        /// </summary>
        public EventHandler<OnGameResultEventArgs> onGameResultEventHandler;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

#if UNITY_EDITOR
            // Debug用にEnemyのAIを一時停止させる
            if (UserController.DebugKeyDown)
            {
                if (EnemyUnitControllers[0].EnemyAI.IsPause)
                {
                    EnemyUnitControllers.ForEach(enemy => enemy.EnemyAI.UnpauseAI());
                }
                else
                {
                    EnemyUnitControllers.ForEach(enemy => enemy.EnemyAI.PauseAI());
                }
            }
#endif
        }


        /// <summary>
        /// ステージの読み込みがGameManagerによって終わった際に呼び出し   
        /// Unitの設置や初期化を行う
        /// </summary>
        public void OnStageLoaded(StageObjectsController stageObjectsController)
        {
            stageObjectsController.OnGoalReached += (sender, e) => StartCoroutine(OnGoalReached(sender, e));

            IEnumerator SpawnUnit(StageObjectsController stageObjectsController)
            {
                this.stageObjectsController = stageObjectsController;
                // Enemy Unitの設置をStageObjectControllerのEditWay index=0から取得
                // Player Unitの設置をStageObjectControllerのチェックポイントから取得
                var spawnCorutines = stageObjectsController.EnemyWays.ConvertAll(way =>
                {
                    var enemyID = enemyUnitIDList[UnityEngine.Random.Range(0, enemyUnitIDList.Count)];
                    return StartCoroutine(this.SpawnUnit(way.pointAndStops[0].pointTransform, enemyID, UnitType.Enemy, way.pointsParent.name, way.pointAndStops));
                });
                spawnCorutines.Add(StartCoroutine(this.SpawnUnit(stageObjectsController.PlayerSpawnPoint.transform, playerUnitID, UnitType.Player, "Player")));

                // すべてのUnitの設置が終わるまで待つ
                foreach (var corutine in spawnCorutines)
                {
                    yield return corutine;
                }

                EnemyUnitControllers.ForEach(enemy =>
                {
                    enemy.EnemyAI.StopAI();
                    enemy.PlayerUnitController = PlayerUnitController;
                });

                OnUnitsLoaded();
            }
            StartCoroutine(SpawnUnit(stageObjectsController));
        }

        /// <summary>
        /// UnitのIDをAddressableから読み込み、transformの位置に設置する
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="way">UnityがEnemyの際には移動経路の設定を渡す</param>
        /// <returns></returns>
        IEnumerator SpawnUnit(Transform spawnPoint, string id, UnitType unitType, string name, List<PointAndStopTime> way=null)
        {
            var handle = Addressables.InstantiateAsync(id, spawnPoint.position, spawnPoint.rotation);
            asyncOperationHandles.Add(handle);
            yield return handle;
            var gameobject = handle.Result;
            gameobject.transform.SetParent(this.transform);
            gameobject.name = name;
            var unitController = gameobject.GetComponent<UnitController>();
            unitController.TPSController.IsTPSControllActive = false;
            if (unitType == UnitType.Player)
            {
                PlayerUnitController = unitController;
            }
            else
            {
                EnemyUnitControllers.Add(unitController);
                unitController.SetUnitAsEnemy(way);
            }
            unitController.OnUnitAction += OnUnitAction;
        }

        /// <summary>
        /// Unitの読み込みが終わった際に呼び出し
        /// </summary>
        private void OnUnitsLoaded()
        {
            if (PlayerUnitController == null)
            {
                PrintError("There are any player in units.");
                return;
            }

            onUnitsLoadedAction?.Invoke();
        }

        /// <summary>
        /// MainMessagesの表示が終わった際に呼び出し ここから自由に操作可能になる
        /// </summary>
        public void OnMessagePanelClosed()
        {
            IEnumerator _OnDelayActions()
            {
                StartCoroutine(cameraUserController.SetAsFollowTarget(PlayerUnitController.TPSController));
                yield return new WaitForSeconds(1f);
                // CameraUserControllerにPlayerのTPSConを渡して、これにカメラをFollowさせる
                PlayerUnitController.TPSController.IsTPSControllActive = true;
                PlayerUnitController.StartGame();
                EnemyUnitControllers.ForEach(enemy => enemy.StartGame());
            }
            StartCoroutine(_OnDelayActions());
        }


        /// <summary>
        /// Scene上に設置されているすべてのUnitを除去する
        /// </summary>
        public void RemoveAllUnits()
        {
            PlayerUnitController.OnUnitAction -= OnUnitAction;
            EnemyUnitControllers.ForEach(enemy => enemy.OnUnitAction -= OnUnitAction);

            foreach (var handle in asyncOperationHandles)
            {
                Addressables.ReleaseInstance(handle);
            }
            asyncOperationHandles.Clear();
            PlayerUnitController = null;
            EnemyUnitControllers.Clear();
        }

        /// <summary>
        /// すべてのUnitの行動を一時停止する
        /// </summary>
        public void PauseAllUnits()
        {
            if (PlayerUnitController.IsPaused)
            {
                PrintWarning("All units are already paused.");
                return;
            }

            PlayerUnitController.PauseUnit();
            EnemyUnitControllers.ForEach(enemy => enemy.PauseUnit());
            cameraUserController.enableCameraControlling = false;
        }

        /// <summary>
        /// すべてのUnitの行動を再開する
        /// </summary>
        public void UnpauseAllUnits()
        {
            if (!PlayerUnitController.IsPaused)
            {
                PrintWarning("All units are already unpaused.");
                return;
            }

            PlayerUnitController.UnpauseUnit();
            EnemyUnitControllers.ForEach(enemy => enemy.UnpauseUnit());
            cameraUserController.enableCameraControlling = true;
        }

        #region Actions
        /// <summary>
        /// キャラクタ-もしくはStageControllerObject送られてくるUnitActionEventArgsを受け取り、判断し、Unitに反映させる
        /// </summary>
        private void OnUnitAction(object sender, UnitActionEventArgs e)
        {
            if (e.Action == UnitAction.FindYou)
            {
                /// Enemyがプレイヤーを見つけた場合、すべてのEnemyにFindYouを渡し、PlayerはDieを呼び出す
                EnemyUnitControllers.ForEach(enemy => 
                {
                    if (enemy != e.ActionFrom)
                        enemy.FoundYou(e.ActionFrom);
                });
                StartCoroutine(PlayerUnitController.Killed());
            }
            else if (e.Action == UnitAction.Die)
            {
                /// UnitがDieした場合、Playerの場合はゲームオーバーとする
                var unitController = sender as UnitController;
                if (unitController.unitType == UnitType.Player)
                {
                    onGameResultEventHandler?.Invoke(this, new OnGameResultEventArgs(GameResultType.Lose));
                    RemoveAllUnits();
                }
            }
            else if (e.Action == UnitAction.MakeNoize)
            {
                /// 物音を立てた場合、直線距離で最も近いEnemyに物音を感知させる
                float minDistance = float.MaxValue;
                UnitController nearEnemy = null;
                foreach (var enemy in EnemyUnitControllers)
                {
                    var distance = Vector3.Distance(enemy.transform.position, e.ActionFrom.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearEnemy = enemy;
                    }
                }
                nearEnemy?.SenseNoize(e.ActionFrom);
            }
            
        }

        /// <summary>
        /// ゴールに到達した際にStageObjectControllerから呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private IEnumerator OnGoalReached(object sender, UnitActionEventArgs e)
        {
            if (e.Action == UnitAction.Goal)
            {
                var stageObjCon = sender as StageObjectsController;
                // すべてのEnemyの追跡や発見度の加算を止める
                EnemyUnitControllers.ForEach(enemy => enemy.FinishToGameAsWin(cameraUserController, stageObjCon.winVirtualCamera));
                // Playerの移動を止める
                PlayerUnitController.FinishToGameAsWin(cameraUserController, stageObjCon.winVirtualCamera);

                yield return new WaitForSeconds(4.0f);
                onGameResultEventHandler?.Invoke(this, new OnGameResultEventArgs(GameResultType.Win));
                RemoveAllUnits();
            }
        }
        #endregion

    }
}