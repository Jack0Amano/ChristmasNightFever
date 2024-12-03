using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using StageObjects;
using MainUI;
using Units;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [Tooltip("MainUIControllerを設定する")]
    [SerializeField] private MainUIController mainUIController;
    [Tooltip("AllUnitsControllerを設定する")]
    [SerializeField] private AllUnitsController allUnitsController;

    /// <summary>
    /// 現在読み込んでいるステージデータ
    /// </summary>
    public StageObjectsController StageObjectController { get; private set; }

    /// <summary>
    /// ゲームの進行状況
    /// </summary>
    public GameState CurrentGameState { get; private set; }

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
        mainUIController.ShowStartPanel();
    }

    /// <summary>
    /// ステージをAddressableからIDで読み込む
    /// </summary>
    public void LoadStage(int stageId)
    {
        var handle = Addressables.LoadAssetAsync<StageObjectsController>($"StageData_{stageId}");
        handle.Completed += op =>
        {
            StageObjectController = op.Result;
            GameObject.Instantiate(StageObjectController);
            OnStageLoaded();
        };
    }

    /// <summary>
    /// ステージの読み込みと設置が終わった際に呼び出される
    /// </summary>
    private void OnStageLoaded()
    {
    }

    /// <summary>
    /// Unitsの読み込みと設置が終わった際にAllUnitsControllerから呼び出される
    /// </summary>
    public void OnUnitsLoaded()
    {
    }

    /// <summary>
    /// ゲームの結果が出た際にAllUnitsControllerから呼び出される
    /// </summary>
    public void OnGameResult()
    {
    }

    #endregion

}

//シングルトンなMonoBehaviourの基底クラス
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = (T)FindObjectOfType(typeof(T));

            if (instance == null)
            {
                Debug.LogWarning(typeof(T) + " is nothing");
            }

            return instance;
        }
    }

    public static T InstanceNullable
    {
        get
        {
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError(typeof(T) + " is multiple created", this);
            return;
        }

        instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}

public enum GameState
{
    Title,
    Game,
    Result,
}