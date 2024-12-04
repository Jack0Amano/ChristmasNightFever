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
    [Tooltip("MainUIController��ݒ肷��")]
    [SerializeField] private MainUIController mainUIController;
    [Tooltip("AllUnitsController��ݒ肷��")]
    [SerializeField] private AllUnitsController allUnitsController;

    /// <summary>
    /// ���ݓǂݍ���ł���X�e�[�W�f�[�^
    /// </summary>
    public StageObjectsController StageObjectController { get; private set; }
    private AsyncOperationHandle<GameObject> stageObjectControllerObj;

    /// <summary>
    /// �Q�[���̐i�s��
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
    // �Q�[����Start����Result�܂ł̗�����Ǘ�����

    /// <summary>
    /// Game Stream�֐��̒��ŃQ�[�����N�����Ĉ�ԍŏ��ɌĂяo�����
    /// </summary>
    private void StartGame()
    {
        mainUIController.ShowStartPanelAtAwake();
    }

    /// <summary>
    /// �X�e�[�W��Addressable����ID�œǂݍ��� MainUIController���烆�[�U�[����ɂČĂяo�����
    /// </summary>
    public void LoadStage(string stageId)
    {
        stageObjectControllerObj = Addressables.InstantiateAsync(stageId, new Vector3(0, 0, 0), Quaternion.identity);
        stageObjectControllerObj.Completed += (obj) => OnStageLoaded();
    }

    /// <summary>
    /// �X�e�[�W�̓ǂݍ��݂Ɛݒu���I������ۂɌĂяo�����
    /// </summary>
    private void OnStageLoaded()
    {
        StageObjectController = stageObjectControllerObj.Result.GetComponent<StageObjectsController>();
        allUnitsController.OnStageLoaded(StageObjectController);
    }

    /// <summary>
    /// Units�̓ǂݍ��݂Ɛݒu���I������ۂ�AllUnitsController����Ăяo�����
    /// </summary>
    public void OnUnitsLoaded()
    {
        CurrentGameState = GameState.Playing;
    }

    /// <summary>
    /// �Q�[���̌��ʂ��o���ۂ�AllUnitsController����Ăяo�����
    /// </summary>
    public void OnGameResult()
    {
    }

    /// <summary>
    /// �Q�[�����I������ StartPanel�܂���resultPanel��ExitButton����Ăяo�����
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