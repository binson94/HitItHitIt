using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    ///<summary> 현재 보유하고 있는 돈 표시 텍스트 </summary>
    [SerializeField] Text holdingMoneyTxt;
    ///<summary> 업그레이드 시 필요한 돈 표시 텍스트 </summary>
    [SerializeField] Text[] moneyNeededTxts;

    [SerializeField] Animator[] sparkAnims;

    private void Start()
    {
        //텍스트 초기값 설정
        holdingMoneyTxt.text = $"{GameManager.instance.gameData.money}";
        for (int i = 0; i < 4; i++)
        {
            int lvl = GameManager.instance.gameData.powLvls[i];
            if (lvl >= 12)
                moneyNeededTxts[i].text = "MAX";
            else
                moneyNeededTxts[i].text = $"{GameManager.instance.GetCost(i)}";
        }

        SoundMgr.instance.PlayBGM(BGMList.Title);
    }

    ///<summary> 각 업그레이드 버튼에 할당, 업그레이드 시도 </summary>
    public void GetUpgrade(int whatToUpgrade)
    {
        int costNeeded = GameManager.instance.GetCost(whatToUpgrade);
        if(costNeeded <= 0) return;

        if (GameManager.instance.gameData.money >= costNeeded)
        {
            GameManager.instance.Upgrade(costNeeded, whatToUpgrade);
            if(whatToUpgrade == 3)
                sparkAnims[2].Play("Spark3");
            else
            {
                sparkAnims[0].Play("Spark1");
                sparkAnims[1].Play("Spark2");
            }

            holdingMoneyTxt.text = $"{GameManager.instance.gameData.money}";
            
            if (GameManager.instance.gameData.powLvls[whatToUpgrade] >= 12)
                moneyNeededTxts[whatToUpgrade].text = "MAX";
            else
                moneyNeededTxts[whatToUpgrade].text = $"{GameManager.instance.GetCost(whatToUpgrade)}";

            if (whatToUpgrade <= 2)
                SoundMgr.instance.PlaySFX(SFXList.Up_Punch);
            else
                SoundMgr.instance.PlaySFX(SFXList.Up_Stamina);
        }
    }

    public void Btn_GoToTitle() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);
}
