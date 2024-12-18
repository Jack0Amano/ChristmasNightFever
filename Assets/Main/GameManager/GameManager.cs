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



// �J�����x����Ԃɍl���Ă���ȊO�͌y���������߃V���O���g���ɗ���؂������ꂪ�ł��Ă���
// �ʂɐi�s�󋵂��Ǘ�����N���X������Ă����ɏ������ڂ� GameStream�N���X���쐬���Ċe�㗬Controller��������Q�Ƃ���悤�ɂ���
namespace MainGame
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// �Q�[���̐i�s��
        /// </summary>
        public GameState CurrentGameState {get; internal set; }

        /// <summary>
        /// Load����Ă���Stage��addressable��ID
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
        /// �Q�[�����I������ StartPanel�܂���resultPanel��ExitButton����Ăяo�����
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