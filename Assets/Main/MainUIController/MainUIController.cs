using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Action from Panels
        /// <summary>
        /// �N������GameManager����Ă΂��
        /// </summary>
        public void ShowStartPanel()
        {
            startPanel.ShowPanel();
        }

        /// <summary>
        /// �V�[��ID�����AGameManager�ɃV�[���̃��[�h���˗�����   
        /// �܂��A���[�h���̃p�l����\������
        /// </summary>
        private void LoadScene(int sceneID)
        {
            loadingPanel.ShowPanel();
            // GameManager�ɃV�[���̃��[�h���˗� ID��Addressable�̃��x�����ƑΉ�
            gameManager.LoadStage(sceneID);
        }

        #endregion
    }

}