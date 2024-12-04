using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using StageObjects;
using MainUI;
using Units;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Utility;

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
        var stageObjectController = stageObjectControllerObj.Result.GetComponent<StageObjectsController>();
        allUnitsController.OnStageLoaded(stageObjectController);
    }

    /// <summary>
    /// Unitsの読み込みと設置が終わった際にAllUnitsControllerから呼び出される
    /// </summary>
    public void OnUnitsLoaded()
    {
        CurrentGameState = GameState.Playing;
        UserController.enableCursor = false;
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
        Addressables.ReleaseInstance(stageObjectControllerObj);
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