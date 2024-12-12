using System;
using System.Collections;
using System.Collections.Generic;
using Units.AI;
using Units.TPS;
using UnityEngine;
using Cinemachine;
using static Utility;

namespace Units
{
    /// <summary>
    /// UnitのPrefabにコンポーネントとしてつけてUnitの挙動を管理する
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Tooltip("Unitの種類を設定する")]
        [SerializeField] internal UnitType unitType;


        // PlayerTargetを持つColliderとDefaultLayerのObjectのColliderは衝突しない設定になっている
        [Tooltip("UnitがPlayerである場合、敵に見つけられる判定\nClliderを持ちLayerがPlayerTargetに設定されている必要がある")]
        [SerializeField] internal Collider targetCollider;

        [Header("Debug用に使うCamera\nこれをセットするとUnitControllerとCameraUserControllerのみでTPS操作が可能になる")]
        [SerializeField] CameraUserController cameraUserController;

        /// <summary>
        /// AllUnitCon.OnUnitAction(UnitActionEventArgs e)を呼び出すためのイベント
        /// </summary>
        internal event EventHandler<UnitActionEventArgs> OnUnitAction;

        /// <summary>
        /// TPS操作を司り、userの入力を受け付ける    
        /// UnitControllerからはCameraUserControllerの要求と、AIでの自動移動の際に使われる
        /// </summary>
        public ThirdPersonUserControl TPSController { get; private set; }

        /// <summary>
        /// PlayerのUnitControllerを設定する Playerの検知などを行うため敵側に必要
        /// </summary>
        public UnitController PlayerUnitController { set => EnemyAI.playerUnitController = value; get => EnemyAI.playerUnitController;}

        public SEController SEController { private set; get; }

        /// <summary>
        /// このUnitが敵の場合、そのAIを設定する
        /// </summary>
        public EnemyAI EnemyAI { private set; get; }

        private AudioSource audioSource;
        /// <summary>
        /// 一時停止中であるか
        /// </summary>
        public bool IsPaused { get; private set; } = false;

        private void Awake()
        {
            TPSController = GetComponent<ThirdPersonUserControl>();
            audioSource = GetComponent<AudioSource>();

            TPSController.makeNoiseAction +=  MakeNoizeEvent;
            SEController = GetComponent<SEController>();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (cameraUserController != null)
            {
                Print("Debug mode is enabled. Controlling", gameObject.name);
                UserController.enableCursor = false;
                TPSController.IsTPSControllActive = true;
                print(TPSController);
                StartCoroutine(cameraUserController.SetAsFollowTarget(TPSController));
            }

            //var animator = GetComponent<Animator>();
            //if (unitType == UnitType.Player)
            //{
            //    animator.SetBool("AlongWall", true);
            //}
            //else
            //{
            //    animator.SetBool("AlongWall", false);
            //}
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
            
        }

        /// <summary>
        /// ゲームを開始する
        /// </summary>
        internal void StartGame()
        {
            EnemyAI?.StartAI();
        }

        /// <summary>
        /// Unitの操作及び行動を一時停止する
        /// </summary>
        internal void PauseUnit()
        {
            if (unitType == UnitType.Player)
            {
                TPSController.IsTPSControllActive = false;
                TPSController.PauseAnimation = true;
                
            }
            else
            {
                EnemyAI?.PauseAI();
            }
            IsPaused = true;
        }

        /// <summary>
        /// ゲームのポーズ状態を解除する
        /// </summary>
        internal void UnpauseUnit()
        {
            if (unitType == UnitType.Player)
            {
                TPSController.IsTPSControllActive = true;
                TPSController.PauseAnimation = false;
            }
            else
            {
                EnemyAI?.UnpauseAI();
            }
            IsPaused = false;
        }

        /// <summary>
        /// EnemyとしてのUnitの初期設定
        /// </summary>
        /// <param name="way">AIが辿るポイントのリスト</param>
        internal void SetUnitAsEnemy(List<StageObjects.PointAndStopTime> way)
        {
            EnemyAI = GetComponent<EnemyAI>();
            EnemyAI.playerUnitController = PlayerUnitController;
            EnemyAI.tpsController = TPSController;
            EnemyAI.OnFoundPlayer += (sender, e) => FoundYou(this);
            EnemyAI.way = way;
            TPSController.IsTPSControllActive = false;
        }

        /// <summary>
        /// 勝利してゲームが終了したことを通知する
        /// </summary>
        internal void FinishToGameAsWin(CameraUserController cameraUserController, CinemachineVirtualCamera winVirtualCamera)
        {
            if (unitType == UnitType.Player)
            {
                TPSController.IsTPSControllActive = false;
                winVirtualCamera.Priority = 1100;
                winVirtualCamera.LookAt = transform;
                StartCoroutine(TPSController.Victory());
                var bgmCon = cameraUserController.GetComponent<BGMController>();
                bgmCon.PlayWinBGM();
            }
            else
            {
                EnemyAI?.StopAI();
            }
        }

        #region アニメーションなどを含むアクションを起こす もしくはアクションを受け取る
        /// <summary>
        /// ユニットが物音を立てる
        /// </summary>
         private void MakeNoizeEvent()
        {
            // DOIT ThirdPersonCharacterでアニメーションを再生する
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.MakeNoize));
        }

        /// <summary>
        /// ユニットがtargetが発した物音を感知するか判定
        /// </summary>
        internal void SenseNoize(UnitController target)
        {
            EnemyAI.SenceNoiseAction(target);
        }

        /// <summary>
        /// FoundYouされたためプレイヤーが死亡する (ゲームオーバー)
        /// </summary>
        internal IEnumerator Killed()
        {
            TPSController.IsTPSControllActive = false;
            // ここで倒れるなどのアニメーションを再生する
            // カメラも倒れた者を撮る感じのアニメーションに変更
            StartCoroutine(TPSController.Killed());
            yield return new WaitForSeconds(4.0f);
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.Die));
        }

        /// <summary>
        /// Enemyがプレイヤーを見つけた これはAllUnitsControllerからすべてのEnemyに共有され Player側にはDieが通知される
        /// </summary>
        internal void FoundYou(UnitController sender)
        {

            IEnumerator DelayWinVoice()
            {
                yield return new WaitForSeconds(1.5f);
                SEController.EnemyWinVoiceSE();
            }

            if (sender.gameObject == this.gameObject)
            {
                SEController.EnemyrWhistleSE();
                OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.FindYou));
            }
            else
            {
                EnemyAI?.StopAI();
                StartCoroutine(DelayWinVoice());
            }
        }
        #endregion
    }

    /// <summary>
    /// Unitの種類
    /// </summary>
    [Serializable] public enum UnitType
    {
        Player,
        Enemy
    }

    public enum UnitAction
    {
        /// <summary>
        /// 発見されてゲームが終了するAction
        /// </summary>
        Die,
        /// <summary>
        /// 物音を立てて誘導している
        /// </summary>
        MakeNoize,
        /// <summary>
        /// 物音に気づいている
        /// </summary>
        SenseNoize,
        /// <summary>
        /// 敵キャラがプレイヤーを見つけた 発見されたら即ゲームオーバーなので、これを渡されたPlayerはDieを返す
        /// </summary>
        FindYou,
        /// <summary>
        /// ゴールに到達した
        /// </summary>
        Goal
    }

    /// <summary>
    /// ユニットのアクションした内容をいれるためのEventArgs
    /// </summary>
    public class UnitActionEventArgs : EventArgs
    {
        /// <summary>
        /// 行われたアクションの内容
        /// </summary>
        public UnitAction Action { get; private set; }

        /// <summary>
        /// 誰がアクションを行ったか
        /// </summary>
        public UnitController ActionFrom { get; private set; }

        /// <summary>
        /// ユニットのアクションした内容をいれるためのEventArgs
        /// </summary>
        /// <param name="action">行動の内容</param>
        /// <param name="actionFrom">誰からアクションが行われたか</param>
        public UnitActionEventArgs(UnitController actionFrom, UnitAction action)
        {
            Action = action;
            ActionFrom = actionFrom;
        }
    }
}
