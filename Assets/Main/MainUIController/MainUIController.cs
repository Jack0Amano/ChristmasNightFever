using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Action from Panels
        /// <summary>
        /// 起動時にGameManagerから呼ばれる
        /// </summary>
        public void ShowStartPanel()
        {
            startPanel.ShowPanel();
        }

        /// <summary>
        /// シーンIDを受取、GameManagerにシーンのロードを依頼する   
        /// また、ロード中のパネルを表示する
        /// </summary>
        private void LoadScene(int sceneID)
        {
            loadingPanel.ShowPanel();
            // GameManagerにシーンのロードを依頼 IDはAddressableのラベル名と対応
            gameManager.LoadStage(sceneID);
        }

        #endregion
    }

}