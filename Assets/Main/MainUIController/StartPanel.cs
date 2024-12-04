using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Linq;
using static Utility;

namespace MainUI
{
    /// <summary>
    /// �Q�[���J�n���̃p�l��    
    /// �X�e�[�W�I���Ȃǂ�UI��\������
    /// </summary>
    public class StartPanel : MonoBehaviour
    {

        [SerializeField] internal List<StartPanelButtonStageID> stageButtons;
        [Tooltip("�Q�[�����I������{�^��")]
        [SerializeField] private Button exitButton;


        private void Awake()
        {
            exitButton.onClick.AddListener(() =>
            {
                GameManager.Instance.EndGame();
            });
            stageButtons.ForEach(b =>
            {
                b.Button.onClick.AddListener(() =>
                {
                    stageButtons.ForEach(s => s.Button.interactable = false);
                });
            });
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// �p�l����\������
        /// </summary>
        internal void ShowPanel()
        {
            stageButtons.ForEach(b=> b.Button.interactable = true);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// �p�l�����\���ɂ���
        /// </summary>
        internal void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// �X�e�[�W��01��Turorial�݂̂Ȃ̂ŉ��ɁAButton��addresaable ID����������
    /// </summary>
    [Serializable] internal class StartPanelButtonStageID
    {
        /// <summary>
        /// Button
        /// </summary>
        public Button Button;
        /// <summary>
        /// Button�I����Load����Stage��Addressable ID
        /// </summary>
        [Tooltip("Button�I����Load����Stage��Addressable ID")]
        public string ID;
    }
}
