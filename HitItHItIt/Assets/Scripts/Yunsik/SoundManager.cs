using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public AudioSource musicsource;

    public void SetMusicVolume(float volume)
    {
        musicsource.volume = volume;
    }

    // Playerprefs로 데이터를 저장
    // 볼륨 사이즈를 저장하여 다른 씬에도 적용되도록 설정
}
