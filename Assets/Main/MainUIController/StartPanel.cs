using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MainUI
{
    /// <summary>
    /// ゲーム開始時のパネル    
    /// ステージ選択などのUIを表示する
    /// </summary>
    public class StartPanel : MonoBehaviour
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
        /// パネルを表示する
        /// </summary>
        internal void ShowPanel()
        {
            gameObject.SetActive(true);
        }
    }
}
