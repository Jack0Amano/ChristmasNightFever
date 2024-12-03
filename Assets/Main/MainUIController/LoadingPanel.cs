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

        // Start is called before the first frame update
        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// パネルを表示する 早めの切り替えアニメーションを利用する
        /// </summary>
        internal void ShowPanel()
        {
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, 0.5f);
        }
    }
}
