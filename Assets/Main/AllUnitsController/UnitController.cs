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
        [SerializeField] UnitType unitType;
        [SerializeField] CameraUserController cameraUserController;

        /// <summary>
        /// TPS������i��Auser�̓��͂��󂯕t����    
        /// UnitController�����CameraUserController�̗v���ƁAAI�ł̎����ړ��̍ۂɎg����
        /// </summary>
        public ThirdPersonUserControl TPSController { get; private set; }

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
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Print(cameraUserController, TPSController);
            if (unitType == UnitType.Player)
            {
                // CamerauserController��Player���̎��ӂ�FollowCamera����悤�ɗv������
                StartCoroutine(cameraUserController.ChangeModeFollowTarget(TPSController));
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    /// <summary>
    /// Unit�̎��
    /// </summary>
    [Serializable] public enum UnitType
    {
        Player,
        Enemy
    }
}
