using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Units.TPS;
using UnityEngine;
using static Utility;

namespace Units
{

    /// <summary>
    /// カメラ間の遷移をスムーズに行うためのコントローラー
    /// </summary>
    public class CameraUserController : MonoBehaviour
    {
        [Tooltip("カメラのマウスコントロールの是非")]
        [SerializeField] public bool enableCameraControlling = true;
        //[SerializeField] UnitsController unitsController;
        [Tooltip("カメラ遷移の時間")]
        [SerializeField] float MoveCameraDuration = 0.7f;

        [Header("Follow Camera Properties")]
        [Tooltip("FollowCameraが非操作時targetの裏に自動で回り込むまでの時間")]
        [SerializeField] float cameraRotateBehindSecond = 3;
        [Tooltip("FollowTargetの高さ")]
        public float targetHeight = 1.4f;
        [Tooltip("FollowCameraのDefault距離")]
        public float distance = 3.0f; // Default Distance 
        public float offsetFromWall = 0.05f; // Bring camera away from any colliding objects 
        [Tooltip("FollowCameraのMax Zoom")]
        public float maxDistance = 5f; // Maximum zoom Distance 
        [Tooltip("FollowCameraのMin Zoom")]
        public float minDistance = 1f; // Minimum zoom Distance 
        [Tooltip("FollowCamera X軸の移動速度")]
        public float xSpeed = 200.0f; // Orbit speed (Left/Right) 
        [Tooltip("FollowCamera Y軸の移動速度")]
        public float ySpeed = 200.0f; // Orbit speed (Up/Down) 
        [Tooltip("FollowCamera Y軸方向の最小角度")]
        public float yMinLimit = -80f; // Looking up limit 
        [Tooltip("FollowCamera Y軸の最大角度")]
        public float yMaxLimit = 80f; // Looking down limit 
        public float zoomRate = 40f; // Zoom Speed 
        public float rotationDampening = 0.5f; // Auto Rotation speed (higher = faster) 
        public float zoomDampening = 5.0f; // Auto Zoom speed (Higher = faster) 
        [Tooltip("Cameraが衝突して避けるCollisionのLayers")]
        public LayerMask collisionLayers = -1; // What the camera will collide with 
        public bool lockToRearOfTarget = false; // Lock camera to rear of target 
        public bool allowMouseInputX = true; // Allow player to control camera angle on the X axis (Left/Right) 
        public bool allowMouseInputY = true; // Allow player to control camera angle on the Y axis (Up/Down) 

        [Tooltip("カメラの遷移を行う際にカメラ間でrayを飛ばしてこのmaskの障害物がある場合はCutBlendを使用する")]
        [SerializeField] private LayerMask useCutBlendCameraMask;


        [Header("Start prepare caemra")]
        [Tooltip("カメラ切り替えの際にカメラの移動をCutにする距離")]
        [SerializeField] float DistanceOfChangeCameraBlendToCut = 20;

        /// <summary>
        /// TacticsのUI表示Tablet
        /// </summary>
        //[NonSerialized] public TacticsTablet.TacticsTablet TacticsTablet;

        public Camera MainCamera { private set; get; }

        [Tooltip("Followカメラが操作されていないときにターゲットの裏に自動的に回り込む機能")]
        [SerializeField] bool autoRotateBehindTarget = true;

        /// <summary>
        /// カットして瞬時にカメラ移動するblend
        /// </summary>
        private CinemachineBlend cutCameraBlend;
        /// <summary>
        /// カメラ移動死ながらblend
        /// </summary>
        private CinemachineBlend moveCameraBlend;

