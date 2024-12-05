using System;
using System.Collections;
using System.Collections.Generic;
using Units.Icon;
using Units.TPS;
using UnityEngine;
using UnityEngine.AI;
using static Utility;

namespace Units.AI
{
    /// <summary>
    /// �G��AI�𐧌䂷��
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Tooltip("�v���C���[�����m�ł���ő勗��")]
        [SerializeField] private float findPlayerDistance = 10f;
        [Tooltip("�v���C���[�����m�ł���ő�p�x")]
        [SerializeField] private float findPlayerAngle = 45f;
        [Tooltip("�v���C���[��������ڂ̕����ƈʒu��Transform")]
        [SerializeField] private Transform eyesTransform;
        [Tooltip("������Ray�̑ΏۂƂȂ郌�C���[�}�X�N�̃��X�g")]
        [SerializeField] private LayerMask rayLayerMask;
        [Tooltip("�������邽�߂̋�����DeltaFindOutLevel�̊֌W�̃J�[�u")]
        [SerializeField] private AnimationCurve detectionDistanceCurve;
        [Tooltip("���b���Ƃ�DeltaFindOutLevel���v�Z���邩")]
        [SerializeField] private float detectUnitTick = 0.3f;
        [Tooltip("FindOutLevel���ǂꂾ���ُ�ɂȂ�����x���Ɉڍs���邩")]
        [SerializeField] private float alertLevel = 0.5f;

        [SerializeField] private float senceNoizeDistance = 15f;

        [Tooltip("��������HeadUpIcon")]
        [SerializeField] private HeadUP headUP;

        [Tooltip("Debug�p�ɖڕW�n�_�܂ł̐���`�悷�邩")]
        [SerializeField] private bool debugDrawAimWalkingLine = false;

        /// <summary>
        /// �v���C���[���ǂ̒��x�����Ă��邩 1�ɂȂ����犮�S�ɔ���
        /// </summary>
        internal float FindOutLevel { private set; get; } = 0;

        /// <summary>
        /// �v���C���[�𔭌������Ƃ���UnitController.FoundYou���Ăяo�����߂�Event
        /// </summary>
        internal event EventHandler<EventArgs> OnFoundPlayer;

        /// <summary>
        /// Player��UnitController��ݒ肷�� Player�̌��m�Ȃǂ��s�����ߓG���ɕK�v
        /// </summary>
        internal UnitController playerUnitController;

        internal ThirdPersonUserControl tpsController;

        /// <summary>
        /// �O��TryFindPlayer���Ăяo���ł���̌o�߃~���b
        /// </summary>
        float lastTryFindPlayerSec = 0;

        private NavMeshAgent navMeshAgent;
        private NavMeshObstacle navMeshObstacle;

        /// <summary>
        /// AI���ړ�����o�H EditorWays�Ŏw�肳���Unit�̐ݒu����AllUnitsController�ɂ����SetUnitAsEnemy�Őݒ肳���
        /// </summary>
        internal List<StageObjects.PointAndStopTime> way;

        /// <summary>
        /// ���݂�way��Index 0����X�^�[�g�@���������Ƃ����u�Ԃ�Index�����Z����Ă���
        /// </summary>
        private int currentWayIndex = 0;

        private EnemyAIMoveState moveState = EnemyAIMoveState.IdleMainRoutine;

        // Start is called before the first frame update
        void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
            // SetUnitAsEnemy(List<StageObjects.PointAndStopTime> way)���Ăяo����EnemyAI��ݒ肵�Ă��Ȃ��ꍇ�͉������Ȃ�
            if (playerUnitController == null)
                return;

            // FixedUpdate�ŌĂяo�����R��Corutine�����[�v�ł̌Ăяo������Object���������Ƃ��Ȃǌ㏈�����ʓ|�Ȃ���
            // FixedUpdate���x������ꍇ��Coroutine���ɕύX����
            // �����Ɗp�x�ŋr�؂肷�邵�Araycast���΂��̂�detectUnitTick�������߂��ɑ�ʂ̓G������ꍇ�͕��ׂ�������\��������
            lastTryFindPlayerSec += Time.fixedDeltaTime;
            if (lastTryFindPlayerSec > detectUnitTick)
            {
                TryFindPlayer();
                lastTryFindPlayerSec = 0;
            }
        }

        /// <summary>
        /// target�͔��������������m����
        /// </summary>
        /// <param name="target"></param>
        internal void SenceNoiseAction(UnitController target)
        {
            var dist = Vector3.Distance(target.transform.position, this.transform.position);
            if (dist < senceNoizeDistance)
            {
                // ���������m����
                moveState = EnemyAIMoveState.SenseNoize;
            }
        }

        internal void StopAI()
        {
           moveState = EnemyAIMoveState.Finish;
        }

        #region Move with AI
        // navmeshagent���g���Ĉړ�����ꍇ�́@NavMeshAgent��Component��ǉ����Ă���

        /// <summary>
        /// AI�̈ړ����J�n����
        /// </summary>
        public void NavigationAIEntryPoint()
        {

           // ������AI�̃��[�v���J�n����
            StartCoroutine(StopAndFindAtCurrentPlace_MainRoutine());
        }

        #region Type routine
        // ���̒��̃��[�`����Node�݂����ɍ쓮����SituationChecker�ŏ󋵂��ς������L�����Z�������
        // Routine�Ɩ������Ă���2�����C���̌o�H�����߂郋�[�`����
        // subRoutine��SituationChecker�ŏ󋵂��ς�������ɍs���郋�[�`��
        // (SubRoutine���ł��L�����Z���Ƒ���SubRoutine�ւ̈ڍs���s���ꍇ������)

        /// <summary>
        /// currentWayIndex�̎���way�Ɉړ�����
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator MoveToNextWay_MainRoutine(float delay=0)
        {
            moveState = EnemyAIMoveState.MoveMainRoutine;
            if (currentWayIndex + 1 >= way.Count )
            {
                currentWayIndex = -1;
            }

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            currentWayIndex++;
            var nextWay = way[currentWayIndex];
            // ������FindOutLevel���Ď����Ȃ���ړ�����������܂ő҂�

            StartCoroutine(MoveTo(nextWay.pointTransform.position));
            var startFindOutLevel = FindOutLevel;
            yield return new WaitForSeconds(0.5f);
            while(tpsController.IsAutoMoving)
            {
                // SituationChecker�ŏ󋵂��ς���Ă��Ȃ������m�F
                // result��false�Ȃ�Έړ����L�����Z���ASituationChecker���V�����󋵂Ɉړ����邽�߂̃A�j���[�V�������J�n����
                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    tpsController.CancelAutoMoving();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // �ړ��������������߈ړ����Ă������݂�way�ł̒�~���Ԃ�҂R���[�`���Ɉړ�
            StartCoroutine(StopAndFindAtCurrentPlace_MainRoutine());
        }

        /// <summary>
        /// currentWaiIndex��stopTime��ݒ肵�Ă���ꍇ�A��~���ĕӂ�̒T�����s��
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator StopAndFindAtCurrentPlace_MainRoutine(float delay=0)
        {
            moveState = EnemyAIMoveState.IdleMainRoutine;
            var currentWay = way[currentWayIndex];

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            if (currentWay.stopTime > 0)
            {
                // ��x���񂷃A�j���[�V�������Đ�����
                StartCoroutine(tpsController.Searching());
                var startTime = Time.time;
                while (Time.time - startTime < currentWay.stopTime)
                {
                    var result = SituationChecker(moveState, FindOutLevel);
                    if (!result)
                    {
                        // �󋵂��ς�������ߒT�����L�����Z��
                        // Stop find animation
                        yield break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (currentWay.stopTime < 0)
            {
                // -1�̏ꍇ�͂��̏�ŒT����������
                var lastSearchiAnimationTime = 0f;
                var nextSearchInterval =UnityEngine.Random.Range(9, 15);
                while (true)
                {
                    // �����悻10�b���Ƃ��������_����Searching���Đ�����
                    if (Time.time - lastSearchiAnimationTime > nextSearchInterval)
                    {
                        StartCoroutine(tpsController.Searching());
                        lastSearchiAnimationTime = Time.time;
                        nextSearchInterval = UnityEngine.Random.Range(5, 15);
                    }

                    var result = SituationChecker(moveState, FindOutLevel);
                    if (!result)
                    {
                        // �󋵂��ς�������ߒT�����L�����Z��
                        yield break;
                    }
                    // ������FindOutLevel���Ď����Ȃ���T������������܂ő҂�
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // �����ŒT���������������ߎ���way�Ɉړ�����
            StartCoroutine(MoveToNextWay_MainRoutine());
        }

        /// <summary>
        /// �s�R�ȏꏊ�𔭌����A���̏ꏊ�Ɍ������Ĉړ�����
        /// </summary>
        /// <param name="position"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator MoveToAlertPosition_SubRoutine(Vector3 position, float delay = 0)
        {
            moveState = EnemyAIMoveState.MoveToSearch;
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            StartCoroutine(MoveTo(position));
            var startFindOutLevel = FindOutLevel;
            yield return new WaitForSeconds(0.5f);
            while (tpsController.IsAutoMoving)
            {
                // SituationChecker�ŏ󋵂��ς���Ă��Ȃ������m�F
                // result��false�Ȃ�Έړ����L�����Z���ASituationChecker���V�����󋵂Ɉړ����邽�߂̃A�j���[�V�������J�n����
                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    tpsController.CancelAutoMoving();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // AlertPosition�ɓ��B�������ߒ�~���ĒT�����s��
            StartCoroutine(SearchAlertArea_SubRoutine());
            
        }

        /// <summary>
        /// �s�R�ȏꏊ�ɓ��B������A���̏ꏊ��T������
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator SearchAlertArea_SubRoutine(float delay = 0)
        {
            moveState = EnemyAIMoveState.SearchingIdle;
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            // ���b�ŒT�����I�����邩
            const float stopSearchTime = 8f;

            // TODO �T���A�j���[�V�������Đ�����
            // �T��������������BackSubRoutine�Ɉڍs����
            StartCoroutine(tpsController.Searching());
            float startSearchTime = Time.time;
            var startFindOutLevel = FindOutLevel;
            while (Time.time - startSearchTime < stopSearchTime)
            {
                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    // �󋵂��ς�������ߒT�����L�����Z��
                    // Stop find animation
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // �{�����Ă�����������Ȃ���������BackSubRoutine�Ɉڍs����
            StartCoroutine(BackToRoutineWay_SubRoutine());
        }

        /// <summary>
        /// CurrentWayIndex��way�ɖ߂�
        /// </summary>
        private IEnumerator BackToRoutineWay_SubRoutine(float delay=0)
        {
            moveState = EnemyAIMoveState.BackToMainRoutine;
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            var currentWay = way[currentWayIndex];
            print(currentWay.pointTransform.position);
            StartCoroutine(MoveTo(currentWay.pointTransform.position));
            var startFindOutLevel = FindOutLevel;
            yield return new WaitForSeconds(0.5f);
            while (tpsController.IsAutoMoving)
            {
                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    tpsController.CancelAutoMoving();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // ���������ł���routine�ɖ߂��Ă���
            StartCoroutine(StopAndFindAtCurrentPlace_MainRoutine());
        }

        /// <summary>
        /// ���������m�����ۂɊ��荞�܂��T�u���[�`�� MoveToAlertPosition_SubRoutine�Ɏ����ňڍs����
        /// </summary>
        /// <returns></returns>
        private IEnumerator SenceNoise_SubRoutine()
        {
            // TODO �����ŕ��������m����A�j���[�V�������Đ�����
            // ���������m����A�j���[�V�������I�������BackToMainRoutine�Ɉڍs����
            moveState = EnemyAIMoveState.MoveToSearch;
            headUP.ShowQuestion(0, true);
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(tpsController.RotateTo(playerUnitController.targetCollider.transform.position));

            StartCoroutine(MoveToAlertPosition_SubRoutine(playerUnitController.targetCollider.transform.position, 1f));
        }

        #endregion


        /// <summary>
        /// �e��̃A�j���[�V�����̍Đ����ɁA�󋵂��ς���Ă��Ȃ������m�F��
        /// �󋵂��ς���Ă�����A�j���[�V�������L�����Z�����ĐV�����A�j���[�V�����Ɉړ����邽�߂̃`�F�b�J�[
        /// </summary>
        private bool SituationChecker(EnemyAIMoveState startState, float startFindOutLevel)
        {
            // ���[�`���{�s�O�̊��荞�݂��m�F
            if (FindOutLevel == 1)
            {
                // �v���C���[�����������߂��ׂẴ��[�v���I�����Ĉ�ڎU�ɋ삯���
                StartCoroutine(MoveTo(playerUnitController.targetCollider.transform.position));
                return false;
            }
            else if (moveState == EnemyAIMoveState.Finish)
            {
                // �V�X�e������̏I������
                // False��Ԃ���SituationChecker���Ăяo�����R���[�`�����I�����A
                // �V����AI�̕ʃR���[�`�����J�n����Ȃ�
                return false;
            }
            else if (moveState == EnemyAIMoveState.SenseNoize)
            {
                // ���������m����
                StartCoroutine(SenceNoise_SubRoutine());
                return false;
            }

            // �x����Ԃɂ���
            var alert = FindOutLevel > alertLevel;
            if (alert)
            {
                var moveTo = playerUnitController.targetCollider.transform.position;
                if (startFindOutLevel > alertLevel)
                {

                    // �x����Ԃ����܂������Ă���
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            // False��Ԃ���SituationChecker���Ăяo�����R���[�`�����I�����A
                            // �V����AI�̕ʃR���[�`�����J�n����Ȃ�
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            // State���x���ɓ����Ă���̂�Idle�̏ꍇ��Idle���L�����Z������moveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveMainRoutine:
                            // State���x���ɓ����Ă���̂�MoveMain�̏ꍇ��MoveMain���L�����Z������moveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveToSearch:
                            // ���݂����Ɍ������Ă���r��
                            break;
                        case EnemyAIMoveState.SearchingIdle:
                            // ���݂����ŒT���� �T�����Ă��Ȃ��ꍇ���X�ɒ��߂�
                            break;
                        case EnemyAIMoveState.BackToMainRoutine:
                            // �A�낤�Ƃ����̂Ɍx����ԂɂȂ���
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                            
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // �x����ԂɂȂ���
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            // False��Ԃ���SituationChecker���Ăяo�����R���[�`�����I�����A
                            // �V����AI�̕ʃR���[�`�����J�n����Ȃ�
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            // �x����ԂɂȂ����̂�Idle���L�����Z������moveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveMainRoutine:
                            // �x����ԂɂȂ����̂�MoveMain���L�����Z������moveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveToSearch:
                            // ���݂����Ɍ������Ă���r��
                            break;
                        case EnemyAIMoveState.SearchingIdle:
                            // ���݂����ŒT���� �T�����Ă��Ȃ��ꍇ���X�ɒ��߂�
                            break;
                        case EnemyAIMoveState.BackToMainRoutine:
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                if (startFindOutLevel > alertLevel)
                {
                    // �x����Ԃ��甲����
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveToSearch:
                            // �x����Ԃ��甲�����̂�MoveToSearch���L�����Z������BackToMainRoutine�Ɉڍs
                            StartCoroutine(BackToRoutineWay_SubRoutine(1));
                            return false;
                        case EnemyAIMoveState.SearchingIdle:
                            break;
                        case EnemyAIMoveState.BackToMainRoutine:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // �x����Ԃł͂Ȃ����AState���ς���Ă��邩���m�F ����Finish���Ƌ����I��
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            // False��Ԃ���SituationChecker���Ăяo�����R���[�`�����I�����A
                            // �V����AI�̕ʃR���[�`�����J�n����Ȃ�
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveToSearch:
                            break;
                        case EnemyAIMoveState.SearchingIdle:
                            StartCoroutine(BackToRoutineWay_SubRoutine(2));
                            return false;
                        case EnemyAIMoveState.BackToMainRoutine:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// NavMesh���g�p����Location�Ɉړ�
        /// </summary>
        /// <param name="location"></param>
        public IEnumerator MoveTo(Vector3 location)
        {
            // NavMeshObstacle�͑���Unit�Ƃ̏Փ˂�����邽�߂Ɏg������͂Ȃ��ł���
            //navMeshObstacle.enabled = false;
            //navMeshAgent.enabled = true;

            // navMeshAgent.SetDestination(location);

            location.y = this.transform.position.y;
            print(navMeshAgent);
            yield return StartCoroutine(tpsController.AutoMove(location, navMeshAgent, debugDrawAimWalkingLine));
            tpsController.CancelAutoMoving();
            //navMeshAgent.isStopped = true;

            //navMeshAgent.enabled = false;
        }
        #endregion

        #region Methods try to find player
        /// <summary>
        /// Enemy���v���C���[�������悤�Ƃ��郋�[�v
        /// </summary>
        private void TryFindPlayer()
        {
            if (FindOutLevel <= 1 && moveState != EnemyAIMoveState.Finish)
            {
                var dist = GetDistanceIfPlayerInSight();
                // DOIT deltalevel��alterLevel�𒴂��Ă���ꍇ���ꂪ�����Ă�����Ԃł�5�balterLevel��ێ�
                FindOutLevel += GetDeltaLevelToFindOut(dist);
                FindOutLevel = Mathf.Clamp(FindOutLevel, 0, 1);
                headUP.SetFindOutLevel(FindOutLevel);
                if (FindOutLevel == 1)
                {
                   // �v���C���[��������
                    OnFoundPlayer?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// playerUnitController������������findPlayerDistance�ȓ��ł���A���E�ɓ����Ă���ꍇ�������擾
        /// </summary>
        /// <returns>���E�ɓ����Ă��Ȃ��ꍇ��-1</returns>
        private float GetDistanceIfPlayerInSight()
        {
            var targetPosition = playerUnitController.targetCollider.transform.position;
            // ������findPlayerDistance�ȏ�ŏ\�����������猩�����Ȃ�
            var dist = Vector3.Distance(targetPosition, eyesTransform.position);
            if (dist > findPlayerDistance)
            {
                return -1;
            }
            // ���E�ɓ����Ă��邩�𔻒�
            var angle = Vector3.Angle(eyesTransform.forward, targetPosition - eyesTransform.position);
            if (angle > findPlayerAngle)
            {
                return -1;
            }
            // �������ʂ��Ă���ray���΂��ăv���C���[�������邩�𔻒�
            // Ray�̑ΏۂɂȂ郌�C���[�� ��Q���� CoverObject��PlayerTarget��CoverObject�ɓ����炸��PlayerTarget�ɓ����邩�𔻒�
            // ���C���[�}�X�N��Inspector��rayLayerMask�Ŏw��
            RaycastHit hit;
            if (Physics.Raycast(eyesTransform.position, targetPosition - eyesTransform.position, out hit, findPlayerDistance, rayLayerMask))
            {
                if (hit.collider == playerUnitController.targetCollider)
                {
                    return hit.distance;
                }
            }
            
            return -1;

        }

        /// <summary>
        /// watcher����target�ւ̔������x�����擾����
        /// </summary>
        /// <param name="distance">���� �����Ă��Ȃ��Ȃ�=-1</param>
        /// <returns></returns>
        private float GetDeltaLevelToFindOut(float distance)
        {
            // ������Ԃ��牽�b�Ō�������
            const float ForgetTime = 5;

            if (distance != -1)
            {
                var x = distance / findPlayerDistance;
                //�@���E���ɑ��݂��Ă���
                return detectionDistanceCurve.Evaluate(x);
            }
            else
            {
                var forgetTick = ForgetTime / detectUnitTick;
                var forgetValueEachTick = -1 / forgetTick;
                return forgetValueEachTick;
            }
        }

        #endregion
   
        
    }

    /// <summary>
    ///�@�G��AI�����݂ǂ̗l�ȍs��������Ă��邩
    /// </summary>
    enum EnemyAIMoveState
    {
        /// <summary>
        /// ���[�v�����S�I��
        /// </summary>
        Finish,
        /// <summary>
        /// ��~��
        /// </summary>
        IdleMainRoutine,
        /// <summary>
        /// Way�ɉ����Ĉړ���
        /// </summary>
        MoveMainRoutine,
        /// <summary>
        /// �s�R�ȏꏊ�𔭌��������Ɍ������Ă���
        /// </summary>
        MoveToSearch,
        /// <summary>
        /// �s�R�ȏꏊ�ŒT����
        /// </summary>
        SearchingIdle,
        /// <summary>
        /// �s�R�ȏꏊ�̒T������A���Ă��Ă���
        /// </summary>
        BackToMainRoutine,
        /// <summary>
        /// �m�C�Y�����ɂ���
        /// </summary>
        SenseNoize,
    }
}