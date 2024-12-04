using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// namespace MainUIにはMainUIControllerとそれに付随するクラスを定義する
namespace MainUI
{
    /// <summary>
    /// MainUI (Title, Loading, Result) の制御を行う
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Tooltip("スタート画面のパネル")]
        [SerializeField] StartPanel startPanel;
        [Tooltip("ロード中に表示するパネル")]
        [SerializeField] LoadingPanel loadingPanel;
        [Tooltip("リザルト画面のパネル")]
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
        /// シーンIDを受取、GameManagerにシーンのロードを依頼する   
        /// また、ロード中のパネルを表示する
        /// </summary>
        private void LoadStage(string stageID)
        {
           
            // GameManagerにシーンのロードを依頼 IDはAddressableのラベル名と対応
            gameManager.LoadStage(stageID);
            StartCoroutine(ShowLoadingAtLoadGame());
        }

        #region Action from Panels
        /// <summary>
        /// 起動時にGameManagerから呼ばれる
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
        /// ゲームのステージなどのロードが始まったときに呼ばれて、LoadingPanelを表示   
        /// GameStateがPlayingになった際にロード画面を非表示にする
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