        /// <summary>
        /// マウスX軸の移動量
        /// </summary>
        public float MouseDeltaX { private set; get; }
        /// <summary>
        /// マウスY軸の移動量
        /// </summary>
        public float MouseDeltaY { private set; get; }
        /// <summary>
        /// キャラクターを中心としたX軸の度数法 Default 90
        /// </summary>
        private float xDeg = 90f;
        /// <summary>
        /// キャラクターを中心としたY軸の度数法 Default 20
        /// </summary>
        private float yDeg = 20f;
        /// <summary>
        /// カメラとの距離  Default 3
        /// </summary>
        private float currentDistance = 3;
        private float desiredDistance = 3;
        private float correctedDistance = 3;
        private bool rotateBehind = false;
        private float pbuffer = 0.0f; //Cooldownpuffer for SideButtons 
        //private float coolDown = 0.5f; //Cooldowntime for SideButtons  
        private float timeDeltaFromControlled = 0;
        /// <summary>
        /// 現在操作中のカメラ
        /// </summary>
        public CinemachineVirtualCamera ActiveVirtualCamera { private set; get; }
        /// <summary>
        /// Followなどの際に中心に置かれるUserController
        /// </summary>
        public ThirdPersonUserControl ActiveTPSController { private set; get; }
        /// <summary>
        /// FollowCameraやStationaryCameraで追従する対象
        /// </summary>
        public GameObject TargetToFollow{ private set; get; }
        // Target to follow 
        /// <summary>
        /// Followの際に動くObject
        /// </summary>
        public GameObject FollowObject { private set; get; }
        /// <summary>
        /// 現在アニメーション中か
        /// </summary>
        public bool IsOnAnimation { private set; get; } = false;
        /// <summary>
        /// カメラの撮影モード
        /// </summary>
        public CameraMode Mode { private set; get; }
        /// <summary>
        /// Maincameraの移動brain
        /// </summary>
        CinemachineBrain cinemachineBrain;

        /// <summary>
        /// Cinemachineによるカメラ移動の時間
        /// </summary>
        public float CameraChangeDuration
        {
            get => cinemachineBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.BlendTime;
        }
        /// <summary>
        /// マウスホイールによる距離の調整を停止
        /// </summary>
        private bool allowMouseWheel = true;

        /// <summary>
        /// CameraUserControllerが初期化されたか
        /// </summary>
        public bool IsActivated { private set; get; } = false;


        #region Base functions
        private void Awake()
        {
            MainCamera = this.GetComponent<Camera>();
            cinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
                PrintError("CinemachineBrain is not attached to MainCamera");
            Mode = CameraMode.Follow;
        }

        // Start is called before the first frame update
        void Start()
        {
            Vector3 angles = MainCamera.transform.eulerAngles;
            xDeg = angles.x;
            yDeg = angles.y;
            currentDistance = distance;
            desiredDistance = distance;
            correctedDistance = distance;
          
            SetCutBlend();

            // Make the rigid body not change rotation 
            //        if (rigidbody)
            //            rigidbody.freezeRotation = true;

            if (lockToRearOfTarget)
                rotateBehind = true;

            if (ActiveTPSController == null && TargetToFollow != null)
                ActiveTPSController = TargetToFollow.GetComponent<ThirdPersonUserControl>();

            IsActivated = true;
        }

        private void LateUpdate()
        {
            //if (gameManager != null && (gameManager.StartCanvasController.IsEnable))
            //    return;
            if (Mode == CameraMode.Follow)
            {
                if (TargetToFollow == null || IsOnAnimation || !enableCameraControlling)
                    return;
                FollowCameraUpdate(FollowObject, TargetToFollow);
                if (UserController.ChangeFollowCameraPositionRightOrLeft)
                    ChangeFollowCameraXAxisGap();
            }
                
        }
        #endregion


        #region Follow Unit Camera Mode
        /// <summary>
        /// FollowVirtualCameraを持っているTPSCOntrollerを追従対象に切り替え
        /// </summary>
        /// <param name="thirdPersonUserControl"></param>
        /// <returns></returns>
        public IEnumerator SetAsFollowTarget(ThirdPersonUserControl thirdPersonUserControl)
        {
            if (Mode == CameraMode.Follow && thirdPersonUserControl == ActiveTPSController) yield break ;

            Mode = CameraMode.Follow;
            allowMouseWheel = true;

            SetFollowCameraXAxisGap(thirdPersonUserControl, false);
            StartCoroutine(ActivateVirtualCamera(thirdPersonUserControl.followCamera));
            yield return StartCoroutine(ChangeFollowTarget(thirdPersonUserControl));
        }

        /// <summary>
        /// FollowするTargetを切り替える
        /// </summary>
        /// <param name="target"></param>
        private IEnumerator ChangeFollowTarget(ThirdPersonUserControl tpsController, float defaultDistance = 3)
        {
            var followObject = tpsController.followCameraParent;
            var targetToFollow = tpsController.FollowCameraCenter;

            allowMouseInputY = true;
            timeDeltaFromControlled = 0;
            ActiveTPSController = tpsController;
            this.TargetToFollow = targetToFollow;
            FollowObject = followObject;

            // カメラ位置をデフォルトに
            RotateBehindTarget(false);

            yDeg = 20f;
            currentDistance = defaultDistance;
            desiredDistance = defaultDistance;
            correctedDistance = defaultDistance;

            FollowCameraUpdate(followObject, targetToFollow);
            

            yield return new WaitForSeconds(CameraChangeDuration);
            enableCameraControlling = true;
            IsOnAnimation = false;
        }

