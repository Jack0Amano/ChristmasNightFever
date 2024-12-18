using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using StageObjects;
using MainUI;
using Units;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Utility;
using UnityEngine.AzureSky;
using System;

namespace MainGame
{
    /// <summary>
    /// ゲームの流れを管理するクラス   
    /// 全てのControllerの上位に位置し、各Controllerの処理を制御するためのクラス   
    /// 各下流Controllerがこれに伝達する際はeventもしくはdelegateを使用する    
    /// 破棄される可能性のあるclassのdelegateの場合はweakReferenceを使用 (StageobjectControllerなど)   
    /// </summary>
    public class GameStreamer : MonoBehaviour
    {
        [Tooltip("MainUIControllerを設定する")]
        [SerializeField] private MainUIController mainUIController;
        [Tooltip("AllUnitsControllerを設定する")]
        [SerializeField] private AllUnitsController allUnitsController;

        /// <summary>
        /// Stageを読み込む際のAsyncOperationHandle
        /// </summary>
        private AsyncOperationHandle<GameObject> stageObjectControllerObj;

        /// <summary>
        /// 現在読み込まれているstageのメインコンポーネント
        /// </summary>
        public StageObjectsController StageObjectsController { get; private set; }

        GameManager gameManager;

        /// <summary>
        /// 各Controllerからこのclassに伝達するためのイベントをここで設定する (個々のclassでは管理しない)
        /// </summary>
        private void SetEvents()
        {
            if (mainUIController != null)
            {
                mainUIController.requestLoadStageHandler += RequestLoadStage;
                mainUIController.onUIPanelClosedHandler += OnCloseUIPanel;
            }
            else
            {
                PrintError("MainUIController is not set in GameStreamer");
            }
            if (allUnitsController != null)
            {
                allUnitsController.onUnitsLoadedAction += OnUnitsLoaded;
                allUnitsController.onGameResultEventHandler += OnGameResult;
            }
            else
            {
                PrintError("AllUnitsController is not set in GameStreamer");
            }
        }


        private void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.Instance;
            gameManager.CurrentGameState = GameState.Title;
            SetEvents();
            StartGame();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Game Stream
        // ゲームのStartからResultまでの流れを管理する

        /// <summary>
        /// Game Stream関数の中でゲームを起動して一番最初に呼び出される
        /// </summary>
        private void StartGame()
        {
            mainUIController.ShowStartPanelAtAwake();
        }

        /// <summary>
        /// ステージをAddressableからIDで読み込む MainUIControllerからユーザー操作にて呼び出される
        /// </summary>
        public void RequestLoadStage(object sender, RequestLoadStageEventArgs args)
        {
            gameManager.CurrentStageId = args.StageId;
            stageObjectControllerObj = Addressables.InstantiateAsync(args.StageId, new Vector3(0, 0, 0), Quaternion.identity);
            stageObjectControllerObj.Completed += (obj) => OnStageLoaded();
        }

        /// <summary>
        /// ステージの読み込みと設置が終わった際に呼び出される
        /// </summary>
        private void OnStageLoaded()
        {
            StageObjectsController = stageObjectControllerObj.Result.GetComponent<StageObjectsController>();
            allUnitsController.OnStageLoaded(StageObjectsController);
        }

        /// <summary>
        /// Unitsの読み込みと設置が終わった際にAllUnitsControllerから呼び出される
        /// </summary>
        private void OnUnitsLoaded()
        {
            ShowMainMessagesPanel();
        }

        /// <summary>
        /// MainMessagesPanelの表示
        /// </summary>
        private void ShowMainMessagesPanel()
        {
            mainUIController.ShowMessagesPanel();
        }

        /// <summary>
        /// MainUIPanelより何らかのUIが閉じられ、ユーザー操作がUIPanelを離れた際に呼び出される
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCloseUIPanel(object sender, OnUIPanelClosedEventArgs args)
        {
            switch (args.PanelType)
            {
                case UIPanelType.Message:
                    OnMessagePanelClosed();
                    break;
            }
        }

        /// <summary>
        /// MessagePanelが終了した際に呼ばれる AllUnitCOntrollerに伝えてゲーム開始
        /// </summary>
        private void OnMessagePanelClosed()
        {
            gameManager.CurrentGameState = GameState.Playing;
            UserController.enableCursor = false;
            allUnitsController.OnMessagePanelClosed();
        }

        /// <summary>
        /// ゲームの結果が出た際に呼び出される
        /// </summary>
        private void OnGameResult(object sender, OnGameResultEventArgs args)
        {
            if (args.ResultType == GameResultType.Win || args.ResultType == GameResultType.Lose)
            {
                UserController.enableCursor = true;
                gameManager.CurrentGameState = GameState.Result;
                StartCoroutine(mainUIController.ShowResultPanel(gameManager.CurrentStageId, args.ResultType));
                gameManager.CurrentStageId = "";
                // stageObjectControllerの破棄
                if (stageObjectControllerObj.IsValid())
                {
                    Addressables.ReleaseInstance(stageObjectControllerObj);
                    StageObjectsController = null;
                }
            }
            else if (args.ResultType == GameResultType.Cancel)
            {
                GameManager.Instance.EndGame();
            }
        }
        #endregion
    }
}


#region Event args between GameStreamer and each controllers
/// <summary>
/// ゲームのステージの読み込みをリクエストするHandlerの引数
/// </summary>
public class RequestLoadStageEventArgs: EventArgs
{
    /// <summary>
    /// 読み込むステージのaddressableのID
    /// </summary>
    public string StageId { get; private set; }

    public RequestLoadStageEventArgs(string stageId)
    {
        StageId = stageId;
    }
}

/// <summary>
/// UIパネルが閉じられた際のHandlerの引数
/// </summary>
public class OnUIPanelClosedEventArgs: EventArgs
{
    /// <summary>
    /// 閉じられたUIパネルの種類
    /// </summary>
    public UIPanelType PanelType { get; private set; }

    public OnUIPanelClosedEventArgs(UIPanelType panelType)
    {
        PanelType = panelType;
    }
}

/// <summary>
/// 表示もしくは閉じられたパネルの種類を示すためのenum
/// </summary>
public enum UIPanelType
{
    Message,
}

/// <summary>
/// ゲームが終了した際のHandlerの引数
/// </summary>
public  class OnGameResultEventArgs: EventArgs
{
    /// <summary>
    /// 終了時の結果
    /// </summary>
    public GameResultType ResultType { get; private set; }

    public OnGameResultEventArgs(GameResultType gameResultType)
    {
        ResultType = gameResultType;
    }
}

/// <summary>
/// ゲームが終了した際の種別を示すためのenum
/// </summary>
public enum GameResultType
{
    /// <summary>
    /// 途中で終了
    /// </summary>
    Cancel,
    Win,
    Lose,
}
#endregion