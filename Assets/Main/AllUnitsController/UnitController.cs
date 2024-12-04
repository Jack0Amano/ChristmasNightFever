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
    /// Unit��Prefab�ɃR���|�[�l���g�Ƃ��Ă���Unit�̋������Ǘ�����
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Tooltip("Unit�̎�ނ�ݒ肷��")]
        [SerializeField] internal UnitType unitType;
        [SerializeField] CameraUserController cameraUserController;

        // PlayerTarget������Collider��DefaultLayer��Object��Collider�͏Փ˂��Ȃ��ݒ�ɂȂ��Ă���
        [Tooltip("Unit��Player�ł���ꍇ�A�G�Ɍ������锻��\nCllider������Layer��PlayerTarget�ɐݒ肳��Ă���K�v������")]
        [SerializeField] internal Collider targetCollider;

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
        public UnitController PlayerUnitController { set => enemyAI.playerUnitController = value; get => enemyAI.playerUnitController;}

        /// <summary>
        /// ����Unit���G�̏ꍇ�A����AI��ݒ肷��
        /// </summary>
        EnemyAI enemyAI;

        private void Awake()
        {
            TPSController = GetComponent<ThirdPersonUserControl>();
            if (unitType == UnitType.Enemy)
            {
                enemyAI = GetComponent<EnemyAI>();
                enemyAI.playerUnitController = PlayerUnitController;
                enemyAI.OnFoundPlayer += (sender, e) => FoundYou();
            }
        }

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
        }

        #region �A�j���[�V�����Ȃǂ��܂ރA�N�V�������N����
        /// <summary>
        /// ���j�b�g�������𗧂Ă�
        /// </summary>
        internal void MakeNoize()
        {
            // DOIT ThirdPersonCharacter�ŃA�j���[�V�������Đ�����
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.MakeNoize));
        }

        /// <summary>
        /// ���j�b�g��target�����������������m���邩����
        /// </summary>
        internal void SenseNoize(UnitController target)
        {
            // DOIT ThirdPersonCharacter�ŃA�j���[�V�������Đ����AAI�ɒʒm����
            
        }

        /// <summary>
        /// FoundYou���ꂽ���߃v���C���[�����S���� (�Q�[���I�[�o�[)
        /// </summary>
        internal IEnumerator Killed()
        {
            TPSController.IsTPSControllActive = false;
            yield return new WaitForSeconds(3.0f);

            // �����œ|���Ȃǂ̃A�j���[�V�������Đ�����
            // �J�������|�ꂽ�҂��B�銴���̃A�j���[�V�����ɕύX

            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.Die));
        }

        /// <summary>
        /// Enemy���v���C���[�������� �����AllUnitsController���炷�ׂĂ�Enemy�ɋ��L���� Player���ɂ�Die���ʒm�����
        /// </summary>
        internal void FoundYou()
        {
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.FindYou));
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
