using System;
using System.Collections;
using System.Collections.Generic;
using Units.Icon;
using Units.TPS;
using UnityEngine;
using UnityEngine.AI;

namespace Units.AI
{
    /// <summary>
    /// 敵のAIを制御する
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Tooltip("プレイヤーを検知できる最大距離")]
        [SerializeField] private float findPlayerDistance = 10f;
        [Tooltip("プレイヤーを検知できる最大角度")]
        [SerializeField] private float findPlayerAngle = 45f;
        [Tooltip("プレイヤーを見つける目の方向と位置のTransform")]
        [SerializeField] private Transform eyesTransform;
        [Tooltip("視線のRayの対象となるレイヤーマスクのリスト")]
        [SerializeField] private LayerMask rayLayerMask;
        [Tooltip("発見するための距離とDeltaFindOutLevelの関係のカーブ")]
        [SerializeField] private AnimationCurve detectionDistanceCurve;
        [Tooltip("何秒ごとにDeltaFindOutLevelを計算するか")]
        [SerializeField] private float detectUnitTick = 0.3f;

        [Tooltip("発見時のHeadUpIcon")]
        [SerializeField] private HeadUP headUP;

        [Tooltip("Debug用に目標地点までの線を描画するか")]
        [SerializeField] private bool debugDrawAimWalkingLine = false;

        /// <summary>
        /// プレイヤーをどの程度見つけているか 1になったら完全に発見
        /// </summary>
        internal float FindOutLevel { private set; get; } = 0;

        /// <summary>
        /// プレイヤーを発見したときにUnitController.FoundYouを呼び出すためのEvent
        /// </summary>
        internal event EventHandler<EventArgs> OnFoundPlayer;

        /// <summary>
        /// PlayerのUnitControllerを設定する Playerの検知などを行うため敵側に必要
        /// </summary>
        internal UnitController playerUnitController;

        internal ThirdPersonUserControl TPSController;

        /// <summary>
        /// 前回TryFindPlayerを呼び出しでからの経過ミリ秒
        /// </summary>
        float lastTryFindPlayerSec = 0;

        private NavMeshAgent navMeshAgent;
        private NavMeshObstacle navMeshObstacle;

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
            // FixedUpdateで呼び出す理由はCorutine内ループでの呼び出しだとObjectが消えたときなど後処理が面倒なため
            // FixedUpdateが遅延する場合はCoroutine内に変更する
            // 距離と角度で脚切りするし、raycastを飛ばすのもdetectUnitTick毎だが近くに大量の敵がいる場合は負荷がかかる可能性がある
            lastTryFindPlayerSec += Time.fixedDeltaTime;
            if (lastTryFindPlayerSec > detectUnitTick)
            {
                TryFindPlayer();
                lastTryFindPlayerSec = 0;
            }
        }

        #region Move with AI
        // navmeshagentを使って移動する場合は　NavMeshAgentのComponentを追加しておく

        /// <summary>
        /// NavMeshを使用してLocationに移動
        /// </summary>
        /// <param name="location"></param>
        public IEnumerator MoveTo(Vector3 location)
        {
            // NavMeshObstacleは他のUnitとの衝突を避けるために使う今回はなしでいい
            //navMeshObstacle.enabled = false;
            //navMeshAgent.enabled = true;

            // navMeshAgent.SetDestination(location);

            location.y = this.transform.position.y;
            yield return StartCoroutine(TPSController.AutoMove(location, navMeshAgent, debugDrawAimWalkingLine));

            //navMeshAgent.isStopped = true;

            //navMeshAgent.enabled = false;

            yield return true;
        }
        #endregion

        #region Methods try to find player
        /// <summary>
        /// Enemyがプレイヤーを見つけようとするループ
        /// </summary>
        private void TryFindPlayer()
        {
            if (FindOutLevel <= 1)
            {
                var dist = GetDistanceIfPlayerInSight();
                FindOutLevel += GetDeltaLevelToFindOut(dist);
                FindOutLevel = Mathf.Clamp(FindOutLevel, 0, 1);
                headUP.SetFindOutLevel(FindOutLevel);
                if (FindOutLevel == 1)
                {
                   // プレイヤーを見つけた
                    OnFoundPlayer?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// playerUnitControllerが直線距離でfindPlayerDistance以内であり、視界に入っている場合距離を取得
        /// </summary>
        /// <returns>視界に入っていない場合は-1</returns>
        private float GetDistanceIfPlayerInSight()
        {
            var targetPosition = playerUnitController.targetCollider.transform.position;
            // 距離がfindPlayerDistance以上で十分遠かったら見つけられない
            var dist = Vector3.Distance(targetPosition, eyesTransform.position);
            if (dist > findPlayerDistance)
            {
                return -1;
            }
            // 視界に入っているかを判定
            var angle = Vector3.Angle(eyesTransform.forward, targetPosition - eyesTransform.position);
            if (angle > findPlayerAngle)
            {
                return -1;
            }
            // 視線が通っているrayを飛ばしてプレイヤーが見えるかを判定
            // Rayの対象になるレイヤーは 障害物の CoverObjectとPlayerTargetでCoverObjectに当たらずにPlayerTargetに当たるかを判定
            // レイヤーマスクはInspectorのrayLayerMaskで指定
            RaycastHit hit;
            if (Physics.Raycast(eyesTransform.position, targetPosition - eyesTransform.position, out hit, findPlayerDistance, rayLayerMask))
            {
                if (hit.collider == playerUnitController.targetCollider)
                {
                    return hit.distance;
                }
            }
            
            return -1;

        }

        /// <summary>
        /// watcherからtargetへの発見レベルを取得する
        /// </summary>
        /// <param name="distance">距離 見えていないなら=-1</param>
        /// <returns></returns>
        private float GetDeltaLevelToFindOut(float distance)
        {
            // 発見状態から何秒で見失うか
            const float ForgetTime = 5;

            if (distance != -1)
            {
                //　視界内に存在している
                return detectionDistanceCurve.Evaluate(distance);
            }
            else
            {
                var forgetTick = ForgetTime / detectUnitTick;
                var forgetValueEachTick = -1 / forgetTick;
                return forgetValueEachTick;
            }
        }

        #endregion
   
        
    }
}