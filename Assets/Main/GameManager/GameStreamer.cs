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
    /// �Q�[���̗�����Ǘ�����N���X   
    /// �S�Ă�Controller�̏�ʂɈʒu���A�eController�̏����𐧌䂷�邽�߂̃N���X   
    /// �e����Controller������ɓ`�B����ۂ�event��������delegate���g�p����    
    /// �j�������\���̂���class��delegate�̏ꍇ��weakReference���g�p (StageobjectController�Ȃ�)   
    /// </summary>
    public class GameStreamer : MonoBehaviour
    {
        [Tooltip("MainUIController��ݒ肷��")]
        [SerializeField] private MainUIController mainUIController;
        [Tooltip("AllUnitsController��ݒ肷��")]
        [SerializeField] private AllUnitsController allUnitsController;

        /// <summary>
        /// Stage��ǂݍ��ލۂ�AsyncOperationHandle
        /// </summary>
        private AsyncOperationHandle<GameObject> stageObjectControllerObj;

        /// <summary>
        /// ���ݓǂݍ��܂�Ă���stage�̃��C���R���|�[�l���g
        /// </summary>
        public StageObjectsController StageObjectsController { get; private set; }

        GameManager gameManager;

        /// <summary>
        /// �eController���炱��class�ɓ`�B���邽�߂̃C�x���g�������Őݒ肷�� (�X��class�ł͊Ǘ����Ȃ�)
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
        public void RequestLoadStage(object sender, RequestLoadStageEventArgs args)
        {
            gameManager.CurrentStageId = args.StageId;
            stageObjectControllerObj = Addressables.InstantiateAsync(args.StageId, new Vector3(0, 0, 0), Quaternion.identity);
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
        private void OnUnitsLoaded()
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
        /// MainUIPanel��艽�炩��UI�������A���[�U�[���삪UIPanel�𗣂ꂽ�ۂɌĂяo�����
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
        /// MessagePanel���I�������ۂɌĂ΂�� AllUnitCOntroller�ɓ`���ăQ�[���J�n
        /// </summary>
        private void OnMessagePanelClosed()
        {
            gameManager.CurrentGameState = GameState.Playing;
            UserController.enableCursor = false;
            allUnitsController.OnMessagePanelClosed();
        }

        /// <summary>
        /// �Q�[���̌��ʂ��o���ۂɌĂяo�����
        /// </summary>
        private void OnGameResult(object sender, OnGameResultEventArgs args)
        {
            if (args.ResultType == GameResultType.Win || args.ResultType == GameResultType.Lose)
            {
                UserController.enableCursor = true;
                gameManager.CurrentGameState = GameState.Result;
                StartCoroutine(mainUIController.ShowResultPanel(gameManager.CurrentStageId, args.ResultType));
                gameManager.CurrentStageId = "";
                // stageObjectController�̔j��
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
/// �Q�[���̃X�e�[�W�̓ǂݍ��݂����N�G�X�g����Handler�̈���
/// </summary>
public class RequestLoadStageEventArgs: EventArgs
{
    /// <summary>
    /// �ǂݍ��ރX�e�[�W��addressable��ID
    /// </summary>
    public string StageId { get; private set; }

    public RequestLoadStageEventArgs(string stageId)
    {
        StageId = stageId;
    }
}

/// <summary>
/// UI�p�l��������ꂽ�ۂ�Handler�̈���
/// </summary>
public class OnUIPanelClosedEventArgs: EventArgs
{
    /// <summary>
    /// ����ꂽUI�p�l���̎��
    /// </summary>
    public UIPanelType PanelType { get; private set; }

    public OnUIPanelClosedEventArgs(UIPanelType panelType)
    {
        PanelType = panelType;
    }
}

/// <summary>
/// �\���������͕���ꂽ�p�l���̎�ނ��������߂�enum
/// </summary>
public enum UIPanelType
{
    Message,
}

/// <summary>
/// �Q�[�����I�������ۂ�Handler�̈���
/// </summary>
public  class OnGameResultEventArgs: EventArgs
{
    /// <summary>
    /// �I�����̌���
    /// </summary>
    public GameResultType ResultType { get; private set; }

    public OnGameResultEventArgs(GameResultType gameResultType)
    {
        ResultType = gameResultType;
    }
}

/// <summary>
/// �Q�[�����I�������ۂ̎�ʂ��������߂�enum
/// </summary>
public enum GameResultType
{
    /// <summary>
    /// �r���ŏI��
    /// </summary>
    Cancel,
    Win,
    Lose,
}
#endregion