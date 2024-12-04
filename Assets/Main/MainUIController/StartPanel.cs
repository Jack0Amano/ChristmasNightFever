using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Linq;
using static Utility;

namespace MainUI
{
    /// <summary>
    /// ゲーム開始時のパネル    
    /// ステージ選択などのUIを表示する
    /// </summary>
    public class StartPanel : MonoBehaviour
    {

        [SerializeField] internal List<ButtonAndStageID> stageButtons;
        [Tooltip("ゲームを終了するボタン")]
        [SerializeField] private Button exitButton;


        private void Awake()
        {
            exitButton.onClick.AddListener(() =>
            {
                GameManager.Instance.EndGame();
            });
            stageButtons.ForEach(b =>
            {
                b.button.onClick.AddListener(() =>
                {
                    stageButtons.ForEach(s => s.button.interactable = false);
                });
            });
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
        /// パネルを表示する
        /// </summary>
        internal void ShowPanel()
        {
            stageButtons.ForEach(b=> b.button.interactable = true);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// パネルを非表示にする
        /// </summary>
        internal void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ステージが01とTurorialのみなので仮に、Buttonにaddresaable IDを持たせる
    /// </summary>
    [Serializable] internal class ButtonAndStageID
    {
        /// <summary>
        /// Button
        /// </summary>
        public Button button;
        /// <summary>
        /// Button選択でLoadするStageのAddressable ID
        /// </summary>
        [Tooltip("Button選択でLoadするStageのAddressable ID")]
        public string stageID;

        public ButtonAndStageID(Button button, string id)
        {
            this.button = button;
            stageID = id;
        }
    }
}
