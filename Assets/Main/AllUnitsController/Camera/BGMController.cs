
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using Units.TPS;
using System;

namespace Units
{
    /// <summary>
    /// �������Ǘ�����audiosource���g���čĐ�����
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BGMController : MonoBehaviour
    {
        [Tooltip("�������ɍĐ������BGM")]
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