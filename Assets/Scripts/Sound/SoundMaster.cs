using System;
using System.Collections.Generic;
using UnityEngine;

namespace WolfheatProductions.SoundMaster
{

	public enum MusicName {MainMusic}


    public class SoundMaster : MonoBehaviour
	{
		public static SoundMaster Instance { get; private set; }

		[SerializeField] private SoundLibrary effectsLibrary; 
		[SerializeField] private SoundLibrary musicLibrary;

		// Amount of allowed sounds at a time - can be changed, will cut off a sound if exceeding
		[SerializeField] private int effectsPoolSize = 20;

		// Audio Pool
		private List<AudioSource> effectsAudioPool;
		private AudioSource musicAudioSource;

		private int poolIndex = 0;

		private void Awake()
		{
			if (Instance != null) {
				Destroy(gameObject);
				return;
			}
			Instance = this;

			musicAudioSource = gameObject.AddComponent<AudioSource>();

			InitiatePool();
		}

		private void InitiatePool()
		{
			effectsAudioPool = new List<AudioSource>();
			for (int i = 0; i < effectsPoolSize; i++) {
				// Create a new Source on a child Gameobject
				GameObject gameObject = new GameObject();
				AudioSource audio = gameObject.AddComponent<AudioSource>();
				gameObject.transform.parent = transform;
				gameObject.name = "AudioSource_" + i;
				effectsAudioPool.Add(audio);

            }
			Debug.Log("Created Sound Pool of items: "+effectsAudioPool.Count);
		}

		private void Start()
		{
			Debug.Log("Sound Master Start");

			// Play Music?
			PlaySound(SoundName.MenuChangeSuccess);

			// Music
			PlayMusic(MusicName.MainMusic);

        }

        public void PlayMusic(MusicName music)
        {
			// Set its values
			Sound sound = musicLibrary.Sounds[(int)music];
				
			AudioClip clip = sound.clips[0];
			musicAudioSource.clip = clip;
			musicAudioSource.volume = sound.volume;
			musicAudioSource.pitch = sound.pitch;
            musicAudioSource.Play();
        }

        public void PlaySound(SoundName soundEnum)
        {
			// Convert the Enum into GUID
			if (!SoundLookup_Generated.IdToGuid.ContainsKey(soundEnum))
				return;
			string guid = SoundLookup_Generated.IdToGuid[soundEnum];

			Debug.Log("Playing Sound "+guid);

            Sound sound = effectsLibrary.GetSound(guid);

			AudioSource source = GetNextAudioSource();
            source.clip = sound.clips[UnityEngine.Random.Range(0,sound.clips.Length)];
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            source.Play();
        }

        private AudioSource GetNextAudioSource()
        {
            poolIndex = (poolIndex + 1) % effectsAudioPool.Count;
			return effectsAudioPool[poolIndex];
        }
    }
}

