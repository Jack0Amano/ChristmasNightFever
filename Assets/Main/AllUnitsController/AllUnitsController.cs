using StageObjects;
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

        /// <summary>
        /// PlayerがScene上に設置されている場合このUnitController
        /// </summary>
        public UnitController PlayerUnitController { get; private set; }
        /// <summary>
        /// Scene上に設置されているすべてのEnemyのUnitController
        /// </summary>
        public List<UnitController> EnemyUnitControllers { get; private set; }　= new List<UnitController>();

        GameManager gameManager;

        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// ステージの読み込みがGameManagerによって終わった際に呼び出し   
        /// Unitの設置や初期化を行う
        /// </summary>
        public void OnStageLoaded(StageObjectsController stageObjectsController)
        {
            stageObjectsController.OnGoalReached += (sender, e) => StartCoroutine(OnGoalReached(sender, e));

            IEnumerator _SpawnUnit(StageObjectsController stageObjectsController)
            {
                // Enemy Unitの設置をStageObjectControllerのEditWay index=0から取得
                // Player Unitの設置をStageObjectControllerのチェックポイントから取得
                var spawnCorutines = stageObjectsController.EnemyWays.ConvertAll(way =>
                {
                    var enemyID = enemyUnitIDList[Random.Range(0, enemyUnitIDList.Count)];
                    return StartCoroutine(SpawnUnit(way.pointAndStops[0].pointTransform, enemyID, UnitType.Enemy, way.pointAndStops));
                });
                spawnCorutines.Add(StartCoroutine(SpawnUnit(stageObjectsController.PlayerSpawnPoint.transform, playerUnitID, UnitType.Player)));

                // すべてのUnitの設置が終わるまで待つ
                foreach (var corutine in spawnCorutines)
                {
                    yield return corutine;
                }

                EnemyUnitControllers.ForEach(enemy => enemy.PlayerUnitController = PlayerUnitController);

                OnUnitsLoaded();
            }
            StartCoroutine(_SpawnUnit(stageObjectsController));
        }

        /// <summary>
        /// UnitのIDをAddressableから読み込み、transformの位置に設置する
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="way">UnityがEnemyの際には移動経路の設定を渡す</param>
        /// <returns></returns>
        IEnumerator SpawnUnit(Transform spawnPoint, string id, UnitType unitType, List<PointAndStopTime> way=null)
        {
            var handle = Addressables.InstantiateAsync(id, spawnPoint.position, spawnPoint.rotation);
            asyncOperationHandles.Add(handle);
            yield return handle;
            var gameobject = handle.Result;
            gameobject.transform.SetParent(this.transform);
            var unitController = gameobject.GetComponent<UnitController>();
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

            gameManager.OnUnitsLoaded();
            // CameraUserControllerにPlayerのTPSConを渡して、これにカメラをFollowさせる
            StartCoroutine(cameraUserController.ChangeModeFollowTarget(PlayerUnitController.TPSController));

            PlayerUnitController.StartToGame();
            EnemyUnitControllers.ForEach(enemy => enemy.StartToGame());
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
                        enemy.FoundYou();
                });
                StartCoroutine(PlayerUnitController.Killed());
            }
            else if (e.Action == UnitAction.Die)
            {
                /// UnitがDieした場合、Playerの場合はゲームオーバーとする
                var unitController = sender as UnitController;
                if (unitController.unitType == UnitType.Player)
                {
                    gameManager.OnGameResult(false);
                    RemoveAllUnits();
                }
            }
            else if (e.Action == UnitAction.MakeNoize)
            {
                /// 物音を立てた場合、すべてのEnemyにSenseNoizeを渡す Enemy側で物音を感知するかどうか判断する
                EnemyUnitControllers.ForEach(enemy => enemy.SenseNoize(PlayerUnitController));
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
                yield return new WaitForSeconds(3.0f);
                gameManager.OnGameResult(true);
                RemoveAllUnits();
            }
        }

    }
}