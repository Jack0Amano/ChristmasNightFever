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
    /// 足音を管理してaudiosourceを使って再生する
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
        [Header("足音を均すためのソース (スピーカー的な)")]
        [SerializeField] AudioSource footstepAudioSource;
        [Header("声のソース")]
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
        /// Deathアニメーションの際に呼び出される声のタイミング
        /// </summary>
        public void PlayDeathVoiceSE()
        {
            voiceAudioSource.PlayOneShot(voiceClip);
        }

        /// <summary>
        /// ベルを鳴らすアニメーションの際に呼び出されるベルのタイミング
        /// </summary>
        public void PlayerBellSE()
        {
            voiceAudioSource.PlayOneShot(bellClip);
        }

        /// <summary>
        /// Enemyが発見した際に呼び出される声のタイミング
        /// </summary>
        public void EnemyWinVoiceSE()
        {
            voiceAudioSource.PlayOneShot(enemyWinVoice);
        }

        /// <summary>
        /// 笛を鳴らすアニメーションの際に呼び出される声のタイミング
        /// </summary>
        public void EnemyrWhistleSE()
        {
            voiceAudioSource.PlayOneShot(whistleClip);
        }

        /// <summary>
        /// 走りのアニメーションの際に呼び出される足音のタイミング
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
        /// 歩きのアニメーションの際に呼び出される足音のタイミング
        /// </summary>
        public void PlayWalkFootstepSE()
        {
            if (tpsController.IsRunning) return;
            footstepAudioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
            footstepAudioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}