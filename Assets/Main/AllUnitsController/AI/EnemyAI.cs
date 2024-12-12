using System;
using System.Collections;
using System.Collections.Generic;
using Units.Icon;
using Units.TPS;
using UnityEngine;
using UnityEngine.AI;
using static Utility;
using static WaitForSecondsExtensions;

namespace Units.AI
{
    /// <summary>
    /// 敵のAIを制御する
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
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
        [Tooltip("FindOutLevelがどれだけ異常になったら警戒に移行するか")]
        [SerializeField] private float alertLevel = 0.5f;

        [SerializeField] private float senceNoizeDistance = 15f;

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

        internal ThirdPersonUserControl tpsController;

        /// <summary>
        /// 前回TryFindPlayerを呼び出しでからの経過ミリ秒
        /// </summary>
        float lastTryFindPlayerSec = 0;

        private NavMeshAgent navMeshAgent;
        private NavMeshObstacle navMeshObstacle;

        /// <summary>
        /// AIが移動する経路 EditorWaysで指定されてUnitの設置時にAllUnitsControllerによってSetUnitAsEnemyで設定される
        /// </summary>
        internal List<StageObjects.PointAndStopTime> way;

        /// <summary>
        /// 現在のwayのIndex 0からスタート　向かおうとした瞬間にIndexが加算されていく
        /// </summary>
        private int currentWayIndex = 0;

        private EnemyAIMoveState moveState = EnemyAIMoveState.IdleMainRoutine;

