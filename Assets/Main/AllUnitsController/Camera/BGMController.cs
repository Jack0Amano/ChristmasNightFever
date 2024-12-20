
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using Units.TPS;
using System;

namespace Units
{
    /// <summary>
    /// 足音を管理してaudiosourceを使って再生する
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BGMController : MonoBehaviour
    {
        [Tooltip("勝利時に再生されるBGM")]
        [SerializeField] AudioClip winClip;
        private AudioSource audioSource;
        

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }


        private void Start()
        {
        }

        public void PlayWinBGM()
        {
            audioSource.PlayOneShot(winClip);
        }
    }
}