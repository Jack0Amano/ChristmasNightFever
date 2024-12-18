using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

namespace MainUI
{
    /// <summary>
    /// ���U���g��ʂ̐�����s�� Win Lose�̗����̕\�����s��   
    /// Win�̏ꍇ�̓X�R�A��\������   
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] internal Button retryButton;
        [SerializeField] Button exitButton;
        [SerializeField] private TextMeshProUGUI resultLabel;

        Image backgroundImage;

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            exitButton.onClick.AddListener(() =>
            {
                MainGame.GameManager.Instance.EndGame();
            });
            resultLabel.text = "";
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
        /// �p�l����\������ �Q�[�����I���Ƃ��̃A�j���[�V�����̂��߁A�G���������̃A�j���[�V�����𗘗p����
        /// </summary>
        internal void ShowPanel(string stageID, GameResultType gameResultType)
        {
            gameObject.SetActive(true);
            if (gameResultType == GameResultType.Win)
            {
                resultLabel.text = "����";
                backgroundImage.DOColor(Color.green, 1);
            }
            else if (gameResultType == GameResultType.Lose)
            {
                resultLabel.text = "����";
                backgroundImage.DOColor(Color.red, 1);
            }
        }

        /// <summary>
        /// �p�l�����B�� �Q�[�����I���Ƃ��̃A�j���[�V�����̂��߁A�G���������̃A�j���[�V�����𗘗p����
        /// </summary>
        /// <param name="animation"></param>
        internal void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
