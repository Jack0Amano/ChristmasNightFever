using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageObjects
{
    /// <summary>
    /// �X�e�[�W�̊e��`�F�b�N�|�C���g(Goal, Event, etc.)��ݒ肷�邽�߂̃R���|�[�l���g   
    /// ���̃R���|�[�l���g������stageObject�ɃA�^�b�`���邱�ƂŁA�X�e�[�W�̃`�F�b�N�|�C���g�Ƃ��ċ@�\����
    /// </summary>
    [RequireComponent(typeof(NotifyTrigger))]
    public class CheckPoint : MonoBehaviour
    {
        [Tooltip("�`�F�b�N�|�C���g�̎�ނ�ݒ肷��")]
        [SerializeField] internal CheckPointType checkPointType;

        [Tooltip("�`�F�b�N�|�C���g��ID��ݒ肷�� �C�x���g�ȂǂŎg����")]
        [SerializeField] internal int checkPointID;

        [Tooltip("�C�x���g�ŕ\�������e�L�X�g")]
        [SerializeField] internal string eventText;

        internal NotifyTrigger NortifyTrigger { private set; get; }

        private void Awake()
        {
            NortifyTrigger = GetComponent<NotifyTrigger>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
#endif
    }

    /// <summary>
    /// �`�F�b�N�|�C���g���ǂ̗l�ȓ�����������̂�
    /// </summary>
    [SerializeField] internal enum CheckPointType
    {
        Goal,
        Event,
        PlayerSpawn,
    }
}