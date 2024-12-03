using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MainUI
{
    /// <summary>
    /// リザルト画面の制御を行う Win Loseの両方の表示を行う   
    /// Winの場合はスコアを表示する   
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// パネルを表示する ゲームが終わるときのアニメーションのため、エモい感じのアニメーションを利用する
        /// </summary>
        internal void ShowPanel()
        {
            gameObject.SetActive(true);
            // ここにアニメーションを追加する
        }
    }
}
