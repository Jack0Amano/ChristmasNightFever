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
    /// Unitから出る音全般を管理する
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
        /// Playerが発見時に出す声がでるアニメーションのタイミングで呼び出される
        /// </summary>
        public void PlayDeathVoiceSE()
        {
            voiceAudioSource.PlayOneShot(voiceClip);
        }

        /// <summary>
        /// ベルを鳴らすアニメーションで音が出るタイミングで呼び出される
        /// </summary>
        public void PlayerBellSE()
        {
            voiceAudioSource.PlayOneShot(bellClip);
        }

        /// <summary>
        /// 敵が発見時に発見者以外の敵が上げる歓声
        /// </summary>
        public void EnemyWinVoiceSE()
        {
            voiceAudioSource.PlayOneShot(enemyWinVoice);
        }

        /// <summary>
        /// 敵が発見時にUnitControllerから呼び出される
        /// </summary>
        public void EnemyrWhistleSE()
        {
            voiceAudioSource.PlayOneShot(whistleClip);
        }

        /// <summary>
        /// 走りのアニメーションの際に足音の出るであろうアニメーションタイミングで呼び出される
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
        /// 歩きのアニメーションの際に足音の出るであろうアニメーションタイミングで呼び出される
        /// </summary>
        public void PlayWalkFootstepSE()
        {
            if (tpsController.IsRunning) return;
            footstepAudioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
            footstepAudioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}