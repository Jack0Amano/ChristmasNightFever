using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using StageObjects;
using Units;
using System;

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

        /// <summary>
        /// UIパネルが閉じられた際の通知をGameStreamerに送るためのイベント
        /// </summary>
        public EventHandler<OnUIPanelClosedEventArgs> onUIPanelClosedHandler;


        /// <summary>
        /// GameStreamerにシーンのロードを依頼するためのイベント
        /// Eventの関数の設定はGameStreamerのSetEventsで行う
        /// </summary>
        public EventHandler<RequestLoadStageEventArgs> requestLoadStageHandler;

        private void Awake()
        {
            loseResultVirtualCamera.Priority = 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            startPanel.stageButtons.ForEach(b =>
            {
                b.button.onClick.AddListener(() =>
                {
                    StartCoroutine(LoadStage(b.stageID));
                });
            });
        }

        private void OnDestroy()
        {
            requestLoadStageHandler = null;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// シーンIDを受取、GameManagerにシーンのロードを依頼する   
        /// また、ロード中のパネルを表示する
        /// </summary>
        private IEnumerator LoadStage(string stageID)
        {
            loadingPanel.ShowPanel(0.5f);
            yield return new WaitForSeconds(0.5f);
            requestLoadStageHandler?.Invoke(this, new RequestLoadStageEventArgs(stageID));
            startPanel.HidePanel();
            resultPanel.HidePanel();
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
        /// メインメッセージパネルを表示する
        /// </summary>
        /// <returns></returns>
        internal void ShowMessagesPanel()
        {
            // メッセージが存在しない場合表示を行わずにcloseイベントを発生させる
            if (messagePanel.DoesMessageExist)
            {
                messagePanel.ShowPanel();
                loadingPanel.HidePanel(true);
            }
            else
            {
                onUIPanelClosedHandler?.Invoke(this, new OnUIPanelClosedEventArgs(UIPanelType.Message));
                loadingPanel.HidePanel(true);
            }
        }

        /// <summary>
        /// リザルト画面が表示される Gamemanagerから呼ばれる
        /// </summary>
        public IEnumerator ShowResultPanel(string doneStageID, GameResultType gameResultType)
        {
            const float duration = 0.5f;
            loadingPanel.ShowPanel(duration);
            yield return new WaitForSeconds(duration);
            resultPanel.ShowPanel(doneStageID, gameResultType);
            resultPanel.retryButton.onClick.RemoveAllListeners();
            resultPanel.retryButton.onClick.AddListener(() =>
            {
                StartCoroutine(LoadStage(doneStageID));
            });
            loadingPanel.HidePanel(true);

            if (gameResultType == GameResultType.Win)
            {
                cameraUserController.SetFreeCameraMode(messageVirtualCamera);
            }
            else
            {
                cameraUserController.SetFreeCameraMode(loseResultVirtualCamera);
            }
        
        }

        /// <summary>
        /// MessagePanelが終了した際にMessagePanelより呼ばれる
        /// </summary>
        internal void OnMessagePanelClosed()
        {
            static IEnumerator DisableCursorDelay()
            {
                yield return new WaitForSeconds(1f);
                UserController.enableCursor = false;
            }
            onUIPanelClosedHandler?.Invoke(this, new OnUIPanelClosedEventArgs(UIPanelType.Message));
            
            StartCoroutine(DisableCursorDelay());
            StartCoroutine( loadingPanel.ShowPanelForSeconds(1.5f));
        }

    }
}