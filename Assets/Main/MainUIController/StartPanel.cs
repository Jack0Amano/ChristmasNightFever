using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MainUI
{
    /// <summary>
    /// �Q�[���J�n���̃p�l��    
    /// �X�e�[�W�I���Ȃǂ�UI��\������
    /// </summary>
    public class StartPanel : MonoBehaviour
    {
        // �p�l���̓����x��ύX���邽�߂̃R���|�[�l���g
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
        /// �p�l����\������
        /// </summary>
        internal void ShowPanel()
        {
            gameObject.SetActive(true);
        }
    }
}
