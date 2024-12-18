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
    /// リザルト画面の制御を行う Win Loseの両方の表示を行う   
    /// Winの場合はスコアを表示する   
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
        /// パネルを表示する ゲームが終わるときのアニメーションのため、エモい感じのアニメーションを利用する
        /// </summary>
        internal void ShowPanel(string stageID, GameResultType gameResultType)
        {
            gameObject.SetActive(true);
            if (gameResultType == GameResultType.Win)
            {
                resultLabel.text = "勝利";
                backgroundImage.DOColor(Color.green, 1);
            }
            else if (gameResultType == GameResultType.Lose)
            {
                resultLabel.text = "負け";
                backgroundImage.DOColor(Color.red, 1);
            }
        }

        /// <summary>
        /// パネルを隠す ゲームが終わるときのアニメーションのため、エモい感じのアニメーションを利用する
        /// </summary>
        /// <param name="animation"></param>
        internal void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
