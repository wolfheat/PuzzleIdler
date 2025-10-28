using System;
using System.Collections.Generic;
using UnityEngine;

namespace WolfheatProductions.SoundMaster
{
    public class SoundMaster : MonoBehaviour
	{
		public static SoundMaster Instance { get; private set; }

		[SerializeField] private SoundLibrary musicLibrary;
		[SerializeField] private SoundLibrary[] soundLibraries;

		// Outside Access
		public SoundLibrary MusicLibrary => musicLibrary;
		public SoundLibrary[] SoundLibraries => soundLibraries;


        // Dictionaries keep all items with fast lookup
        private Dictionary<string, Sound> soundDictionary = new();
		private Dictionary<string, Sound> musicDictionary = new();

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

			GenerateDictionaries();
		}

        private void GenerateDictionaries()
        {
			// Read all Sound Libraries and add them to the Dictionaries
			foreach (var library in soundLibraries) {
				foreach (var sound in library.Sounds) {
					string soundName = sound.name;
					if (soundDictionary.ContainsKey(soundName)) {
						Debug.Log("Duplicate Sound Entry "+soundName);
						continue;
					}
					// Add it
					soundDictionary[soundName] = sound;
				}
			}
			Debug.Log("Added " + soundDictionary.Count + " individual sounds.");

			// Same for Music
			foreach (var sound in musicLibrary.Sounds) {
				string soundName = sound.name;
				if (musicDictionary.ContainsKey(soundName)) {
					Debug.Log("Duplicate Msuic Entry "+soundName);
					continue;
				}
                // Add it
                musicDictionary[soundName] = sound;
			}
			Debug.Log("Added " + musicDictionary.Count + " individual music.");


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

		private MusicName activeMusic;

		private void Start()
		{
			Debug.Log("Sound Master Start");

			// Play Music?
			PlaySound(SoundName.MenuChangeSuccess);

            // Music
            //PlayMusic(MusicName);
            activeMusic = MusicName.MainMusic;
        }

        // Using Enum to get the same String by name
        public void PlayMusic(MusicName musicName) => PlayMusic(musicName.ToString());
        public void PlayMusic(string musicString)
        {
            Debug.Log("Trying to play sound: " + musicString);

            // Find the sound by string
            if (!musicDictionary.ContainsKey(musicString)) {
                Debug.Log("Could not find sound named: " + musicString);
                return;
            }
            // Set its values
            Sound sound = musicDictionary[musicString];
				
			AudioClip clip = sound.clips[0];
			musicAudioSource.clip = clip;
			musicAudioSource.volume = sound.volume;
			musicAudioSource.pitch = sound.pitch;
			musicAudioSource.loop = true;
			musicAudioSource.Play();
        }

        // Using Enum to get the same String by name
        public void PlaySound(SoundName soundName) => PlaySound(soundName.ToString());

        public void PlaySound(string soundString)
		{
			Debug.Log("Trying to play sound: "+soundString);


            // Find the sound by string
            if (!soundDictionary.ContainsKey(soundString)) {
                Debug.Log("Could not find sound named: " + soundString);
                return;
            }
			Sound sound = soundDictionary[soundString];

			// Play it
			PlaySound(sound);
		}

        public void PlaySound(Sound sound)
        {			
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

        internal void ActivateMusic()
        {
			PlayMusic(activeMusic);
        }

        internal void MuteMusic()
        {
			// Stop any playing music
			musicAudioSource.Stop();
        }
    }
}