        /// <summary>
        /// カメラ位置をマウス位置からアップデートする
        /// </summary>
        /// <param name="followObject">followして移動するobject</param>
        /// <param name="target">followされて周りを回られるObject</param>
        private void FollowCameraUpdate(GameObject followObject, GameObject target)
        {
            //pushbuffer 
            if (pbuffer > 0)
                pbuffer -= Time.deltaTime;
            if (pbuffer < 0)
                pbuffer = 0;

            // If either mouse buttons are down, let the mouse govern camera position 
            if (GUIUtility.hotControl == 0)
            {
                //Check to see if mouse input is allowed on the axis 
                MouseDeltaX = 0f;
                if (allowMouseInputX)
                {
                    MouseDeltaX = UserController.MouseDeltaX * xSpeed * 0.02f;
                    xDeg += MouseDeltaX;
                }
                else
                {
                    //RotateBehindTarget(true);
                }
                MouseDeltaY = UserController.MouseDeltaY * ySpeed * 0.02f;
                if (allowMouseInputY)
                    yDeg -= MouseDeltaY;

                //Interrupt rotating behind if mouse wants to control rotation 
                if (!lockToRearOfTarget)
                    rotateBehind = false;


                // ease behind the target if the character is not controlled
                bool isNotControlled = MouseDeltaX == 0 && MouseDeltaY == 0;
                if (isNotControlled)
                    timeDeltaFromControlled += Time.deltaTime;
                else
                    timeDeltaFromControlled = 0;

                if ((rotateBehind || timeDeltaFromControlled > cameraRotateBehindSecond) && autoRotateBehindTarget)
                {
                    RotateBehindTarget(true);
                }
            }

            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            // Set camera rotation 
            Quaternion rotation = Quaternion.Euler(yDeg, xDeg, 0);

            // Calculate the desired distance 
            if (allowMouseWheel)
            {
                desiredDistance -= UserController.MouseWheel * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                correctedDistance = desiredDistance;
            }

            // Calculate desired camera position 
            Vector3 position = target.transform.position - (rotation * Vector3.forward * desiredDistance);

            // Check for collision using the true target's desired registration point as set by user using height 
            Vector3 trueTargetPosition = new Vector3(target.transform.position.x,
                target.transform.position.y + targetHeight, target.transform.position.z);

            // ? カメラがCollisionを持つ物に遮られた場合のいち補正?
            // If there was a collision, correct the camera position and calculate the corrected distance 
            var isCorrected = false;
            if (Physics.Linecast(trueTargetPosition, position, out RaycastHit collisionHit, collisionLayers))
            {
                // Calculate the distance from the original estimated position to the collision location, 
                // subtracting out a safety "offset" distance from the object we hit.  The offset will help 
                // keep the camera from being right on top of the surface we hit, which usually shows up as 
                // the surface geometry getting partially clipped by the camera's front clipping plane. 
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }

            // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
            currentDistance = !isCorrected || correctedDistance > currentDistance
                ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening)
                : correctedDistance;

            // Keep within limits 
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            // Recalculate position based on the new currentDistance 
            position = target.transform.position - (rotation * Vector3.forward * currentDistance);

            //Finally Set rotation and position of camera 
            followObject.transform.SetPositionAndRotation(position, rotation);
            //tpsControl.followCamera.transform.rotation = rotation;
        }

