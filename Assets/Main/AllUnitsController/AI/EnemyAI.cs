using System;
using System.Collections;
using System.Collections.Generic;
using Units.Icon;
using Units.TPS;
using UnityEngine;
using UnityEngine.AI;

namespace Units.AI
{
    /// <summary>
    /// �G��AI�𐧌䂷��
    /// </summary>
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

        internal ThirdPersonUserControl TPSController;

        /// <summary>
        /// �O��TryFindPlayer���Ăяo���ł���̌o�߃~���b
        /// </summary>
        float lastTryFindPlayerSec = 0;

        private NavMeshAgent navMeshAgent;
        private NavMeshObstacle navMeshObstacle;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
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

        #region Move with AI
        // navmeshagent���g���Ĉړ�����ꍇ�́@NavMeshAgent��Component��ǉ����Ă���

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
            yield return StartCoroutine(TPSController.AutoMove(location, navMeshAgent, debugDrawAimWalkingLine));

            //navMeshAgent.isStopped = true;

            //navMeshAgent.enabled = false;

            yield return true;
        }
        #endregion

        #region Methods try to find player
        /// <summary>
        /// Enemy���v���C���[�������悤�Ƃ��郋�[�v
        /// </summary>
        private void TryFindPlayer()
        {
            if (FindOutLevel <= 1)
            {
                var dist = GetDistanceIfPlayerInSight();
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
                //�@���E���ɑ��݂��Ă���
                return detectionDistanceCurve.Evaluate(distance);
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
}