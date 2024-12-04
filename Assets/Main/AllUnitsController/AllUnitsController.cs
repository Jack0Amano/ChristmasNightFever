using StageObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Units
{
    /// <summary>
    /// Unitの設置や除去、Unit同士のアクションを管理する
    public class AllUnitsController : MonoBehaviour
    {
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
        public List<UnitController> EnemyUnitControllers { get; private set; }

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

            IEnumerator _SpawnUnit(StageObjectsController stageObjectsController)
            {
                // Enemy Unitの設置をStageObjectControllerのEditWay index=0から取得
                // Player Unitの設置をStageObjectControllerのチェックポイントから取得
                var spawnCorutines = stageObjectsController.EnemySpawnPoints.ConvertAll(spawnPoint =>
                {
                    var enemyID = enemyUnitIDList[Random.Range(0, enemyUnitIDList.Count)];
                    return StartCoroutine(SpawnUnit(spawnPoint, enemyID, UnitType.Enemy));
                });
                spawnCorutines.Add(StartCoroutine(SpawnUnit(stageObjectsController.PlayerSpawnPoint.transform, playerUnitID, UnitType.Enemy)));

                // すべてのUnitの設置が終わるまで待つ
                foreach (var corutine in spawnCorutines)
                {
                    yield return corutine;
                }

                gameManager.OnUnitsLoaded();
            }
            StartCoroutine(_SpawnUnit(stageObjectsController));
        }

        /// <summary>
        /// UnitのIDをAddressableから読み込み、transformの位置に設置する
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        IEnumerator SpawnUnit(Transform transform, string id, UnitType unitType)
        {
            var handle = Addressables.InstantiateAsync(id, transform.position, Quaternion.identity);
            asyncOperationHandles.Add(handle);
            yield return handle;
            var gameobject = handle.Result;
            gameobject.transform.SetParent(transform);
            if (unitType == UnitType.Player)
            {
                PlayerUnitController = gameobject.GetComponent<UnitController>();
            }
            else
            {
                EnemyUnitControllers.Add(gameobject.GetComponent<UnitController>());
            }
        }

        /// <summary>
        /// Scene上に設置されているすべてのUnitを除去する
        /// </summary>
        public void RemoveAllUnits()
        {
            foreach (var handle in asyncOperationHandles)
            {
                Addressables.ReleaseInstance(handle);
            }
            asyncOperationHandles.Clear();
            PlayerUnitController = null;
            EnemyUnitControllers.Clear();
        }

    }
}