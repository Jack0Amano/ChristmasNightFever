using System;
using System.Collections;
using System.Collections.Generic;
using Units.AI;
using Units.TPS;
using UnityEngine;
using Cinemachine;
using static Utility;

namespace Units
{
    /// <summary>
    /// Unit��Prefab�ɃR���|�[�l���g�Ƃ��Ă���Unit�̋������Ǘ�����
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Tooltip("Unit�̎�ނ�ݒ肷��")]
        [SerializeField] internal UnitType unitType;


        // PlayerTarget������Collider��DefaultLayer��Object��Collider�͏Փ˂��Ȃ��ݒ�ɂȂ��Ă���
        [Tooltip("Unit��Player�ł���ꍇ�A�G�Ɍ������锻��\nCllider������Layer��PlayerTarget�ɐݒ肳��Ă���K�v������")]
        [SerializeField] internal Collider targetCollider;

        [Header("Debug�p�Ɏg��Camera\n������Z�b�g�����UnitController��CameraUserController�݂̂�TPS���삪�\�ɂȂ�")]
        [SerializeField] CameraUserController cameraUserController;

        /// <summary>
        /// AllUnitCon.OnUnitAction(UnitActionEventArgs e)���Ăяo�����߂̃C�x���g
        /// </summary>
        internal event EventHandler<UnitActionEventArgs> OnUnitAction;

        /// <summary>
        /// TPS������i��Auser�̓��͂��󂯕t����    
        /// UnitController�����CameraUserController�̗v���ƁAAI�ł̎����ړ��̍ۂɎg����
        /// </summary>
        public ThirdPersonUserControl TPSController { get; private set; }

        /// <summary>
        /// Player��UnitController��ݒ肷�� Player�̌��m�Ȃǂ��s�����ߓG���ɕK�v
        /// </summary>
        public UnitController PlayerUnitController { set => EnemyAI.playerUnitController = value; get => EnemyAI.playerUnitController;}

        public SEController SEController { private set; get; }

        /// <summary>
        /// ����Unit���G�̏ꍇ�A����AI��ݒ肷��
        /// </summary>
        public EnemyAI EnemyAI { private set; get; }

        private AudioSource audioSource;
        /// <summary>
        /// �ꎞ��~���ł��邩
        /// </summary>
        public bool IsPaused { get; private set; } = false;

        private void Awake()
        {
            TPSController = GetComponent<ThirdPersonUserControl>();
            audioSource = GetComponent<AudioSource>();

            TPSController.makeNoiseAction +=  MakeNoizeEvent;
            SEController = GetComponent<SEController>();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (cameraUserController != null)
            {
                Print("Debug mode is enabled. Controlling", gameObject.name);
                UserController.enableCursor = false;
                TPSController.IsTPSControllActive = true;
                print(TPSController);
                StartCoroutine(cameraUserController.SetAsFollowTarget(TPSController));
            }

            //var animator = GetComponent<Animator>();
            //if (unitType == UnitType.Player)
            //{
            //    animator.SetBool("AlongWall", true);
            //}
            //else
            //{
            //    animator.SetBool("AlongWall", false);
            //}
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
            
        }

        /// <summary>
        /// �Q�[�����J�n����
        /// </summary>
        internal void StartGame()
        {
            EnemyAI?.StartAI();
        }

        /// <summary>
        /// Unit�̑���y�эs�����ꎞ��~����
        /// </summary>
        internal void PauseUnit()
        {
            if (unitType == UnitType.Player)
            {
                TPSController.IsTPSControllActive = false;
                TPSController.PauseAnimation = true;
                
            }
            else
            {
                EnemyAI?.PauseAI();
            }
            IsPaused = true;
        }

        /// <summary>
        /// �Q�[���̃|�[�Y��Ԃ���������
        /// </summary>
        internal void UnpauseUnit()
        {
            if (unitType == UnitType.Player)
            {
                TPSController.IsTPSControllActive = true;
                TPSController.PauseAnimation = false;
            }
            else
            {
                EnemyAI?.UnpauseAI();
            }
            IsPaused = false;
        }

        /// <summary>
        /// Enemy�Ƃ��Ă�Unit�̏����ݒ�
        /// </summary>
        /// <param name="way">AI���H��|�C���g�̃��X�g</param>
        internal void SetUnitAsEnemy(List<StageObjects.PointAndStopTime> way)
        {
            EnemyAI = GetComponent<EnemyAI>();
            EnemyAI.playerUnitController = PlayerUnitController;
            EnemyAI.tpsController = TPSController;
            EnemyAI.OnFoundPlayer += (sender, e) => FoundYou(this);
            EnemyAI.way = way;
            TPSController.IsTPSControllActive = false;
        }

        /// <summary>
        /// �������ăQ�[�����I���������Ƃ�ʒm����
        /// </summary>
        internal void FinishToGameAsWin(CameraUserController cameraUserController, CinemachineVirtualCamera winVirtualCamera)
        {
            if (unitType == UnitType.Player)
            {
                TPSController.IsTPSControllActive = false;
                winVirtualCamera.Priority = 1100;
                winVirtualCamera.LookAt = transform;
                StartCoroutine(TPSController.Victory());
                var bgmCon = cameraUserController.GetComponent<BGMController>();
                bgmCon.PlayWinBGM();
            }
            else
            {
                EnemyAI?.StopAI();
            }
        }

        #region �A�j���[�V�����Ȃǂ��܂ރA�N�V�������N���� �������̓A�N�V�������󂯎��
        /// <summary>
        /// ���j�b�g�������𗧂Ă�
        /// </summary>
         private void MakeNoizeEvent()
        {
            // DOIT ThirdPersonCharacter�ŃA�j���[�V�������Đ�����
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.MakeNoize));
        }

        /// <summary>
        /// ���j�b�g��target�����������������m���邩����
        /// </summary>
        internal void SenseNoize(UnitController target)
        {
            EnemyAI.SenceNoiseAction(target);
        }

        /// <summary>
        /// FoundYou���ꂽ���߃v���C���[�����S���� (�Q�[���I�[�o�[)
        /// </summary>
        internal IEnumerator Killed()
        {
            TPSController.IsTPSControllActive = false;
            // �����œ|���Ȃǂ̃A�j���[�V�������Đ�����
            // �J�������|�ꂽ�҂��B�銴���̃A�j���[�V�����ɕύX
            StartCoroutine(TPSController.Killed());
            yield return new WaitForSeconds(4.0f);
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.Die));
        }

        /// <summary>
        /// Enemy���v���C���[�������� �����AllUnitsController���炷�ׂĂ�Enemy�ɋ��L���� Player���ɂ�Die���ʒm�����
        /// </summary>
        internal void FoundYou(UnitController sender)
        {

            IEnumerator DelayWinVoice()
            {
                yield return new WaitForSeconds(1.5f);
                SEController.EnemyWinVoiceSE();
            }

            if (sender.gameObject == this.gameObject)
            {
                SEController.EnemyrWhistleSE();
                OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.FindYou));
            }
            else
            {
                EnemyAI?.StopAI();
                StartCoroutine(DelayWinVoice());
            }
        }
        #endregion
    }

    /// <summary>
    /// Unit�̎��
    /// </summary>
    [Serializable] public enum UnitType
    {
        Player,
        Enemy
    }

    public enum UnitAction
    {
        /// <summary>
        /// ��������ăQ�[�����I������Action
        /// </summary>
        Die,
        /// <summary>
        /// �����𗧂ĂėU�����Ă���
        /// </summary>
        MakeNoize,
        /// <summary>
        /// �����ɋC�Â��Ă���
        /// </summary>
        SenseNoize,
        /// <summary>
        /// �G�L�������v���C���[�������� �������ꂽ�瑦�Q�[���I�[�o�[�Ȃ̂ŁA�����n���ꂽPlayer��Die��Ԃ�
        /// </summary>
        FindYou,
        /// <summary>
        /// �S�[���ɓ��B����
        /// </summary>
        Goal
    }

    /// <summary>
    /// ���j�b�g�̃A�N�V�����������e������邽�߂�EventArgs
    /// </summary>
    public class UnitActionEventArgs : EventArgs
    {
        /// <summary>
        /// �s��ꂽ�A�N�V�����̓��e
        /// </summary>
        public UnitAction Action { get; private set; }

        /// <summary>
        /// �N���A�N�V�������s������
        /// </summary>
        public UnitController ActionFrom { get; private set; }

        /// <summary>
        /// ���j�b�g�̃A�N�V�����������e������邽�߂�EventArgs
        /// </summary>
        /// <param name="action">�s���̓��e</param>
        /// <param name="actionFrom">�N����A�N�V�������s��ꂽ��</param>
        public UnitActionEventArgs(UnitController actionFrom, UnitAction action)
        {
            Action = action;
            ActionFrom = actionFrom;
        }
    }
}
