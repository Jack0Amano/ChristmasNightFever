using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MainUI
{
    /// <summary>
    /// ���[�h���̂��낢����B�����߂̃p�l��
    /// </summary>
    public class LoadingPanel : MonoBehaviour
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
        /// �p�l����\������ ���߂̐؂�ւ��A�j���[�V�����𗘗p����
        /// </summary>
        internal void ShowPanel()
        {
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, 0.5f);
        }
    }
}
