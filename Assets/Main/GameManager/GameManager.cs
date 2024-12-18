using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using StageObjects;
using MainUI;
using Units;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Utility;
using UnityEngine.AzureSky;



// 開発速度を一番に考えてそれ以外は軽視したためシングルトンに頼り切った流れができている
// 別に進行状況を管理するクラスを作ってそこに処理を移す GameStreamクラスを作成して各上流Controllerがそれを参照するようにする
namespace MainGame
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// ゲームの進行状況
        /// </summary>
        public GameState CurrentGameState {get; internal set; }

        /// <summary>
        /// LoadされているStageのaddressableのID
        /// <./summary>
        public string CurrentStageId { get; internal set; }

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        /// <summary>
        /// ゲームを終了する StartPanelまたはresultPanelのExitButtonから呼び出される
        /// </summary>
        public void EndGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

    }


    public enum GameState
    {
        Title,
        Playing,
        Result,
    }
}