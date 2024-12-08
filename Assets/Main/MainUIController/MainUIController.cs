using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using StageObjects;
using Units;

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

        GameManager gameManager;


        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.Instance;
            startPanel.stageButtons.ForEach(b =>
            {
                b.button.onClick.AddListener(() =>
                {
                    LoadStage(b.stageID);
                });
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// �V�[��ID�����AGameManager�ɃV�[���̃��[�h���˗�����   
        /// �܂��A���[�h���̃p�l����\������
        /// </summary>
        private void LoadStage(string stageID)
        {
           
            // GameManager�ɃV�[���̃��[�h���˗� ID��Addressable�̃��x�����ƑΉ�
            gameManager.LoadStage(stageID);
            StartCoroutine(ShowLoadingAtLoadGame());
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
        /// �Q�[���̃X�e�[�W�Ȃǂ̃��[�h���n�܂����Ƃ��ɌĂ΂�āALoadingPanel��\��   
        /// GameState��Playing�ɂȂ����ۂɃ��[�h��ʂ��\���ɂ���
        /// </summary>
        internal IEnumerator ShowLoadingAtLoadGame()
        {
            const float duration = 0.5f;
            loadingPanel.ShowPanel(duration);
            yield return new WaitForSeconds(duration);
            startPanel.HidePanel();
            resultPanel.HidePanel();
        }

        /// <summary>
        /// ���C�����b�Z�[�W�p�l����\������
        /// </summary>
        /// <returns></returns>
        internal void ShowMessagesPanel()
        {
            loadingPanel.HidePanel(true);
            messagePanel.ShowPanel();
        }

        /// <summary>
        /// ���U���g��ʂ��\������� Gamemanager����Ă΂��
        /// </summary>
        public IEnumerator ShowResultPanel(string doneStageID, bool doesWin)
        {
            const float duration = 0.5f;
            loadingPanel.ShowPanel(duration);
            yield return new WaitForSeconds(duration);
            resultPanel.ShowPanel(doneStageID, doesWin);
            resultPanel.retryButton.onClick.RemoveAllListeners();
            resultPanel.retryButton.onClick.AddListener(() =>
            {
                LoadStage(doneStageID);
            });
            loadingPanel.HidePanel(true);

            if (doesWin)
            {
                cameraUserController.SetFreeCameraMode(messageVirtualCamera);
            }
            else
            {
                cameraUserController.SetFreeCameraMode(loseResultVirtualCamera);
            }
        
        }

        /// <summary>
        /// MessagePanel���I��
        /// </summary>
        public void OnMessagePanelClosed()
        {
            IEnumerator Show()
            {
                yield return new WaitForSeconds(1f);
                UserController.enableCursor = false;
            }

            StartCoroutine(Show());
            StartCoroutine( loadingPanel.ShowPanelForSeconds(1.5f));
        }
    }

}