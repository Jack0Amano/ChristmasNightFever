using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using StageObjects;
using Units;
using System;

// StageID��Inspector��Őݒ肳��Ă���A��Փx�Ȃǂ�����΂����̃f�[�^�ƕR�Â��邽�߂̃f�[�^�̎Q�ƌ����K�v

// namespace MainUI�ɂ�MainUIController�Ƃ���ɕt������N���X���`����
namespace MainUI
{
    /// <summary>
    /// MainUI (Title, Loading, Result) �̐�����s��
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Tooltip("�X�^�[�g��ʂ̃p�l��")]
        [SerializeField] StartPanel startPanel;
        [Tooltip("���[�h���ɕ\������p�l��")]
        [SerializeField] LoadingPanel loadingPanel;
        [Tooltip("���U���g��ʂ̃p�l��")]
        [SerializeField] ResultPanel resultPanel;
        [Tooltip("���b�Z�[�W��\������p�l��")]
        [SerializeField] MessagePanel messagePanel;

        [Tooltip("���b�Z�[�W��Start���Win��ʂ̍ۂɂ�\������o�[�`�����J����")]
        [SerializeField] CinemachineVirtualCamera messageVirtualCamera;
        [Tooltip("Lose��ʂ̍ۂɂ�\������o�[�`�����J����")]
        [SerializeField] CinemachineVirtualCamera loseResultVirtualCamera;

        [SerializeField] CameraUserController cameraUserController;

        /// <summary>
        /// UI�p�l��������ꂽ�ۂ̒ʒm��GameStreamer�ɑ��邽�߂̃C�x���g
        /// </summary>
        public EventHandler<OnUIPanelClosedEventArgs> onUIPanelClosedHandler;


        /// <summary>
        /// GameStreamer�ɃV�[���̃��[�h���˗����邽�߂̃C�x���g
        /// Event�̊֐��̐ݒ��GameStreamer��SetEvents�ōs��
        /// </summary>
        public EventHandler<RequestLoadStageEventArgs> requestLoadStageHandler;

        private void Awake()
        {
            loseResultVirtualCamera.Priority = 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            startPanel.stageButtons.ForEach(b =>
            {
                b.button.onClick.AddListener(() =>
                {
                    StartCoroutine(LoadStage(b.stageID));
                });
            });
        }

        private void OnDestroy()
        {
            requestLoadStageHandler = null;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// �V�[��ID�����AGameManager�ɃV�[���̃��[�h���˗�����   
        /// �܂��A���[�h���̃p�l����\������
        /// </summary>
        private IEnumerator LoadStage(string stageID)
        {
            loadingPanel.ShowPanel(0.5f);
            yield return new WaitForSeconds(0.5f);
            requestLoadStageHandler?.Invoke(this, new RequestLoadStageEventArgs(stageID));
            startPanel.HidePanel();
            resultPanel.HidePanel();
        }


        /// <summary>
        /// �N������GameManager����Ă΂��
        /// </summary>
        public void ShowStartPanelAtAwake()
        {
            startPanel.gameObject.SetActive(true);
            loadingPanel.gameObject.SetActive(true);
            resultPanel.gameObject.SetActive(true);
            startPanel.ShowPanel();
            loadingPanel.HidePanel(false);
            resultPanel.HidePanel();
            cameraUserController.SetFreeCameraMode(messageVirtualCamera);
        }

        /// <summary>
        /// ���C�����b�Z�[�W�p�l����\������
        /// </summary>
        /// <returns></returns>
        internal void ShowMessagesPanel()
        {
            // ���b�Z�[�W�����݂��Ȃ��ꍇ�\�����s�킸��close�C�x���g�𔭐�������
            if (messagePanel.DoesMessageExist)
            {
                messagePanel.ShowPanel();
                loadingPanel.HidePanel(true);
            }
            else
            {
                onUIPanelClosedHandler?.Invoke(this, new OnUIPanelClosedEventArgs(UIPanelType.Message));
                loadingPanel.HidePanel(true);
            }
        }

        /// <summary>
        /// ���U���g��ʂ��\������� Gamemanager����Ă΂��
        /// </summary>
        public IEnumerator ShowResultPanel(string doneStageID, GameResultType gameResultType)
        {
            const float duration = 0.5f;
            loadingPanel.ShowPanel(duration);
            yield return new WaitForSeconds(duration);
            resultPanel.ShowPanel(doneStageID, gameResultType);
            resultPanel.retryButton.onClick.RemoveAllListeners();
            resultPanel.retryButton.onClick.AddListener(() =>
            {
                StartCoroutine(LoadStage(doneStageID));
            });
            loadingPanel.HidePanel(true);

            if (gameResultType == GameResultType.Win)
            {
                cameraUserController.SetFreeCameraMode(messageVirtualCamera);
            }
            else
            {
                cameraUserController.SetFreeCameraMode(loseResultVirtualCamera);
            }
        
        }

        /// <summary>
        /// MessagePanel���I�������ۂ�MessagePanel���Ă΂��
        /// </summary>
        internal void OnMessagePanelClosed()
        {
            static IEnumerator DisableCursorDelay()
            {
                yield return new WaitForSeconds(1f);
                UserController.enableCursor = false;
            }
            onUIPanelClosedHandler?.Invoke(this, new OnUIPanelClosedEventArgs(UIPanelType.Message));
            
            StartCoroutine(DisableCursorDelay());
            StartCoroutine( loadingPanel.ShowPanelForSeconds(1.5f));
        }

    }
}