using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using Units.TPS;

namespace Units
{
    /// <summary>
    /// �������Ǘ�����audiosource���g���čĐ�����
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(ThirdPersonUserControl))]
    public class SEController : MonoBehaviour
    {
        [SerializeField] AudioClip[] clips;
        [SerializeField] AudioClip voiceClip;
        [SerializeField] AudioClip bellClip;

        [SerializeField] AudioClip enemyWinVoice;
        [SerializeField] AudioClip whistleClip;
        [SerializeField] float pitchRange = 0.1f;
        [Header("�������ς����߂̃\�[�X (�X�s�[�J�[�I��)")]
        [SerializeField] AudioSource footstepAudioSource;
        [Header("���̃\�[�X")]
        [SerializeField] AudioSource voiceAudioSource;
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] string footstampMixerGroup;
        [SerializeField] string seMixerGroup;

        internal ThirdPersonUserControl tpsController;


        private void Awake()
        {
            footstepAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(footstampMixerGroup)[0];
            voiceAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(seMixerGroup)[0];
            tpsController = GetComponent<ThirdPersonUserControl>();

        }


        /// <summary>
        /// Death�A�j���[�V�����̍ۂɌĂяo����鐺�̃^�C�~���O
        /// </summary>
        public void PlayDeathVoiceSE()
        {
            voiceAudioSource.PlayOneShot(voiceClip);
        }

        /// <summary>
        /// �x����炷�A�j���[�V�����̍ۂɌĂяo�����x���̃^�C�~���O
        /// </summary>
        public void PlayerBellSE()
        {
            voiceAudioSource.PlayOneShot(bellClip);
        }

        /// <summary>
        /// Enemy�����������ۂɌĂяo����鐺�̃^�C�~���O
        /// </summary>
        public void EnemyWinVoiceSE()
        {
            voiceAudioSource.PlayOneShot(enemyWinVoice);
        }

        /// <summary>
        /// �J��炷�A�j���[�V�����̍ۂɌĂяo����鐺�̃^�C�~���O
        /// </summary>
        public void EnemyrWhistleSE()
        {
            voiceAudioSource.PlayOneShot(whistleClip);
        }

        /// <summary>
        /// ����̃A�j���[�V�����̍ۂɌĂяo����鑫���̃^�C�~���O
        /// </summary>
        public void PlayRunFootstepSE()
        {
            if (tpsController.IsRunning)
            {
                footstepAudioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
                footstepAudioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
            else if (tpsController.IsAutoMoving)
            {
                footstepAudioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
                footstepAudioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }

        /// <summary>
        /// �����̃A�j���[�V�����̍ۂɌĂяo����鑫���̃^�C�~���O
        /// </summary>
        public void PlayWalkFootstepSE()
        {
            if (tpsController.IsRunning) return;
            footstepAudioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
            footstepAudioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}