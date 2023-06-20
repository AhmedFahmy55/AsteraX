using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace FPS.Sounds
{
    public class SoundManager : Singelton<SoundManager>
    {


        [SerializeField] AudioMixerGroup music, vfx;
        [SerializeField] float vfxSpetialBlend = .8f;
        [SerializeField] SoundInfo[] soundInfos;




        public void PlaySound(Sound sound , Vector3 position)
        {
            GameObject obj = new GameObject("SoundFX");
            AudioSource audio = obj.AddComponent<AudioSource>();
            obj.transform.position = position;

            SoundInfo info = FindSoundData(sound);
            audio.clip = GetRandomClip(info);
            audio.playOnAwake = true;

            if(info.type == SoundType.VFX)
            {
                if (vfx != null) audio.outputAudioMixerGroup = vfx;
                audio.spatialBlend = vfxSpetialBlend;
            }
            else
            {
                if(music != null) audio.outputAudioMixerGroup = music;
                audio.spatialBlend = 0;
                audio.loop = true;
            }
            audio.Play();
            Destroy(obj, audio.clip.length);
        }

        private SoundInfo FindSoundData(Sound sound) 
        {
            foreach (var soundinfo in soundInfos)
            {
                if (soundinfo.name != sound) continue;
                
                return soundinfo;  
            }
            return soundInfos[0];
        }

        private static AudioClip GetRandomClip(SoundInfo soundinfo)
        {
            int index = UnityEngine.Random.Range(0, soundinfo.clips.Length);
            return soundinfo.clips[index];
        }

        public void SetVFXVloume(float value)
        {
            vfx.audioMixer.SetFloat("SoundEffects", value);
        }
        public void SetMusicVloume(float value)
        {
            music.audioMixer.SetFloat("Music", value);

        }
    }

    [System.Serializable]
    struct SoundInfo
    {
        public Sound name;
        public SoundType type;
        public AudioClip[] clips;
    }

}