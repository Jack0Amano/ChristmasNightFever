using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using StageObjects;
using Units;

// StageIDがInspector上で設定されており、難易度などがあればそれらのデータと紐づけるためのデータの参照元が必要

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
        [Tooltip("メッセージを表示するパネル")]
        [SerializeField] MessagePanel messagePanel;

        [Tooltip("メッセージとStart画面Win画面の際にを表示するバーチャルカメラ")]
        [SerializeField] CinemachineVirtualCamera messageVirtualCamera;
        [Tooltip("Lose画面の際にを表示するバーチャルカメラ")]
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
        /// シーンIDを受取、GameManagerにシーンのロードを依頼する   
        /// また、ロード中のパネルを表示する
        /// </summary>
        private void LoadStage(string stageID)
        {
           
            // GameManagerにシーンのロードを依頼 IDはAddressableのラベル名と対応
            gameManager.LoadStage(stageID);
            StartCoroutine(ShowLoadingAtLoadGame());
        }


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
            cameraUserController.SetFreeCameraMode(messageVirtualCamera);
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
        }

        /// <summary>
        /// メインメッセージパネルを表示する
        /// </summary>
        /// <returns></returns>
        internal void ShowMessagesPanel()
        {
            loadingPanel.HidePanel(true);
            messagePanel.ShowPanel();
        }

        /// <summary>
        /// リザルト画面が表示される Gamemanagerから呼ばれる
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
        /// MessagePanelが終了
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