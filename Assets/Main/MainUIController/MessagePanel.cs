using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.AzureSky;

namespace MainUI
{
    public class MessagePanel : MonoBehaviour
    {
      
        [SerializeField] TextMeshProUGUI messageText;
        [Tooltip("画面全体を覆うボタン これでコンテナのMessageを次に送る")]
        [SerializeField] public Button overlayButton;

        [SerializeField] List<MessageContainer> messageContainerList;

        [Tooltip("これより早くメッセージ送りができないようにする")]
        [SerializeField] float messageInterval = 0.5f;

        [SerializeField] private AzureCoreSystem azureCoreSystem;

        private MessageContainer currentCountainer;
        private int messageIndex = 0;

        // 最後に表示したメッセージの時間
        private float lastMessageTime;

        /// <summary>
        /// 表示するメッセージがあるかどうか
        /// </summary>
        public bool DoesMessageExist
        {
            get => messageContainerList != null && 
                   messageContainerList.Count != 0 && 
                   messageIndex < messageContainerList[0].messages.Count;
        }

        // Start is called before the first frame update
        void Start()
        {
            gameObject.SetActive(false);
            messageText.text = "";
            overlayButton.onClick.AddListener(NextMessage);
            ChangeWeatherToSnow();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <system>
        /// 天気を雪に変更する globallistの8版 specialIDから指定できる
        /// </system>
        private void ChangeWeatherToSnow()
        {
            azureCoreSystem.weatherSystem.SetGlobalWeather(8);
        }

        /// <summary>
        /// OverlayButtonが押されたときに呼ばれる これで次のメッセージを表示する
        /// </summary>
        private void NextMessage()
        {
            if (Time.time - lastMessageTime < messageInterval)
            {
                return;
            }
            if (messageIndex < currentCountainer.messages.Count)
            {
                var message = currentCountainer.messages[messageIndex];
                // \\nを改行に変換
                message = message.Replace("\\n", "\n");
                messageText.text = message;
            }
            else
            {
                GameManager.Instance.OnMessagePanelClosed();
                gameObject.SetActive(false);
                
            }
            lastMessageTime = Time.time;
            messageIndex++;
        }

        /// <summary>
        /// パネルの表示
        /// </summary>
        internal void ShowPanel()
        {
            if (messageContainerList == null || messageContainerList.Count == 0)
            {
                GameManager.Instance.OnMessagePanelClosed();
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            currentCountainer = messageContainerList[0];
            NextMessage();
        }
    }

    // TODO 本当はイベントノードで行いたいが時間がないし、表示するメッセージは簡素で短いのでインスペクタで代用
    // インスペクタは簡単に消えるので注意
    /// <summary>
    /// インスペクタ上でメッセージを保存しておくためのクラス
    /// </summary>
    [Serializable] class MessageContainer
    {
        [SerializeField] internal string containerID;
        [Tooltip("メッセージを保存するリスト")]
        [SerializeField] internal List<string> messages;
        [Tooltip("選択表示で次のコンテナ 選択肢のメッセージはその候補のコンテナのindex0メッセージが使用される")]
        [SerializeField] internal List<string> nextMessageContainersID;
        /// <summary>
        /// 会話途中で雪を降らせたりするための特殊なID
        /// </summary>
        [Tooltip("コンテナのメッセージを出し終えた後に実行される特殊なID")]
        [SerializeField] internal string specialID;
    }
}