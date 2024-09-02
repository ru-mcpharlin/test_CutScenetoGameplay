using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class SFXController : MonoBehaviour
    {
        [HideInInspector] PlayerManager _manager;

        /********************************* SFX ****************************//*
        #region SFX
        [Space]
        [Header("SFX")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        #endregion*/

        private void Awake()
        {
            _manager = GetComponent<PlayerManager>();
        }

        /************ SFX **************/
        #region SFX
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (_manager._data.FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, _manager._data.FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(_manager._data.FootstepAudioClips[index], transform.TransformPoint(_manager._character.center), _manager._data.FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(_manager._data.LandingAudioClip, transform.TransformPoint(_manager._character.center), _manager._data.FootstepAudioVolume);
            }
        }

        #endregion
    }
}