using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MainUI
{
    /// <summary>
    /// ロード中のいろいろを隠すためのパネル
    /// </summary>
    public class LoadingPanel : MonoBehaviour
    {
        // パネルの透明度を変更するためのコンポーネント
        private CanvasGroup canvasGroup;

        public bool IsActive => gameObject.activeSelf;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// パネルを表示する 早めの切り替えアニメーションを利用する
        /// </summary>
        internal void ShowPanel(float duration)
        {
            if (canvasGroup.alpha != 0)
            {
                return;
            }
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, duration);
        }

        /// <summary>
        /// パネルを非表示にする 早めの切り替えアニメーションを利用する
        /// </summary>
        internal void HidePanel(bool animation)
        {
            if (animation)
            {
                canvasGroup.DOFade(0, 0.5f).OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                canvasGroup.alpha = 0;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 指定された秒数だけパネルを表示する
        /// </summary>
        internal IEnumerator ShowPanelForSeconds(float duration)
        {
            ShowPanel(0.5f);
            yield return new WaitForSeconds(duration);
            HidePanel(true);
        }
    }
}