        /// <summary>
        /// Followcamraを撮影中のUnitの背後に移動させる
        /// </summary>
        /// <param name="lerp">移動をなめらかにアニメーションさせるか</param>
        public void RotateBehindTarget(bool lerp)
        {
            float targetRotationAngle = TargetToFollow.transform.eulerAngles.y;
            float currentRotationAngle = MainCamera.transform.eulerAngles.y;
            float lerpAngle = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
            xDeg = lerp ? lerpAngle : targetRotationAngle;

            // Stop rotating behind if not completed 
            if (targetRotationAngle == currentRotationAngle || !lerp)
            {
                if (!lockToRearOfTarget)
                    rotateBehind = false;
            }
            else
                rotateBehind = true;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// FollowCameraの位置を左右に変更する
        /// </summary>
        /// <returns></returns>
        private void ChangeFollowCameraXAxisGap()
        {
            //var setting = gameManager.StaticData.CommonSetting;
            //setting.IsFollowCameraCenterRight = !setting.IsFollowCameraCenterRight;
            //SetFollowCameraXAxisGap(ActiveTPSController, true);
        }

        /// <summary>
        /// FollowCamraの位置をx軸でずらす
        /// </summary>
        /// <param name="animation"></param>
        private void SetFollowCameraXAxisGap(ThirdPersonUserControl activeTPSControl, bool animation)
        {
            //var setting = gameManager.StaticData.CommonSetting;
            //var gap = setting.IsFollowCameraCenterRight ? FollowCameraXAxisGap : -FollowCameraXAxisGap;
            //if (changeFollowCameraCenterPositionAnimation != null && changeFollowCameraCenterPositionAnimation.IsActive())
            //    changeFollowCameraCenterPositionAnimation.Kill();
            //if (animation)
            //{
            //    changeFollowCameraCenterPositionAnimation = DOTween.Sequence();
            //    changeFollowCameraCenterPositionAnimation.Append(activeTPSControl.followCamera.transform.DOLocalMoveX(gap, 0.3f));
            //    changeFollowCameraCenterPositionAnimation.Play();
            //}
            //else
            //{
            //    activeTPSControl.followCamera.transform.localPosition = new Vector3(gap, 0f, 0f);
            //}
        }

        #endregion

        #region Free camera mode
        // 別個のVirtualCameraに対して遷移を行う
        /// <summary>
        /// このVirtualCameraをアクティブにし、以前のVirtualCameraを非アクティブにする
        /// </summary>
        /// <param name="freeCamera"></param>
        public void SetFreeCameraMode(CinemachineVirtualCamera freeCamera)
        {
            Mode = CameraMode.None;
            StartCoroutine(ActivateVirtualCamera(freeCamera));
        }
        #endregion

        // +++++++++++++

        #region Camera move animations
        /// <summary>
        /// CinemachineBlendのカメラ遷移にカットを使用
        /// </summary>
        private void SetCutBlend()
        {
            cinemachineBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Style = CinemachineBlendDefinition.Style.Cut;
            cinemachineBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Time = 0;
        }

        /// <summary>
        /// CinemachineBlendのカメラ遷移にEaseInOut移動を使用
        /// </summary>
        private void SetEraceInOutBlend()
        {
            //Print(cinemachineBrain, cinemachineBrain.m_CustomBlends.m_CustomBlends.Length, cinemachineBrain.m_CustomBlends.m_CustomBlends[0].m_Blend);
            cinemachineBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cinemachineBrain.m_CustomBlends.m_CustomBlends[0].m_Blend.m_Time = MoveCameraDuration;
        }

        /// <summary>
        /// 与えられたVirtualCameraのPriorityを上げる、それ以外のcameraのPriorityは下げる
        /// </summary>
        /// <param name="cam">アクティブにするVirtualCamera</param>
        /// <param name="thirdPersonUserControl">新しいTPSControllerがあればこれをセットする 以前のTPSCamのCameraは非アクティブになる</param>
        private IEnumerator ActivateVirtualCamera(CinemachineVirtualCamera cam)
        {
            yield return null;

            // カメラの遷移でカメラ間の距離が離れている場合はCutBlendを使用する
            var cameraModeCut = false;
            var distOldToNewCamera = Vector3.Distance(MainCamera.transform.position, cam.transform.position);
            var direction = cam.transform.position - MainCamera.transform.position;
            if (distOldToNewCamera > DistanceOfChangeCameraBlendToCut)
                cameraModeCut = true;

            // カメラの遷移でカメラ間に障害物がある場合はCutBlendを使用する
            var ray = new Ray(MainCamera.transform.position, direction * 30);
            if (Physics.Raycast(ray, out var hit, 100, useCutBlendCameraMask))
            {
                if (hit.distance < distOldToNewCamera)
                    cameraModeCut = true;
            }

            if (cameraModeCut)
            {
                SetCutBlend();
            }
            else
            {
                SetEraceInOutBlend();
            }
            if (ActiveVirtualCamera != null)
                ActiveVirtualCamera.Priority = 0;
            cam.Priority = 1;

            ActiveVirtualCamera = cam;

        }

        #endregion

    }

    /// <summary>
    /// カメラの動作モード
    /// </summary>
    public enum CameraMode
    {
        None,
        /// <summary>
        /// TPSでUnitの周りを周回する
        /// </summary>
        Follow
    }
}