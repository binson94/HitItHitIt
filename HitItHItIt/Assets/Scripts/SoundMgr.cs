using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMgr : MonoBehaviour
{
    static SoundMgr _instance = null;
    static GameObject container;

    //get, set - https://novlog.tistory.com/43
    //singleton - https://glikmakesworld.tistory.com/2
    ///<summary> 외부 접근자 - 외부에서 접근 시 _instance가 null이면 초기 생성 </summary>
    public static SoundMgr instance
    {
        get
        {
            if(_instance == null)
            {
                //SoundMgr gameObject 생성, container에 저장해둠
                container = new GameObject();
                container.name = "Sound Mgr";
                _instance = container.AddComponent(typeof(SoundMgr)) as SoundMgr;

                //audio mixer load
                mixer = Resources.Load<AudioMixer>("Sounds/AudioMixer");

                //bgm gameObject 생성
                GameObject tmp = CreateAudioSource("BGM", bgmSource, true);
                tmp.transform.SetParent(container.transform);

                //sfx gameObject 생성
                tmp = CreateAudioSource("SFX", sfxSource, false);
                tmp.transform.SetParent(container.transform);

                LoadClips();

                //씬이 넘어가도 사라지지 않게 설정 - child(BGM, SFX)도 영향 받아 사라지지 않음
                DontDestroyOnLoad(container);
            }

            return _instance;
        }
    }

    static AudioSource bgmSource;
    static AudioClip[] bgmClips;
    static AudioSource sfxSource;
    static AudioClip[] sfxClips;
    static AudioMixer mixer;

    ///<summary> Audio Source gameObject 생성 후 초기화하여 반환하는 함수 </summary>
    static GameObject CreateAudioSource(string sourceName, AudioSource source, bool loop)
    {
        GameObject go = new GameObject();
        go.name = sourceName;
        source = go.AddComponent(typeof(AudioSource)) as AudioSource;
        source.loop = loop;
        source.outputAudioMixerGroup = mixer.FindMatchingGroups(sourceName)[0];
        source.playOnAwake = false;

        return go;
    }
    
    //Resources.Load - https://learnandcreate.tistory.com/753 (Resources 내 경로 제공 시 파일 불러오기 가능)
    static void LoadClips()
    {
        //bgmClips = new AudioClip[bgm 총 갯수];
        //for(int i = 0;i < bgm 총 갯수;i++)
            //bgmClips[i] = Resources.Load<AudioClip>("Sounds/"BGM 이름");

        //sfxClips = new AudipClip[sfx 총 갯수];
        //for(int i = 0;i < sfx 총 갯수;i++)
            //sfxClips[i] = Resources.Load<AudioClip>("Sounds/"SFX 이름");
    }

    ///<summary> BGM 크기 조절 함수 </summary>
    ///<param name="value"> 0.0001 ~ 1 사이 값, 함수에서 -80 ~ 0으로 조절 </param>
    public void SetBGM(float value)
    {
        mixer.SetFloat("BGM", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("BGM", value);
    }

    ///<summary> BGM 재생 함수 </summary>
    public void PlayBGM(int idx)
    {
        Debug.Log($"play BGM {idx}");
        //bgmSource.PlayOneShot(bgmClips[idx]);
    }

    ///<summary> SFX 크기 조절 함수 </summary>
    ///<param name="value"> 0.0001 ~ 1 사이 값, 함수에서 -80 ~ 0으로 조절 </param>
    public void SetSFX(float value)
    {
        mixer.SetFloat("SFX", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFX", value);
    }

    ///<summary> SFX 재생 함수 </summary>
    public void PlaySFX(int idx)
    {
        Debug.Log($"play SFX {idx}");
        //sfxSource.PlayOneShot(sfxClips[idx]);
    }
}
