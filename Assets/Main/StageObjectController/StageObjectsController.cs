using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageObjects
{
    /// <summary>
    /// �X�e�[�W�Ƃ��ēǂݍ��܂��Prefab�̍ł��㗬�̐e�I�u�W�F�N�g�ɃA�^�b�`�����R���|�[�l���g   
    /// ��ɃX�e�[�W�ŗL�̏���Prefab��Inspector��Őݒ肷�邽�߂̂���
    /// </summary>
    public class StageObjectsController : MonoBehaviour
    {
        [Tooltip("�X�e�[�W�̃`�F�b�N�|�C���gObject��ݒ肷��")]
        [SerializeField] List<CheckPoint> checkPoints = new List<CheckPoint>();


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