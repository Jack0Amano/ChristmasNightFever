﻿using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using static Utility;
using Cinemachine.Utility;
using DG.Tweening;
using UnityEditor.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace Units.TPS
{
    /// <summary>
    /// Animatorに直接パラメーターを渡し制御する唯一のクラス
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonCharacter : MonoBehaviour
    {
        [SerializeField] float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StationaryTurnSpeed = 180;
        [SerializeField] float m_JumpPower = 12f;
        [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;
        [SerializeField] float aimSensitivity = 3f;
        //[Tooltip("BodyTrigger Layerに反応する　他のUnitとの接触感知")]
        //[SerializeField] internal BodyTrigger BodyTrigger;

        [SerializeField] float WalkSpeed = 0.5f;
        [SerializeField] float DashSpeed = 1.2f;

        [Header("Along wall parameters")]
        [Tooltip("壁に沿う形で移動する際 壁のNormalと移動方向の最大角度\n これより上だと沿った移動を行わない")]
        [SerializeField] float FollowingWallMaxDegree = 165;
        [Tooltip("壁に沿う形で移動する際 壁のNormalと移動方向の最小角度\n これより下だと壁から離れる")]
        [SerializeField] float FollowingWallMinDegree = 35;
        [Tooltip("壁に沿って移動する際の移動開始カーブ")]
        [SerializeField] AnimationCurve MoveFollowingWallAnimationCurve;
        [Tooltip("壁から離れる方向の力をどれだけの時間与えれば離れるか")]
        [SerializeField] float LeaveFromWallSeconds = 0.4f;

        /// <summary>
        /// UnitがWallに張り付いておりここで移動を開始した時間
        /// </summary>
        private DateTime startToMoveTimeOnFollowingWall;
        /// <summary>
        /// Wallに張り付き移動をした最後のOnMoveAnimatorCount
        /// </summary>
        private uint lastMoveFollowingWallCount = 0;
        /// <summary>
        /// 壁に沿って移動する際の移動開始カーブのカーブ経過時間 MoveAlongWallAnimationCurve.length.time
        /// </summary>
        private double moveAlongWallAnimationTotalDuration;
        /// <summary>
        /// Animatorの移動の継続カウント 移動=0になったときにOnMoveAnimatorCount=0になる
        /// </summary>
        private uint onMoveAnimatorCount = 0;
        /// <summary>
        /// Wallから離れるLeaveMoveの最後のOnMoveAnimatorCount
        /// </summary>
        private uint lastLeaveFromWallCount = 0;
        /// <summary>
        /// Leave動作を開始した時間
        /// </summary>
        private DateTime startToLeaveFromWallTime;

        Sequence alongWallRotationAnimation;

        public bool IsDashMode { private set; get; }

        new Rigidbody rigidbody;
        Animator animator;
        // bool m_IsGrounded;
        float origGroundCheckDistance;
        const float K_HALF = 0.5f;
        float turnAmount;
        float forwardAmount;
        Vector3 groundNormal;
        float capsuleHeight;
        Vector3 capsuleCenter;
        CapsuleCollider capsule;
        public bool Crouching { private set; get; }
        bool aiming = false;

        /// <summary>
        /// Animatorのレイヤー
        /// </summary>
        const int BASE_LAYER = 0;
        const int UPPER_LAYER = 1;
        const int UPPER_LAYER_WITH_MASK = 2;
        const int OVERLAY_LAYER = 3;

        /// <summary>
        /// UnitがAlongWallに接触しておりカバー状態である
        /// </summary>
        public bool IsFollowingWallMode { get => FollowWallObject != null; }
        /// <summary>
        /// Unitが沿っているWallのgameobject
        /// </summary>
        public GameObject FollowWallObject { private set; get; }
        private Vector3 followWallNormal;

        /// <summary>
        /// Unitが沿っているObjectがGimmickObjectである場合そのGimmickObject
        /// </summary>
        //public GimmickObject FollowingGimmickObject { private set; get; }
        /// <summary>
        /// Unitが沿っているObjectの接触地点
        /// </summary>
        public Vector3 FollowingWallTouchPosition { private set; get; }

        /// <summary>
        /// アニメーションの再生の停止再開
        /// </summary>
        public bool PauseAnimation
        {
            get => pauseAnimation;
            set
            {
                pauseAnimation = value;
                if (animator != null && animator.enabled)
                    animator.speed = value ? 0 : 1;
            }
        }
        private bool pauseAnimation = false;

        /**
         * 最低限必要なAnimatorパラメータ
         *  - Forward
         **/

        #region CoverAction properties
        
        
        private int collisionObjectLayer;
        int unitHitsWallFrameCount;
        /// <summary>
        /// Unitが壁に接触している状態
        /// </summary>
        public bool UnitHitsWall { private set; get; } = false;
        bool isCovering = false;
        Vector3 takeCoverNormal;
        /// <summary>
        /// 接触している壁に対するDot積
        /// </summary>
        float wallDotProduct;
        /// <summary>
        /// 接触している壁
        /// </summary>
        public GameObject AlongWallObject { private set; get; }
        /// <summary>
        /// Unitの移動
        /// </summary>
        Vector3 addVelocity;
        /// <summary>
        /// Unitが貼り付ける壁の最小角度
        /// </summary>
        const float TAKE_COVER_MIN_ANGLE = 80;
        /// <summary>
        /// Unitが貼り付ける壁の最大角度
        /// </summary>
        const float TAKE_COVER_MAX_ANGLE = 110;
        /// <summary>
        /// Wallに沿って歩いているときの最大速度 MoveFollowingWallAnimationCurveのvalueの最終値となる
        /// </summary>
        private float maxSpeedWhenFollowWall;
        /// <summary>
        /// Animatorのアニメーションからの加速度とRigidbodyの速度を合わせるためにFixedUpdateで使う Animatorの1フレーム前の速度
        /// </summary>
        Vector3 previousAnimatorVelocity = Vector3.zero;


        #endregion

        void Awake()
        {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            capsuleHeight = capsule.height;
            capsuleCenter = capsule.center;

            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            origGroundCheckDistance = m_GroundCheckDistance;

            //BodyTrigger.EnterAlongWallActionHandler = EnterAlongWallCallback;
            //BodyTrigger.StayAlongWallActionHandler = StayAlonwWallCallback;
            //BodyTrigger.ExitAlongWallActionHandler = ExitAlongWallCallback;

            //MoveAlongWallAnimationTotalDuration = MoveFollowingWallAnimationCurve.keys.Last().time;
            //MaxSpeedWhenFollowWall = MoveFollowingWallAnimationCurve.keys.Last().value;
            //StartToMoveTimeOnFollowingWall = DateTime.Now;
        }



        void Start()
        {
        }

        // 壁に沿った移動を行う際に必要だが今回は使用しない
        #region Along Wall
        //// Tag: AlongWall, Layer: ObjectのColliderに接触したときの呼び出し
        ///// <summary>
        ///// UnitがAlongWallに接触した際の呼び出し
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="alongWallEventArgs"></param>
        //private void EnterAlongWallCallback(object sender, HitEventArgs alongWallEventArgs)
        //{
        //    if (IsFollowingWallMode && FollowWallObject != alongWallEventArgs.HitObject)
        //    {
        //        EndToFollowWall(alongWallEventArgs.HitObject);
        //    }
        //}


        ///// <summary>
        ///// UnitがAlongWallに接触し続けている間の呼び出し
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="followingWallEventArgs"></param>
        //private void StayAlonwWallCallback(object sender, HitEventArgs followingWallEventArgs)
        //{
        //    FollowingWallTouchPosition = followingWallEventArgs.Position;
        //    if (FollowWallObject == followingWallEventArgs.HitObject) return;
        //    if (IsFollowingWallMode) return;
        //    if (IsDashMode) return;
        //    if (m_ForwardAmount > 0)
        //    {
        //        var ray = new Ray(transform.position, followingWallEventArgs.Position - transform.position);
        //        if (Physics.Raycast(ray, out var hit, 5, BodyTrigger.WallLayerMask))
        //        {
        //            followingWallEventArgs.Normal = hit.normal;
        //            var angle = Vector3.Angle(transform.forward, followingWallEventArgs.Normal);
        //            // 歩いている状況
        //            if (FollowingWallMaxDegree < angle)
        //            {
        //                // 壁に向かってほぼ垂直に移動しようとしている
        //                StartToFollowWall(followingWallEventArgs);
        //            }
        //        }

        //    }
        //}

        ///// <summary>
        ///// UnitがAlongWallから離れた際の呼び出し
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="alongWallEventArgs"></param>
        //private void ExitAlongWallCallback(object sender, HitEventArgs alongWallEventArgs)
        //{
        //    EndToFollowWall(alongWallEventArgs.HitObject);
        //}

        ///// <summary>
        ///// Wallから離れる際の処理
        ///// </summary>
        ///// <param name="alongWallEventArgs"></param>
        //private void EndToFollowWall(GameObject hitObject)
        //{
        //    if (hitObject == FollowWallObject && IsFollowingWallMode)
        //    {
        //        print("AlongWallMode OFF");
        //        FollowWallObject = null;
        //        FollowingGimmickObject = null;
        //        m_Animator.SetBool("AlongWall", false);
        //    }
        //}

        ///// <summary>
        ///// 壁に沿った風に移動する
        ///// </summary>
        ///// <param name="alongWallEventArgs"></param>
        //private void StartToFollowWall(HitEventArgs alongWallEventArgs)
        //{
        //    if (IsFollowingWallMode) return;

        //    // 新たな壁に張り付いた場合
        //    FollowWallObject = alongWallEventArgs.HitObject;
        //    FollowWallNormal = alongWallEventArgs.Normal;

        //   // FollowingGimmickObject = FollowWallObject.transform.GetComponentInParent<GimmickObject>();

        //    if (AlongWallRotationAnimation != null && AlongWallRotationAnimation.IsActive())
        //        AlongWallRotationAnimation.Kill();
        //    AlongWallRotationAnimation = DOTween.Sequence();
        //    // Normal方向と同じ方向にUnitを回転させる
        //    var vectorA = transform.forward;
        //    var vectorB = alongWallEventArgs.Normal.normalized;
        //    var angle = Vector3.Angle(vectorA, vectorB);
        //    var endAngle = transform.rotation.eulerAngles;
        //    Vector3 cross = Vector3.Cross(vectorA, vectorB);
        //    if (cross.y < 0) angle = -angle;
        //    endAngle.y += angle;

        //    AlongWallRotationAnimation.Append(transform.DORotate(endAngle, 0.5f));
        //    AlongWallRotationAnimation.Play();

        //    print("AlongWall Mode ON");
        //    m_Animator.SetBool("AlongWall", true);
        //}

        #endregion

        #region Item animations
        ///// <summary>
        ///// haveItemAnimationで指定したアイテムを持つ
        ///// </summary>
        ///// <param name="trigger"></param>
        ///// <returns></returns>
        //public IEnumerator HaveItemAnimation(Items.Item item)
        //{
        //    if (item != null)
        //    {
        //        m_Animator.SetLayerWeight(UpperLayer, 1);
        //        m_Animator.CrossFade(item.havingItemAnimationClip.name, 0.2f);
        //        yield return StartCoroutine(WaitForSeconds(0.2f));
        //    }
        //    else
        //    {
        //        m_Animator.CrossFade("Default Upper", 0.3f);
        //        yield return StartCoroutine(WaitForSeconds(0.3f));
        //        m_Animator.SetLayerWeight(UpperLayer, 0);
        //    }
        //    currentItem = item;
        //}

        //public IEnumerator ChangeItemAnimation(AnimationClip changeItemAnimation)
        //{
        //    // TODO 現在
        //    yield return StartCoroutine(WaitForSeconds(0.2f));
        //}

        ///// <summary>
        ///// アイテムを構えた状態のアニメーションを再生
        ///// </summary>
        ///// <param name="active"></param>
        //public void SetItemAnimation(Items.UseItemType useItemType, bool active)
        //{
        //    //m_Animator.SetLayerWeight(UpperLayer, 1);
        //    m_Animator.SetInteger("ItemType", useItemType.GetHashCode());
        //    m_Animator.SetBool("SetItem", active);
        //}

        #endregion

        #region Walk
        public void Move(Vector3 move, bool crouch, bool jump, bool dash)
        {
            IsDashMode = dash;
            move *= WalkSpeed;
            if (dash)
            {
                move *= DashSpeed;
                FollowWallObject = null;
            }

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();
            move = transform.InverseTransformDirection(move);
            move = Vector3.ProjectOnPlane(move, groundNormal);
            turnAmount = Mathf.Atan2(move.x, move.z);
            forwardAmount = move.z;

            ApplyExtraTurnRotation();

            // ScaleCapsuleForCrouching(crouch);
            // PreventStandingInLowHeadroom();

            // send input and other state parameters to the animator
            UpdateMoveAnimator(move);
        }

        /// <summary>
        /// World座標軸を基準にした移動 XがTurn YがForward
        /// </summary>
        /// <param name="move">YがForward XがTurn</param>
        /// <param name="speed">0.5(歩行)~1(走る)</param>
        public void WorldMove(Vector2 move, float speed)
        {
            animator.SetLayerWeight(UPPER_LAYER, 0);

            move.Normalize();
            var _speed = speed * 10f;
            var x = (float)Math.Ceiling(move.x * _speed) / 10f;
            var y = (float)Math.Ceiling(move.y * _speed) / 10f;

            turnAmount = x;
            forwardAmount = y;


            ApplyExtraTurnRotation();

            UpdateMoveAnimator(new Vector2(x, y));
        }

        /// <summary>
        /// valueだけUnitを回転させる
        /// </summary>
        /// <param name="move"></param>
        public void Rotate(float value)
        {
            RotateWithoutAnimation(value);
            UpdateMoveAnimator(Vector3.zero);
        }

        /// <summary>
        /// アニメーションなしで回転
        /// </summary>
        /// <param name="value"></param>
        public void RotateWithoutAnimation(float value)
        {
            turnAmount = value;
            forwardAmount = 0;
            ApplyExtraTurnRotation();

            animator.SetFloat("Forward", 0, 0, Time.deltaTime);
            animator.SetFloat("Turn", 0, 0, Time.deltaTime);
            if(!rigidbody.isKinematic)
                rigidbody.velocity = Vector3.zero;
        }

        void PreventStandingInLowHeadroom()
        {
            // prevent standing up in crouch-only zones
            if (!Crouching)
            {
                Ray crouchRay = new Ray(rigidbody.position + Vector3.up * capsule.radius * K_HALF, Vector3.up);
                float crouchRayLength = capsuleHeight - capsule.radius * K_HALF;
                if (Physics.SphereCast(crouchRay, capsule.radius * K_HALF, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    Crouching = true;
                }
            }
        }

        /// <summary>
        /// 移動の際にAnimatorにパラメーターを渡す
        /// </summary>
        /// <param name="move"></param>
        void UpdateMoveAnimator(Vector3 move)
        {
            // update the animator parameters
            if (!IsFollowingWallMode)
            {
                animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
                animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            }
            else
            {
                animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
                animator.SetFloat("Forward", -0.0001f, 0.1f, Time.deltaTime);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (move.magnitude > 0)
            {
                animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                // don't use that while airborne
                animator.speed = 1;
            }
        }


        void HandleAirborneMovement()
        {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = rigidbody.velocity.y < 0 ? origGroundCheckDistance : 0.01f;
        }


        void HandleGroundedMovement(bool crouch, bool jump)
        {
            // check whether conditions are right to allow a jump:
            if (jump && !crouch && animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                // jump!
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, m_JumpPower, rigidbody.velocity.z);
                animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        void ApplyExtraTurnRotation()
        {
            if (!IsFollowingWallMode)
            {
                // help the character turn faster (this is in addition to root rotation in the animation)
                float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, forwardAmount);
                transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
            }
        }

        private uint lastFrameMoveOnFollowingWall = 0;
        public void OnAnimatorMove()
        {
            // KinematicなRigidbodyの場合はVelocityを設定できないためアニメーションからの移動を行わない
            if (rigidbody.isKinematic) return;

            if (!IsFollowingWallMode)
            {
                // FollowWallModeでなく通常の移動
                // we implement this function to override the default root motion.
                // this allows us to modify the positional speed before it's applied.
                if (Time.deltaTime > 0 && animator.deltaPosition != Vector3.zero)
                {
                    addVelocity = (animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
                    addVelocity.y = rigidbody.velocity.y;
                    if (rigidbody.isKinematic)
                        print(this);
                    rigidbody.velocity = addVelocity;
                }
            }
            else
            {
                // すでに修正済みだが壁から離れる動作が遅れる == 離れるアニメーションが即座に実行されない原因はAnimatorの遷移のHasExitTimeがtrueになっている

                addVelocity = new Vector3(turnAmount, 0, forwardAmount);
                if (addVelocity.magnitude > 0.1)
                {
                    // Convert local velocity to velocity on world position
                    addVelocity = (transform.TransformPoint(addVelocity) - transform.position).normalized;
                    var angle = Vector3.Angle(addVelocity, followWallNormal);
                    if (FollowingWallMaxDegree < angle)
                    {
                        // 壁に向かってほぼ垂直に移動しようとしている
                        startToLeaveFromWallTime = DateTime.Now;
                    }
                    else if (FollowingWallMinDegree > angle)
                    {
                        // 壁から離れようとする移動方向
                        //m_Rigidbody.velocity = AddVelocity;
                        //AlongWallObject = null;
                        if (onMoveAnimatorCount - lastLeaveFromWallCount > 30)
                        {
                            startToLeaveFromWallTime = DateTime.Now;
                        }
                        else if ((float)((DateTime.Now - startToLeaveFromWallTime).TotalSeconds) > LeaveFromWallSeconds)
                        {
                            // 離れる動作をLeaveFromWallSecond間行ったためLeave動作を開始する
                            // EndToFollowWall(FollowWallObject);

                        }
                        lastLeaveFromWallCount = onMoveAnimatorCount + 1;
                    }
                    else
                    {
                        startToLeaveFromWallTime = DateTime.Now;

                        var moveAmount = Math.Abs(turnAmount);
                        float secondsFromStartToMoveTimeOnFollowingWall = (float)(DateTime.Now - startToMoveTimeOnFollowingWall).TotalSeconds;
                        if (onMoveAnimatorCount - lastMoveFollowingWallCount > 60)
                        {
                            // 最後の入力から十分時間が経っている状況
                            startToMoveTimeOnFollowingWall = DateTime.Now;
                        }
                        if (secondsFromStartToMoveTimeOnFollowingWall < moveAlongWallAnimationTotalDuration)
                        {
                            // 移動を開始したCurve内
                            moveAmount = MoveFollowingWallAnimationCurve.Evaluate(secondsFromStartToMoveTimeOnFollowingWall);
                        }


                        // 壁に沿って移動する速度の最大値に達している
                        if (onMoveAnimatorCount - lastFrameMoveOnFollowingWall < 3)
                        {
                            //IsMovingOnFollowingWall = true;
                        }
                                
                        else
                            lastFrameMoveOnFollowingWall = onMoveAnimatorCount;
                        rigidbody.velocity = Quaternion.AngleAxis(turnAmount < 0 ? -90 : 90, Vector3.up) * followWallNormal * moveAmount;

                        lastMoveFollowingWallCount = onMoveAnimatorCount + 1;

                        // 壁に沿って左右に移動する
                        
                    }
                }
            }

            onMoveAnimatorCount++;
        }

        private void FixedUpdate()
        {
            

            if (IsFollowingWallMode && !rigidbody.isKinematic)
            {
                if (forwardAmount == 0 || turnAmount == 0)
                    rigidbody.velocity = Vector3.zero;
                // Rigidbodyの移動を停止する
            }
        }

        #endregion

        #region Crouching
        internal IEnumerator Crouch(bool active)
        {
            ScaleCapsuleForCrouching(active);
            animator.SetBool("Crouch", Crouching);

            if (active)
            {
                var weight = 1f;
                while (weight > 0)
                {
                    weight -= 0.01f;
                    animator.SetLayerWeight(UPPER_LAYER, weight);
                    yield return null;
                }
                
            }
            else
            {
                var weight = 0f;
                while (weight < 1)
                {
                    weight += 0.01f;
                    animator.SetLayerWeight(UPPER_LAYER, weight);
                    yield return null;
                }
            }
        }

        void ScaleCapsuleForCrouching(bool crouch)
        {
            if (crouch)
            {
                if (Crouching) return;
                capsule.height /= 2f;
                capsule.center /= 2f;
                Crouching = true;
            }
            else
            {
                //Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
                //float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
                //if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                //{
                //	m_Crouching = true;
                //	return;
                //}
                capsule.height = capsuleHeight;
                capsule.center = capsuleCenter;
                Crouching = false;
            }
        }
        #endregion

        /// <summary>
        /// <c>PauseAnimation</c>に対応したWaitForSeconds
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator WaitForSeconds(float duration)
        {
            var start = DateTime.Now;
            while((DateTime.Now - start).TotalMilliseconds < duration * 1000)
            {
                if (PauseAnimation)
                {
                    var startStopping = DateTime.Now;
                    while (PauseAnimation)
                        yield return null;
                    var stopTime = (DateTime.Now - startStopping).TotalMilliseconds;
                    start.AddMilliseconds(stopTime);
                }
                else
                {
                    yield return null;
                }
            }
        }


        #region Motions
        // 各種細々としたモーション

        /// <summary>
        /// あたりを見回すSearchingモーション
        /// </summary>
        internal IEnumerator Searching()
        {
            // Searchingアニメーションは上半身の動きのアニメーションのためUpperLayerのブレンドを行う
            // アニメーションは3秒で終了  0.5fはアニメーションの速度
            // Weightは0.57がギリギリ自然 歩いていって停止を挟まずにSearchingに入るとより自然
            animator.SetLayerWeight(UPPER_LAYER, 0.57f);
            animator.SetTrigger("Searching");

            yield return new WaitForSeconds(3f / 0.5f);
            animator.SetLayerWeight(UPPER_LAYER, 0);
        }

        /// <summary>
        /// ノイズを鳴らすモーション
        /// </summary>
        internal IEnumerator MakeNoize()
        {
            animator.SetLayerWeight(UPPER_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER_WITH_MASK, 1);
            animator.SetTrigger("UseItem");
            yield return new WaitForSeconds(2.4f);
            animator.SetLayerWeight(UPPER_LAYER_WITH_MASK, 0);
        }

        /// <summary>
        /// 死亡するアニメーション　layerWeightを使ってすべてのモーションに優先する
        /// </summary>
        internal IEnumerator Killed()
        {
            animator.SetLayerWeight(BASE_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER_WITH_MASK, 0);
            animator.SetLayerWeight(OVERLAY_LAYER, 1);
            animator.SetBool("Death", true);
            yield return new WaitForSeconds(3.5f);
        }

        /// <summary>
        /// KilledされたUnitのアニメーションをもとに戻す
        /// </summary>
        internal void ResetKilled()
        {
            animator.SetLayerWeight(BASE_LAYER, 1);
            animator.SetLayerWeight(UPPER_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER_WITH_MASK, 0);
            animator.SetLayerWeight(OVERLAY_LAYER, 0);
            animator.SetBool("Death", false);
        }

        /// <summary>
        /// 勝利モーション　すべてのモーションを停止しWinモーションを再生
        /// </summary>
        internal IEnumerator Victory()
        {
            animator.SetLayerWeight(BASE_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER_WITH_MASK, 0);
            animator.SetLayerWeight(OVERLAY_LAYER, 1);
            animator.SetTrigger("Victory");
            yield return new WaitForSeconds(4.5f);
            // すべてのモーションをリセット
            animator.SetLayerWeight(BASE_LAYER, 1);
            animator.SetLayerWeight(UPPER_LAYER, 0);
            animator.SetLayerWeight(UPPER_LAYER_WITH_MASK,0);
            animator.SetLayerWeight(OVERLAY_LAYER, 0);

        }

        #endregion
    }

}