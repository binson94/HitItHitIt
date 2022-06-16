using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Yeol
{
    public class TitleManager : MonoBehaviour
    {
        //serialize Field - https://docs.unity3d.com/kr/530/ScriptReference/SerializeField.html (private 변수도 유니티 인스펙터에서 보이게)
        [SerializeField] Slider bgmSlider;
        [SerializeField] Slider sfxSlider;

        private void Start()
        {
            LoadSoundOption();
            SoundMgr.instance.PlayBGM(BGMList.Title);
        }

        //게임 최초 실행 시, PlayerPrefs로 저장된 사운드 옵션 불러옴
        void LoadSoundOption()
        {
            float bgm = PlayerPrefs.GetFloat("BGM", 1);
            float sfx = PlayerPrefs.GetFloat("BGM", 1);
            SoundMgr.instance.SetBGM(bgm);
            SoundMgr.instance.SetSFX(sfx);

            bgmSlider.value = bgm;
            sfxSlider.value = sfx;
        }

        //bgm 슬라이더에 할당, 슬라이더의 값은 0.0001 ~ 1로 범위 제한
        public void SetBGM() { SoundMgr.instance.SetBGM(bgmSlider.value); }
        //sfx 슬라이더에 할당, 슬라이더의 값은 0.0001 ~ 1로 범위 제한
        public void SetSFX() { SoundMgr.instance.SetSFX(sfxSlider.value); }
        public void SceneLoad(int sceneIndex) { SceneManager.LoadScene(sceneIndex); }
        public void ExitScene() { Application.Quit(); }
    }
}