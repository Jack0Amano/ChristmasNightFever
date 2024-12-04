using System;
using System.Collections;
using System.Collections.Generic;
using Units.AI;
using Units.TPS;
using UnityEngine;
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
        [SerializeField] CameraUserController cameraUserController;

        // PlayerTargetを持つColliderとDefaultLayerのObjectのColliderは衝突しない設定になっている
        [Tooltip("UnitがPlayerである場合、敵に見つけられる判定\nClliderを持ちLayerがPlayerTargetに設定されている必要がある")]
        [SerializeField] internal Collider targetCollider;

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
        public UnitController PlayerUnitController { set => enemyAI.playerUnitController = value; get => enemyAI.playerUnitController;}

        /// <summary>
        /// このUnitが敵の場合、そのAIを設定する
        /// </summary>
        EnemyAI enemyAI;

        private void Awake()
        {
            TPSController = GetComponent<ThirdPersonUserControl>();
            if (unitType == UnitType.Enemy)
            {
                enemyAI = GetComponent<EnemyAI>();
                enemyAI.playerUnitController = PlayerUnitController;
                enemyAI.OnFoundPlayer += (sender, e) => FoundYou();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
        }

        #region アニメーションなどを含むアクションを起こす
        /// <summary>
        /// ユニットが物音を立てる
        /// </summary>
        internal void MakeNoize()
        {
            // DOIT ThirdPersonCharacterでアニメーションを再生する
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.MakeNoize));
        }

        /// <summary>
        /// ユニットがtargetが発した物音を感知するか判定
        /// </summary>
        internal void SenseNoize(UnitController target)
        {
            // DOIT ThirdPersonCharacterでアニメーションを再生し、AIに通知する
            
        }

        /// <summary>
        /// FoundYouされたためプレイヤーが死亡する (ゲームオーバー)
        /// </summary>
        internal IEnumerator Killed()
        {
            TPSController.IsTPSControllActive = false;
            yield return new WaitForSeconds(3.0f);

            // ここで倒れるなどのアニメーションを再生する
            // カメラも倒れた者を撮る感じのアニメーションに変更

            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.Die));
        }

        /// <summary>
        /// Enemyがプレイヤーを見つけた これはAllUnitsControllerからすべてのEnemyに共有され Player側にはDieが通知される
        /// </summary>
        internal void FoundYou()
        {
            OnUnitAction?.Invoke(this, new UnitActionEventArgs(this, UnitAction.FindYou));
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
