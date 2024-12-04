using StageObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Units
{
    /// <summary>
    /// Unit�̐ݒu�⏜���AUnit���m�̃A�N�V�������Ǘ�����
    public class AllUnitsController : MonoBehaviour
    {
        [Tooltip("Player��Addressable ID��ݒ肷��")]
        [SerializeField] private string playerUnitID;
        [Tooltip("Enemy��Addressable ID��ݒ肷��")]
        [SerializeField] private List<string> enemyUnitIDList;

        List<AsyncOperationHandle<GameObject>> asyncOperationHandles = new List<AsyncOperationHandle<GameObject>>();

        /// <summary>
        /// Player��Scene��ɐݒu����Ă���ꍇ����UnitController
        /// </summary>
        public UnitController PlayerUnitController { get; private set; }
        /// <summary>
        /// Scene��ɐݒu����Ă��邷�ׂĂ�Enemy��UnitController
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
        /// �X�e�[�W�̓ǂݍ��݂�GameManager�ɂ���ďI������ۂɌĂяo��   
        /// Unit�̐ݒu�⏉�������s��
        /// </summary>
        public void OnStageLoaded(StageObjectsController stageObjectsController)
        {

            IEnumerator _SpawnUnit(StageObjectsController stageObjectsController)
            {
                // Enemy Unit�̐ݒu��StageObjectController��EditWay index=0����擾
                // Player Unit�̐ݒu��StageObjectController�̃`�F�b�N�|�C���g����擾
                var spawnCorutines = stageObjectsController.EnemySpawnPoints.ConvertAll(spawnPoint =>
                {
                    var enemyID = enemyUnitIDList[Random.Range(0, enemyUnitIDList.Count)];
                    return StartCoroutine(SpawnUnit(spawnPoint, enemyID, UnitType.Enemy));
                });
                spawnCorutines.Add(StartCoroutine(SpawnUnit(stageObjectsController.PlayerSpawnPoint.transform, playerUnitID, UnitType.Enemy)));

                // ���ׂĂ�Unit�̐ݒu���I���܂ő҂�
                foreach (var corutine in spawnCorutines)
                {
                    yield return corutine;
                }

                gameManager.OnUnitsLoaded();
            }
            StartCoroutine(_SpawnUnit(stageObjectsController));
        }

        /// <summary>
        /// Unit��ID��Addressable����ǂݍ��݁Atransform�̈ʒu�ɐݒu����
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
        /// Scene��ɐݒu����Ă��邷�ׂĂ�Unit����������
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