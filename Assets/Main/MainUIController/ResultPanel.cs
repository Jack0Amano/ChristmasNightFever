using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MainUI
{
    /// <summary>
    /// ���U���g��ʂ̐�����s�� Win Lose�̗����̕\�����s��   
    /// Win�̏ꍇ�̓X�R�A��\������   
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
        /// �p�l����\������ �Q�[�����I���Ƃ��̃A�j���[�V�����̂��߁A�G���������̃A�j���[�V�����𗘗p����
        /// </summary>
        internal void ShowPanel(bool animation)
        {
            gameObject.SetActive(true);
            if (animation)
            {
                // �����ɃA�j���[�V������ǉ�����
            }
        }

        /// <summary>
        /// �p�l�����B�� �Q�[�����I���Ƃ��̃A�j���[�V�����̂��߁A�G���������̃A�j���[�V�����𗘗p����
        /// </summary>
        /// <param name="animation"></param>
        internal void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
