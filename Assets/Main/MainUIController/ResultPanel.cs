using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

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


        Image backgroundImage;

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            exitButton.onClick.AddListener(() =>
            {
                GameManager.Instance.EndGame();
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
        /// パネルを表示する ゲームが終わるときのアニメーションのため、エモい感じのアニメーションを利用する
        /// </summary>
        internal void ShowPanel(string stageID, bool doesWin)
        {
            gameObject.SetActive(true);
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
