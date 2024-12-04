using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace StageObjects
{
    /// <summary>
    /// �X�e�[�W�Ƃ��ēǂݍ��܂��Prefab�̍ł��㗬�̐e�I�u�W�F�N�g�ɃA�^�b�`�����R���|�[�l���g   
    /// ��ɃX�e�[�W�ŗL�̏���Prefab��Inspector��Őݒ肷�邽�߂̂���
    /// </summary>
    public class StageObjectsController : MonoBehaviour
    {
        [Tooltip("CheckPoint����ꂽObject�̐e")]
        [SerializeField] GameObject checkPointsParent;
        [Tooltip("Enemy�̈ړ��o�H������Editorways")]
        [SerializeField] public EditorWays editorWays;

        /// <summary>
        /// Player�̃X�|�[���n�_ CheckPoints�̒�����PlayerSpawn��CheckPoint���擾����
        /// </summary>
        public CheckPoint PlayerSpawnPoint { get; private set; }

        /// <summary>
        /// Enemy�̃X�|�[���n�_ EditorWays�̒�����o�Hindex=0�̍ŏ���Point���擾����
        /// </summary>
        public List<Transform> EnemySpawnPoints { get; private set; }


        /// <summary>
        /// �X�e�[�W�̃`�F�b�N�|�C���g Player�̃X�|�[���n�_��Goal�Ȃǂ̏�������
        /// </summary>
        public List<CheckPoint> CheckPoints { get; private set; }

        private void Awake()
        {
            CheckPoints = new List<CheckPoint>(checkPointsParent.GetComponentsInChildren<CheckPoint>());
            PlayerSpawnPoint = CheckPoints.Find(cp => cp.checkPointType == CheckPointType.PlayerSpawn);
            EnemySpawnPoints = editorWays.ways.ConvertAll(w => w.pointsParent.GetChild(0));
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}