using System;
using System.Collections;
using UnityEngine;

namespace ETLab
{
    public class AudioManager : MonoBehaviour
    {
        AudioSource[] audio_source;
        AudioSource audioSource;
        AudioSource audio_source1;
        AudioSource audio_source2;

        bool fade_out;

        // 與實際檔名相同
        public enum AudioName
        {
            Story1_1,
            Story1_2,
            Story1_3,
            Story2_1,
            Story2_2,
            Story2_3,
            Story2_4,
            Correct,
            Game1,
            Game2,
            Game3,
            GameStart,
            Score,
            Scorll,
            StoryEnd,
            StoryStart,
            Wrong,
            Success,
            Fail
        }

        private void Awake()
        {
            audio_source = GetComponents<AudioSource>();
            audio_source1 = audio_source[0];
            audio_source2 = audio_source[1];
            fade_out = false;
        }

        private void Update()
        {
            if (fade_out)
            {
                audio_source1.volume -= Time.deltaTime / 10.0f;

                if (audio_source1.volume <= 0.1f)
                {
                    fade_out = false;
                    stop();
                }
            }
        }

        public void play(Enum file_name, int source = 1)
        {
            string audio_path = string.Format("_Audio/{0}", file_name);
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            audioSource.pitch = 1f;
            audioSource.PlayOneShot(Resources.Load<AudioClip>(audio_path));
        }

        public void playOnLoop(Enum file_name, int source = 1)
        {
            string audio_path = string.Format("_Audio/{0}", file_name);
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            audioSource.pitch = 1f;
            audioSource.clip = Resources.Load<AudioClip>(audio_path);
            audioSource.loop = true;
            audioSource.Play();

        }

        public void modifyVolumn(float sound, int source = 1)
        {
            sound /= 100f;
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            audioSource.volume = sound;
        }

        public void modifyPitch(float time_scale, int source = 1)
        {
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            audioSource.pitch = time_scale;
        }

        public void fadeOut()
        {
            fade_out = true;
        }

        public IEnumerator ieFadeOut(int source = 1, float minimum = 0.1f)
        {
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            while (audioSource.volume > minimum)
            {
                audioSource.volume -= Time.deltaTime / 10f;
                yield return new WaitForSeconds(Time.deltaTime);
            }

            stop();
        }

        public void pause(int source = 1)
        {
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            audioSource.Pause();
        }

        public void stop(int source = 1)
        {
            switch (source)
            {
                case 1:
                    audioSource = audio_source1;
                    break;
                case 2:
                    audioSource = audio_source2;
                    break;
            }

            audioSource.Stop();
        }
    }
}