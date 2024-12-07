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
    /// Unit�̐ݒu�⏜���AUnit���m�̃A�N�V�������Ǘ�����
    public class AllUnitsController : MonoBehaviour
    {

        [SerializeField] CameraUserController cameraUserController;
        [Tooltip("Player��Addressable ID��ݒ肷��")]
        [SerializeField] private string playerUnitID;
        [Tooltip("Enemy��Addressable ID��ݒ肷��")]
        [SerializeField] private List<string> enemyUnitIDList;

        List<AsyncOperationHandle<GameObject>> asyncOperationHandles = new List<AsyncOperationHandle<GameObject>>();

        StageObjectsController stageObjectsController;

        /// <summary>
        /// Player��Scene��ɐݒu����Ă���ꍇ����UnitController
        /// </summary>
        public UnitController PlayerUnitController { get; private set; }
        /// <summary>
        /// Scene��ɐݒu����Ă��邷�ׂĂ�Enemy��UnitController
        /// </summary>
        public List<UnitController> EnemyUnitControllers { get; private set; }�@= new List<UnitController>();

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
            stageObjectsController.OnGoalReached += (sender, e) => StartCoroutine(OnGoalReached(sender, e));

            IEnumerator _SpawnUnit(StageObjectsController stageObjectsController)
            {
                this.stageObjectsController = stageObjectsController;
                // Enemy Unit�̐ݒu��StageObjectController��EditWay index=0����擾
                // Player Unit�̐ݒu��StageObjectController�̃`�F�b�N�|�C���g����擾
                var spawnCorutines = stageObjectsController.EnemyWays.ConvertAll(way =>
                {
                    var enemyID = enemyUnitIDList[Random.Range(0, enemyUnitIDList.Count)];
                    return StartCoroutine(SpawnUnit(way.pointAndStops[0].pointTransform, enemyID, UnitType.Enemy, way.pointsParent.name, way.pointAndStops));
                });
                spawnCorutines.Add(StartCoroutine(SpawnUnit(stageObjectsController.PlayerSpawnPoint.transform, playerUnitID, UnitType.Player, "Player")));

                // ���ׂĂ�Unit�̐ݒu���I���܂ő҂�
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
            StartCoroutine(_SpawnUnit(stageObjectsController));
        }

        /// <summary>
        /// Unit��ID��Addressable����ǂݍ��݁Atransform�̈ʒu�ɐݒu����
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="way">Unity��Enemy�̍ۂɂ͈ړ��o�H�̐ݒ��n��</param>
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
        /// Unit�̓ǂݍ��݂��I������ۂɌĂяo��
        /// </summary>
        private void OnUnitsLoaded()
        {
            if (PlayerUnitController == null)
            {
                PrintError("There are any player in units.");
                return;
            }

            gameManager.OnUnitsLoaded();
        }

        /// <summary>
        /// MainMessages�̕\�����I������ۂɌĂяo�� �������玩�R�ɑ���\�ɂȂ�
        /// </summary>
        public void OnMessagePanelClosed()
        {
            IEnumerator _OnDelayActions()
            {

               yield return new WaitForSeconds(0.5f);
                // CameraUserController��Player��TPSCon��n���āA����ɃJ������Follow������
                PlayerUnitController.TPSController.followCamera.Priority = 1;
                PlayerUnitController.TPSController.IsTPSControllActive = true;
                StartCoroutine(cameraUserController.ChangeModeFollowTarget(PlayerUnitController.TPSController));

                PlayerUnitController.StartToGame();
                EnemyUnitControllers.ForEach(enemy => enemy.StartToGame());
            }
            StartCoroutine(_OnDelayActions());

        }


        /// <summary>
        /// Scene��ɐݒu����Ă��邷�ׂĂ�Unit����������
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
        /// �L�����N�^-��������StageControllerObject�����Ă���UnitActionEventArgs���󂯎��A���f���AUnit�ɔ��f������
        /// </summary>
        private void OnUnitAction(object sender, UnitActionEventArgs e)
        {
            if (e.Action == UnitAction.FindYou)
            {
                /// Enemy���v���C���[���������ꍇ�A���ׂĂ�Enemy��FindYou��n���APlayer��Die���Ăяo��
                EnemyUnitControllers.ForEach(enemy => 
                {
                    if (enemy != e.ActionFrom)
                        enemy.FoundYou(e.ActionFrom);
                });
                StartCoroutine(PlayerUnitController.Killed());
            }
            else if (e.Action == UnitAction.Die)
            {
                /// Unit��Die�����ꍇ�APlayer�̏ꍇ�̓Q�[���I�[�o�[�Ƃ���
                var unitController = sender as UnitController;
                if (unitController.unitType == UnitType.Player)
                {
                    gameManager.OnGameResult(false);
                    RemoveAllUnits();
                }
            }
            else if (e.Action == UnitAction.MakeNoize)
            {
                /// �����𗧂Ă��ꍇ�A���������ōł��߂�Enemy�ɕ��������m������
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
        /// �S�[���ɓ��B�����ۂ�StageObjectController����Ăяo�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private IEnumerator OnGoalReached(object sender, UnitActionEventArgs e)
        {
            if (e.Action == UnitAction.Goal)
            {
                var stageObjCon = sender as StageObjectsController;
                // ���ׂĂ�Enemy�̒ǐՂ┭���x�̉��Z���~�߂�
                EnemyUnitControllers.ForEach(enemy => enemy.FinishToGameAsWin(cameraUserController, stageObjCon.winVirtualCamera));
                // Player�̈ړ����~�߂�
                PlayerUnitController.FinishToGameAsWin(cameraUserController, stageObjCon.winVirtualCamera);

                yield return new WaitForSeconds(4.0f);
                gameManager.OnGameResult(true);
                RemoveAllUnits();
            }
        }

    }
}