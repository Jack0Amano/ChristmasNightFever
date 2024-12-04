using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        GameManager gameManager;

        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.Instance;
            startPanel.stageButtons.ForEach(b =>
            {
                b.Button.onClick.AddListener(() =>
                {
                    LoadStage(b.ID);
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

        #region Action from Panels
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
            yield return new WaitUntil(() => gameManager.CurrentGameState == GameState.Playing);
            loadingPanel.HidePanel(true);
        }

        #endregion
    }

}