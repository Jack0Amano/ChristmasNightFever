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



// 開発速度を一番に考えてそれ以外は軽視したためシングルトンに頼り切った流れができている
// 別に進行状況を管理するクラスを作ってそこに処理を移す GameStreamクラスを作成して各上流Controllerがそれを参照するようにする

public class GameManager : MonoBehaviour
{
    [Tooltip("MainUIControllerを設定する")]
    [SerializeField] private MainUIController mainUIController;
    [Tooltip("AllUnitsControllerを設定する")]
    [SerializeField] private AllUnitsController allUnitsController;
    

    private AsyncOperationHandle<GameObject> stageObjectControllerObj;

    public string CurrentStageId { get; private set; }

    /// <summary>
    /// ゲームの進行状況
    /// </summary>
    public GameState CurrentGameState { get; private set; }

    public static GameManager Instance { get; private set; }

    public StageObjectsController StageObjectsController { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentGameState = GameState.Title;
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        UserControl();
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
    public void LoadStage(string stageId)
    {
        CurrentStageId = stageId;
        stageObjectControllerObj = Addressables.InstantiateAsync(stageId, new Vector3(0, 0, 0), Quaternion.identity);
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
    public void OnUnitsLoaded()
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
    /// MessagePanelが終了 AllUnitCOntrollerに伝えてゲーム開始
    /// </summary>
    public void OnMessagePanelClosed()
    {

        CurrentGameState = GameState.Playing;
        mainUIController.OnMessagePanelClosed();
        allUnitsController.OnMessagePanelClosed();
    }

    /// <summary>
    /// ゲームの結果が出た際にAllUnitsControllerから呼び出される
    /// </summary>
    public void OnGameResult(bool doseWin)
    {
        
        UserController.enableCursor = true;
        CurrentGameState = GameState.Result;
        StartCoroutine(mainUIController.ShowResultPanel(CurrentStageId, doseWin));
        CurrentStageId = "";
        // stageObjectControllerの破棄
        if (stageObjectControllerObj.IsValid())
        {
            Addressables.ReleaseInstance(stageObjectControllerObj);
            StageObjectsController = null;
        }
            
    }

    /// <summary>
    /// ゲームを終了する StartPanelまたはresultPanelのExitButtonから呼び出される
    /// </summary>
    public void EndGame()
    {
        if (stageObjectControllerObj.IsValid())
        {
            Addressables.ReleaseInstance(stageObjectControllerObj);
        }
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    /// <summary>
    /// ユーザーの様々な入力を受け付ける
    /// </summary>
    private void UserControl()
    {
        //if (UserController.KeyCodeEscape)
        //{
        //    if (CurrentGameState == GameState.Playing)
        //    {
        //        mainUIController.ShowStartPanelAtAwake();
        //        CurrentGameState = GameState.Title;
        //    }
        //    else if (CurrentGameState == GameState.Title)
        //    {
        //        EndGame();
        //    }
        //}
    }

}


public enum GameState
{
    Title,
    Playing,
    Result,
}

/// <summary>
/// ゲームの結果
/// </summary>
public enum ResultState
{

}