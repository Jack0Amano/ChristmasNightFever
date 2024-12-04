using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Utility;

namespace Units.Icon
{
    /// <summary>
    /// 頭上に現れる!とか?とかのclass
    /// </summary>
    public class HeadUP : MonoBehaviour
    {
        [SerializeField] GameObject questionPanel;
        [SerializeField] GameObject questionInnerPanel;
        [SerializeField] GameObject exculamationPanel;
        [Tooltip("Exculamationマークを表示する最小時間")]
        [SerializeField] float minExculamationTime = 2;
        [Tooltip("Unitが発見したとかの時の頭上アイコンの距離によるサイズ変更の倍率")]
        [SerializeField] public float HeadUpIconSizeRate = 1;


        Material questionMaterial;
        Material exculamationMaterial;

        Transform cameraTransform;

        /// <summary>
        /// 頭上アイコンが現在フェードアニメーション中か
        /// </summary>
        bool isFadeAnimating = false;

        static readonly float distanceStartResize = 8;
        /// <summary>
        /// Exculamationを表示した時の時刻
        /// </summary>
        float showExculamationTime = 0;

        /// <summary>
        /// 現在表示中のアイコンの種類
        /// </summary>
        public HeadUpIconType Type { private set; get; } = HeadUpIconType.None;

        float defaultSize;
        float IconSize
        {
            get
            {
                var distFromCam = Vector3.Distance(transform.position, cameraTransform.position);
                if (distFromCam > distanceStartResize)
                    return (distFromCam - distanceStartResize) * HeadUpIconSizeRate + defaultSize;
                return defaultSize;
            }
        }

        private void Awake()
        {
            defaultSize = questionPanel.transform.localScale.y;
        }

        // Start is called before the first frame update
        void Start()
        {
            questionMaterial = questionInnerPanel.GetComponent<MeshRenderer>().material;
            exculamationMaterial = exculamationPanel.GetComponent<MeshRenderer>().material;

            cameraTransform = Camera.main.transform;

            questionPanel.transform.localScale = Vector3.zero;
            questionInnerPanel.transform.localScale = Vector3.zero;
            exculamationPanel.transform.localScale = Vector3.zero;
        }

        private void Update()
        {

            if (Type != HeadUpIconType.None)
            {
                transform.LookAt(cameraTransform);
                var size = IconSize;

                //if (!isFadeAnimating)
                //{
                //    if (type == FindOutType.Exculamation && exculamationPanel.transform.localScale.x != size)
                //    {
                //        exculamationPanel.transform.localScale = new Vector3(size, size, size);
                //    }
                //    else if (type == FindOutType.Question && questionPanel.transform.localScale.x != size)
                //    {
                //        var newSize = new Vector3(size, size, size);
                //        questionPanel.transform.localScale = newSize;
                //        questionInnerPanel.transform.localScale = newSize;
                //    }
                        
                //}
            }
                
        }

        /// <summary>
        /// FindOutLevelに応じてアイコンを表示    
        /// EnemyAI.TryFindPlayer()からFindOutLevelを計算するたび呼び出される   
        /// FindOutLevelが1になったら一回のみ呼ばれて後は呼ばれない (発見されてゲームオーバーなため)
        /// </summary> 
        /// <param name="level">0~1のLevelで1だと！が出る</param>
        public void SetFindOutLevel(float level)
        {
            level = (float)System.Math.Truncate(level * 1000) / 1000;

            if (level < 1 && level > 0.05)
            {
                ShowQuestion(level);
            }
            else if (1 <= level)
            {
                ShowExculamation();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// Questionマークを指定したレベルで表示
        /// </summary>
        /// <param name="level">0~1で</param>
        public void ShowQuestion(float level)
        {

            if (Type == HeadUpIconType.Exculamation)
                return;


            exculamationPanel.transform.localScale = Vector3.zero;
            if (Type == HeadUpIconType.None)
            {
                // NoneからQuestionに変わる場合
                isFadeAnimating = true;
                var seq = DOTween.Sequence();
                var size = IconSize;
                seq.Append(questionPanel.transform.DOScale(size, 0.2f));
                seq.Join(questionInnerPanel.transform.DOScale(size, 0.2f));
                seq.OnComplete(() =>
                {
                    questionMaterial.SetFloat("_Level", level);
                    isFadeAnimating = false;
                });
                seq.Play();
            }
            else
            {
                questionMaterial.SetFloat("_Level", level);
            }
            Type = HeadUpIconType.Question;
        }

        /// <summary>
        /// Questionマークを消してExculamationマークを表示 2秒後に消える （ゲームオーバー時のアニメーションに移るため)
        /// </summary>
        /// <returns>新たにExculamationマークを出現させる場合true</returns>
        public bool ShowExculamation()
        {
            if (Type == HeadUpIconType.Exculamation)
                return false;
            questionMaterial.SetFloat("_Level", 0);
            var seq = DOTween.Sequence();
            seq.Append(questionPanel.transform.DOScale(0, 0.2f));
            seq.Join(questionInnerPanel.transform.DOScale(0, 0.2f));
            seq.Join(exculamationPanel.transform.DOScale(IconSize, 0.2f));
            seq.AppendInterval(2f);
            seq.Append(exculamationPanel.transform.DOScale(0, 0.2f));
            seq.OnComplete(() => isFadeAnimating = false);
            isFadeAnimating = true;
            seq.Play();

            showExculamationTime = Time.time;
            Type = HeadUpIconType.Exculamation;
            return true;
        }

        /// <summary>
        /// マークを非表示にする
        /// </summary>
        public void Hide()
        {
            if (Type == HeadUpIconType.None)
                return;

            questionMaterial.SetFloat("_Level", 0);
            var seq = DOTween.Sequence();
            //var showTime = Time.time - showExculamationTime;
            //if (showExculamationTime != 0 && showTime < minExculamationTime)
            //    seq.SetDelay(minExculamationTime - showTime);
            seq.Append(questionPanel.transform.DOScale(0, 0.2f));
            seq.Join(questionInnerPanel.transform.DOScale(0, 0.2f));
            if (exculamationPanel.transform.localScale.x != 0)
                seq.Join(exculamationPanel.transform.DOScale(0, 0.2f));
            seq.Play();
            Type = HeadUpIconType.None;
        }
    }

    /// <summary>
    /// HeadUPIconの種類
    /// </summary>
    public enum HeadUpIconType
    {
        None,
        Question,
        Exculamation
    }
}