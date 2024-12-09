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



// �J�����x����Ԃɍl���Ă���ȊO�͌y���������߃V���O���g���ɗ���؂������ꂪ�ł��Ă���
// �ʂɐi�s�󋵂��Ǘ�����N���X������Ă����ɏ������ڂ� GameStream�N���X���쐬���Ċe�㗬Controller��������Q�Ƃ���悤�ɂ���

public class GameManager : MonoBehaviour
{
    [Tooltip("MainUIController��ݒ肷��")]
    [SerializeField] private MainUIController mainUIController;
    [Tooltip("AllUnitsController��ݒ肷��")]
    [SerializeField] private AllUnitsController allUnitsController;
    

    private AsyncOperationHandle<GameObject> stageObjectControllerObj;

    public string CurrentStageId { get; private set; }

    /// <summary>
    /// �Q�[���̐i�s��
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
        CurrentStageId = stageId;
        stageObjectControllerObj = Addressables.InstantiateAsync(stageId, new Vector3(0, 0, 0), Quaternion.identity);
        stageObjectControllerObj.Completed += (obj) => OnStageLoaded();
    }

    /// <summary>
    /// �X�e�[�W�̓ǂݍ��݂Ɛݒu���I������ۂɌĂяo�����
    /// </summary>
    private void OnStageLoaded()
    {
        StageObjectsController = stageObjectControllerObj.Result.GetComponent<StageObjectsController>();
        allUnitsController.OnStageLoaded(StageObjectsController);
    }

    /// <summary>
    /// Units�̓ǂݍ��݂Ɛݒu���I������ۂ�AllUnitsController����Ăяo�����
    /// </summary>
    public void OnUnitsLoaded()
    {
        ShowMainMessagesPanel();
    }

    /// <summary>
    /// MainMessagesPanel�̕\��
    /// </summary>
    private void ShowMainMessagesPanel()
    {
        mainUIController.ShowMessagesPanel();
    }

    /// <summary>
    /// MessagePanel���I�� AllUnitCOntroller�ɓ`���ăQ�[���J�n
    /// </summary>
    public void OnMessagePanelClosed()
    {

        CurrentGameState = GameState.Playing;
        mainUIController.OnMessagePanelClosed();
        allUnitsController.OnMessagePanelClosed();
    }

    /// <summary>
    /// �Q�[���̌��ʂ��o���ۂ�AllUnitsController����Ăяo�����
    /// </summary>
    public void OnGameResult(bool doseWin)
    {
        
        UserController.enableCursor = true;
        CurrentGameState = GameState.Result;
        StartCoroutine(mainUIController.ShowResultPanel(CurrentStageId, doseWin));
        CurrentStageId = "";
        // stageObjectController�̔j��
        if (stageObjectControllerObj.IsValid())
        {
            Addressables.ReleaseInstance(stageObjectControllerObj);
            StageObjectsController = null;
        }
            
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

    /// <summary>
    /// ���[�U�[�̗l�X�ȓ��͂��󂯕t����
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
/// �Q�[���̌���
/// </summary>
public enum ResultState
{

}