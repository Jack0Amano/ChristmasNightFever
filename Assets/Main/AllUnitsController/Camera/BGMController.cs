
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using Units.TPS;
using System;

namespace Units
{
    /// <summary>
    /// ‘«‰¹‚ğŠÇ—‚µ‚Äaudiosource‚ğg‚Á‚ÄÄ¶‚·‚é
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BGMController : MonoBehaviour
    {
        [Tooltip("Ÿ—˜‚ÉÄ¶‚³‚ê‚éBGM")]
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