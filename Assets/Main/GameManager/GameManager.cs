using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using StageObjects;
using MainUI;
using Units;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [Tooltip("MainUIController��ݒ肷��")]
    [SerializeField] private MainUIController mainUIController;
    [Tooltip("AllUnitsController��ݒ肷��")]
    [SerializeField] private AllUnitsController allUnitsController;

    /// <summary>
    /// ���ݓǂݍ���ł���X�e�[�W�f�[�^
    /// </summary>
    public StageObjectsController StageObjectController { get; private set; }

    /// <summary>
    /// �Q�[���̐i�s��
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
    // �Q�[����Start����Result�܂ł̗�����Ǘ�����

    /// <summary>
    /// Game Stream�֐��̒��ŃQ�[�����N�����Ĉ�ԍŏ��ɌĂяo�����
    /// </summary>
    private void StartGame()
    {
        mainUIController.ShowStartPanel();
    }

    /// <summary>
    /// �X�e�[�W��Addressable����ID�œǂݍ���
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
    /// �X�e�[�W�̓ǂݍ��݂Ɛݒu���I������ۂɌĂяo�����
    /// </summary>
    private void OnStageLoaded()
    {
    }

    /// <summary>
    /// Units�̓ǂݍ��݂Ɛݒu���I������ۂ�AllUnitsController����Ăяo�����
    /// </summary>
    public void OnUnitsLoaded()
    {
    }

    /// <summary>
    /// �Q�[���̌��ʂ��o���ۂ�AllUnitsController����Ăяo�����
    /// </summary>
    public void OnGameResult()
    {
    }

    #endregion

}

//�V���O���g����MonoBehaviour�̊��N���X
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