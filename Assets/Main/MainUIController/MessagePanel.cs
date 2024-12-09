using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.AzureSky;

namespace MainUI
{
    public class MessagePanel : MonoBehaviour
    {
      
        [SerializeField] TextMeshProUGUI messageText;
        [Tooltip("��ʑS�̂𕢂��{�^�� ����ŃR���e�i��Message�����ɑ���")]
        [SerializeField] public Button overlayButton;

        [SerializeField] List<MessageContainer> messageContainerList;

        [Tooltip("�����葁�����b�Z�[�W���肪�ł��Ȃ��悤�ɂ���")]
        [SerializeField] float messageInterval = 0.5f;

        [SerializeField] private AzureCoreSystem azureCoreSystem;

        private MessageContainer currentCountainer;
        private int messageIndex = 0;

        // �Ō�ɕ\���������b�Z�[�W�̎���
        private float lastMessageTime;

        /// <summary>
        /// �\�����郁�b�Z�[�W�����邩�ǂ���
        /// </summary>
        public bool DoesMessageExist
        {
            get => messageContainerList != null && 
                   messageContainerList.Count != 0 && 
                   messageIndex < messageContainerList[0].messages.Count;
        }

        // Start is called before the first frame update
        void Start()
        {
            gameObject.SetActive(false);
            messageText.text = "";
            overlayButton.onClick.AddListener(NextMessage);
            ChangeWeatherToSnow();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <system>
        /// �V�C���ɕύX���� globallist��8�� specialID����w��ł���
        /// </system>
        private void ChangeWeatherToSnow()
        {
            azureCoreSystem.weatherSystem.SetGlobalWeather(8);
        }

        /// <summary>
        /// OverlayButton�������ꂽ�Ƃ��ɌĂ΂�� ����Ŏ��̃��b�Z�[�W��\������
        /// </summary>
        private void NextMessage()
        {
            if (Time.time - lastMessageTime < messageInterval)
            {
                return;
            }
            if (messageIndex < currentCountainer.messages.Count)
            {
                var message = currentCountainer.messages[messageIndex];
                // \\n�����s�ɕϊ�
                message = message.Replace("\\n", "\n");
                messageText.text = message;
            }
            else
            {
                GameManager.Instance.OnMessagePanelClosed();
                gameObject.SetActive(false);
                
            }
            lastMessageTime = Time.time;
            messageIndex++;
        }

        /// <summary>
        /// �p�l���̕\��
        /// </summary>
        internal void ShowPanel()
        {
            if (messageContainerList == null || messageContainerList.Count == 0)
            {
                GameManager.Instance.OnMessagePanelClosed();
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            currentCountainer = messageContainerList[0];
            NextMessage();
        }
    }

    // TODO �{���̓C�x���g�m�[�h�ōs�����������Ԃ��Ȃ����A�\�����郁�b�Z�[�W�͊ȑf�ŒZ���̂ŃC���X�y�N�^�ő�p
    // �C���X�y�N�^�͊ȒP�ɏ�����̂Œ���
    /// <summary>
    /// �C���X�y�N�^��Ń��b�Z�[�W��ۑ����Ă������߂̃N���X
    /// </summary>
    [Serializable] class MessageContainer
    {
        [SerializeField] internal string containerID;
        [Tooltip("���b�Z�[�W��ۑ����郊�X�g")]
        [SerializeField] internal List<string> messages;
        [Tooltip("�I��\���Ŏ��̃R���e�i �I�����̃��b�Z�[�W�͂��̌��̃R���e�i��index0���b�Z�[�W���g�p�����")]
        [SerializeField] internal List<string> nextMessageContainersID;
        /// <summary>
        /// ��b�r���Ő���~�点���肷�邽�߂̓����ID
        /// </summary>
        [Tooltip("�R���e�i�̃��b�Z�[�W���o���I������Ɏ��s���������ID")]
        [SerializeField] internal string specialID;
    }
}