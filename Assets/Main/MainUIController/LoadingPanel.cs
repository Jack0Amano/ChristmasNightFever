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
        /// �p�l����\������ ���߂̐؂�ւ��A�j���[�V�����𗘗p����
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
        /// �p�l�����\���ɂ��� ���߂̐؂�ւ��A�j���[�V�����𗘗p����
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
        /// �w�肳�ꂽ�b�������p�l����\������
        /// </summary>
        internal IEnumerator ShowPanelForSeconds(float duration)
        {
            ShowPanel(0.5f);
            yield return new WaitForSeconds(duration);
            HidePanel(true);
        }
    }
}