        /// <summary>
        /// 途中でポーズやcancelが行えるwaitforsecondsStoppableに値渡しするためのトリガー
        /// 注意点として別メソッドが不用意にcancelすると現在waitforsecondsStoppableが動いているコルーチンもキャンセルされる
        /// </summary>
        private WaitForSecondsStopableTrigger waitForSecondsTrigger = new WaitForSecondsStopableTrigger();
        /// <summary>
        /// AIがポーズ中であるか
        /// </summary>
        public bool IsPause
        {
            get => waitForSecondsTrigger.value == WaitForSecondsStopableTriggerEnum.Pause;
            private set
            {
                if (waitForSecondsTrigger.value != WaitForSecondsStopableTriggerEnum.Cancel)
                {
                    waitForSecondsTrigger.value = value ? WaitForSecondsStopableTriggerEnum.Pause : WaitForSecondsStopableTriggerEnum.None;
                }
                else
                {
                    // キャンセルされてAIが終了しているのにポーズを行おうとする不正な操作
                    PrintWarning("AI is already finished:", gameObject.name);
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //headUP.SetFindOutLevel(0.7f);
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.CurrentGameState != GameState.Playing)
                return;

            if (moveState == EnemyAIMoveState.Finish || IsPause)
                return;

            // SetUnitAsEnemy(List<StageObjects.PointAndStopTime> way)を呼び出してEnemyAIを設定していない場合は何もしない
            if (playerUnitController == null)
                return;

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

        /// <summary>
        /// targetは発した物音を感知する
        /// </summary>
        /// <param name="target"></param>
        internal void SenceNoiseAction(UnitController target)
        {
            var dist = Vector3.Distance(target.transform.position, this.transform.position);
            if (dist < senceNoizeDistance)
            {
                // 物音を感知した
                moveState = EnemyAIMoveState.SenseNoize;
            }
        }

        /// <summary>
        /// Unitの移動及び発見ルーチンを停止する
        /// </summary>
        internal void StopAI()
        {
           waitForSecondsTrigger.value = WaitForSecondsStopableTriggerEnum.Cancel;
           moveState = EnemyAIMoveState.Finish;
        }

        /// <summary>
        /// Unitの移動及び発見ルーチンを一時停止する
        /// </summary>
        internal void PauseAI()
        {
            IsPause = true;
            tpsController.PauseAnimation = true;
        }

        /// <summary>
        /// Unitの移動及び発見ルーチンを再開する
        /// </summary>
        internal void UnpauseAI()
        {
            IsPause = false;
            tpsController .PauseAnimation = false;
        }

        #region Move with AI
        // navmeshagentを使って移動する場合は　NavMeshAgentのComponentを追加しておく

        /// <summary>
        /// AIの移動を開始する
        /// </summary>
        public void StartAI()
        {
            waitForSecondsTrigger.value = WaitForSecondsStopableTriggerEnum.None;
           // ここでAIのループを開始する
            StartCoroutine(StopAndFindAtCurrentPlace_MainRoutine());
        }

        #region Type routine
        // この中のルーチンはNodeみたいに作動してSituationCheckerで状況が変わったらキャンセルされる
        // Routineと名がついている2つがメインの経路を決めるルーチンで
        // subRoutineがSituationCheckerで状況が変わった時に行われるルーチン
        // (SubRoutine内でもキャンセルと他のSubRoutineへの移行を行う場合もある)

        /// <summary>
        /// currentWayIndexの次のwayに移動する
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator MoveToNextWay_MainRoutine(float delay=0)
        {
            moveState = EnemyAIMoveState.MoveMainRoutine;
            if (currentWayIndex + 1 >= way.Count )
            {
                currentWayIndex = -1;
            }

            if (delay > 0)
            {
                yield return WaitForSecondsStopable(delay, waitForSecondsTrigger);
            }

            currentWayIndex++;
            var nextWay = way[currentWayIndex];
            // ここでFindOutLevelを監視しながら移動が完了するまで待つ

            StartCoroutine(MoveTo(nextWay.pointTransform.position, nextWay.runToThisPoint));
            var startFindOutLevel = FindOutLevel;
            yield return WaitForSecondsStopable(1f, waitForSecondsTrigger);
            while(tpsController.IsAutoMoving && tpsController.IsMoving)
            {
                if (IsPause)
                {
                    yield return null;
                    continue;
                }

                // SituationCheckerで状況が変わっていないかを確認
                // resultがfalseならば移動をキャンセル、SituationCheckerが新しい状況に移動するためのアニメーションを開始する
                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    tpsController.CancelAutoMoving();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // 移動が完了したため移動してきた現在のwayでの停止時間を待つコルーチンに移動
            StartCoroutine(StopAndFindAtCurrentPlace_MainRoutine());
        }

        /// <summary>
        /// currentWaiIndexがstopTimeを設定している場合、停止して辺りの探索を行う
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator StopAndFindAtCurrentPlace_MainRoutine(float delay=0)
        {
            moveState = EnemyAIMoveState.IdleMainRoutine;
            var currentWay = way[currentWayIndex];

            if (delay > 0)
            {
                yield return WaitForSecondsStopable(delay, waitForSecondsTrigger);
            }

            if (currentWay.stopTime > 0)
            {
                // 一度見回すアニメーションを再生する
                StartCoroutine(tpsController.Searching());
                var searchDuration = 0f;
                while (searchDuration < currentWay.stopTime)
                {
                    if (IsPause)
                    {
                        yield return null;
                        continue;
                    }

                    var result = SituationChecker(moveState, FindOutLevel);
                    if (!result)
                    {
                        // 状況が変わったため探索をキャンセル
                        // Stop find animation
                        yield break;
                    }
                    yield return new WaitForSeconds(0.1f);
                    searchDuration += 0.1f;
                }
            }
            else if (currentWay.stopTime < 0)
            {
                // -1の場合はその場で探索し続ける
                var lastSearchiAnimationTime = 0f;
                var nextSearchInterval =UnityEngine.Random.Range(9, 15);
                while (true)
                {
                    if (IsPause)
                    {
                        yield return null;
                        continue;
                    }

                    // おおよそ10秒ごとだがランダムにSearchingを再生する
                    if (Time.time - lastSearchiAnimationTime > nextSearchInterval)
                    {
                        StartCoroutine(tpsController.Searching());
                        lastSearchiAnimationTime = Time.time;
                        nextSearchInterval = UnityEngine.Random.Range(5, 15);
                    }

                    var result = SituationChecker(moveState, FindOutLevel);
                    if (!result)
                    {
                        // 状況が変わったため探索をキャンセル
                        yield break;
                    }
                    // ここでFindOutLevelを監視しながら探索が完了するまで待つ
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // ここで探索が完了したため次のwayに移動する
            StartCoroutine(MoveToNextWay_MainRoutine());
        }

        /// <summary>
        /// 不審な場所を発見し、その場所に向かって移動する
        /// </summary>
        /// <param name="position"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator MoveToAlertPosition_SubRoutine(Vector3 position, float delay = 0)
        {
            moveState = EnemyAIMoveState.MoveToSearch;
            if (delay > 0)
            {
                yield return WaitForSecondsStopable(delay, waitForSecondsTrigger);
            }

            StartCoroutine(MoveTo(position, true));
            var startFindOutLevel = FindOutLevel;
            yield return new WaitForSeconds(0.5f);
            while (tpsController.IsAutoMoving)
            {
                if (IsPause)
                {
                    yield return null;
                    continue;
                }

                // SituationCheckerで状況が変わっていないかを確認
                // resultがfalseならば移動をキャンセル、SituationCheckerが新しい状況に移動するためのアニメーションを開始する
                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    tpsController.CancelAutoMoving();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // AlertPositionに到達したため停止して探索を行う
            StartCoroutine(SearchAlertArea_SubRoutine());
            
        }

        /// <summary>
        /// 不審な場所に到達した後、その場所を探索する
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator SearchAlertArea_SubRoutine(float delay = 0)
        {
            moveState = EnemyAIMoveState.SearchingIdle;
            if (delay > 0)
            {
                yield return WaitForSecondsStopable(delay, waitForSecondsTrigger);
            }

            // 何秒で探索を終了するか
            const float stopSearchTime = 10f;

            // TODO 探索アニメーションを再生する
            // 探索が完了したらBackSubRoutineに移行する
            StartCoroutine(tpsController.Searching());
            float searchDuration = 0;
            var startFindOutLevel = FindOutLevel;
            while (searchDuration < stopSearchTime)
            {
                if (IsPause)
                {
                    yield return null;
                    continue;
                }

                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    // 状況が変わったため探索をキャンセル
                    // Stop find animation
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
                searchDuration += 0.1f;
            }

            // 捜索しても何も見つからなかったためBackSubRoutineに移行する
            StartCoroutine(BackToRoutineWay_SubRoutine());
        }

        /// <summary>
        /// CurrentWayIndexのwayに戻る
        /// </summary>
        private IEnumerator BackToRoutineWay_SubRoutine(float delay=0)
        {
            moveState = EnemyAIMoveState.BackToMainRoutine;
            if (delay > 0)
            {
                yield return WaitForSecondsStopable(delay, waitForSecondsTrigger);
            }

            var currentWay = way[currentWayIndex];
            StartCoroutine(MoveTo(currentWay.pointTransform.position, false));
            var startFindOutLevel = FindOutLevel;
            yield return new WaitForSeconds(0.5f);
            while (tpsController.IsAutoMoving)
            {
                if (IsPause)
                {
                    yield return null;
                    continue;
                }

                var result = SituationChecker(moveState, startFindOutLevel);
                if (!result)
                {
                    tpsController.CancelAutoMoving();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // 何も発見できずroutineに戻ってきた
            StartCoroutine(StopAndFindAtCurrentPlace_MainRoutine());
        }

        /// <summary>
        /// 物音を感知した際に割り込まれるサブルーチン MoveToAlertPosition_SubRoutineに自動で移行する
        /// </summary>
        /// <returns></returns>
        private IEnumerator SenceNoise_SubRoutine()
        {
            // TODO ここで物音を感知するアニメーションを再生する
            // 物音を感知するアニメーションが終わったらBackToMainRoutineに移行する
            moveState = EnemyAIMoveState.MoveToSearch;
            yield return WaitForSecondsStopable(1f, waitForSecondsTrigger);
            headUP.ShowQuestion(0, true);
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(tpsController.RotateTo(playerUnitController.targetCollider.transform.position));

            StartCoroutine(MoveToAlertPosition_SubRoutine(playerUnitController.targetCollider.transform.position, 1f));
        }

        #endregion


        /// <summary>
        /// 各種のアニメーションの再生中に、状況が変わっていないかを確認し
        /// 状況が変わっていたらアニメーションをキャンセルして新しいアニメーションに移動するためのチェッカー
        /// </summary>
        private bool SituationChecker(EnemyAIMoveState startState, float startFindOutLevel)
        {
            // ルーチン施行前の割り込みを確認
            if (FindOutLevel == 1)
            {
                // プレイヤーを見つけたためすべてのループを終了して一目散に駆け寄る
                StartCoroutine(MoveTo(playerUnitController.targetCollider.transform.position, true));
                return false;
            }
            else if (moveState == EnemyAIMoveState.Finish)
            {
                // システムからの終了処理
                // Falseを返すとSituationCheckerを呼び出したコルーチンも終了し、
                // 新たにAIの別コルーチンも開始されない
                return false;
            }
            else if (moveState == EnemyAIMoveState.SenseNoize)
            {
                // 物音を感知した
                StartCoroutine(SenceNoise_SubRoutine());
                return false;
            }

            // 警戒状態にある
            var alert = FindOutLevel > alertLevel;
            if (alert)
            {
                var moveTo = playerUnitController.targetCollider.transform.position;
                if (startFindOutLevel > alertLevel)
                {

                    // 警戒状態がいまだ続いている
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            // Falseを返すとSituationCheckerを呼び出したコルーチンも終了し、
                            // 新たにAIの別コルーチンも開始されない
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            // Stateが警戒に入っているのにIdleの場合はIdleをキャンセルしてmoveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveMainRoutine:
                            // Stateが警戒に入っているのにMoveMainの場合はMoveMainをキャンセルしてmoveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveToSearch:
                            // 現在そこに向かっている途中
                            break;
                        case EnemyAIMoveState.SearchingIdle:
                            // 現在そこで探索中 探索してもない場合早々に諦める
                            break;
                        case EnemyAIMoveState.BackToMainRoutine:
                            // 帰ろうとしたのに警戒状態になった
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                            
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // 警戒状態になった
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            // Falseを返すとSituationCheckerを呼び出したコルーチンも終了し、
                            // 新たにAIの別コルーチンも開始されない
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            // 警戒状態になったのでIdleをキャンセルしてmoveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveMainRoutine:
                            // 警戒状態になったのでMoveMainをキャンセルしてmoveto
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        case EnemyAIMoveState.MoveToSearch:
                            // 現在そこに向かっている途中
                            break;
                        case EnemyAIMoveState.SearchingIdle:
                            // 現在そこで探索中 探索してもない場合早々に諦める
                            break;
                        case EnemyAIMoveState.BackToMainRoutine:
                            StartCoroutine(MoveToAlertPosition_SubRoutine(moveTo, 2));
                            return false;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                if (startFindOutLevel > alertLevel)
                {
                    // 警戒状態から抜けた
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveToSearch:
                            // 警戒状態から抜けたのでMoveToSearchをキャンセルしてBackToMainRoutineに移行
                            StartCoroutine(BackToRoutineWay_SubRoutine(1));
                            return false;
                        case EnemyAIMoveState.SearchingIdle:
                            break;
                        case EnemyAIMoveState.BackToMainRoutine:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // 警戒状態ではないが、Stateが変わっているかを確認 特にFinishだと強制終了
                    switch (startState)
                    {
                        case EnemyAIMoveState.Finish:
                            // Falseを返すとSituationCheckerを呼び出したコルーチンも終了し、
                            // 新たにAIの別コルーチンも開始されない
                            return false;
                        case EnemyAIMoveState.IdleMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveMainRoutine:
                            break;
                        case EnemyAIMoveState.MoveToSearch:
                            break;
                        case EnemyAIMoveState.SearchingIdle:
                            StartCoroutine(BackToRoutineWay_SubRoutine(2));
                            return false;
                        case EnemyAIMoveState.BackToMainRoutine:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// NavMeshを使用してLocationに移動
        /// </summary>
        /// <param name="location"></param>
        public IEnumerator MoveTo(Vector3 location, bool run)
        {
            location.y = this.transform.position.y;
            yield return StartCoroutine(tpsController.AutoMove(location, navMeshAgent, run, debug: debugDrawAimWalkingLine));
        }
        #endregion

        #region Methods try to find player
        /// <summary>
        /// Enemyがプレイヤーを見つけようとするループ
        /// </summary>
        private void TryFindPlayer()
        {
            if (FindOutLevel < 1)
            {
                var dist = GetDistanceIfPlayerInSight();
                // DOIT deltalevelでalterLevelを超えている場合これが減っていく状態では5秒alterLevelを保持
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

        // TODO 発見フェーズをJobsystemなどの並列処理で軽量化する
        // 発見は必ずしもタイミングが同じである必要はないため、Jobsystemで並列処理を行い軽量化出来ると思う

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
                var x = distance / findPlayerDistance;
                //　視界内に存在している
                return detectionDistanceCurve.Evaluate(x);
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

    /// <summary>
    ///　敵のAIが現在どの様な行動を取っているか
    /// </summary>
    enum EnemyAIMoveState
    {
        /// <summary>
        /// ループを完全終了
        /// </summary>
        Finish,
        /// <summary>
        /// 停止中
        /// </summary>
        IdleMainRoutine,
        /// <summary>
        /// Wayに沿って移動中
        /// </summary>
        MoveMainRoutine,
        /// <summary>
        /// 不審な場所を発見しそこに向かっている
        /// </summary>
        MoveToSearch,
        /// <summary>
        /// 不審な場所で探索中
        /// </summary>
        SearchingIdle,
        /// <summary>
        /// 不審な場所の探索から帰ってきている
        /// </summary>
        BackToMainRoutine,
        /// <summary>
        /// ノイズを耳にした
        /// </summary>
        SenseNoize,
    }
}