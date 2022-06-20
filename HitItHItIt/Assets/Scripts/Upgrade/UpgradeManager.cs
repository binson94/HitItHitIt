using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    ///<summary> 현재 보유하고 있는 돈 표시 텍스트 </summary>
    [SerializeField] Text holdingMoneyTxt;
    ///<summary> 현재 레벨 표시 텍스트
    ///<para> 0 jap, 1 hook, 2 upper, 3 stamina </para>
    ///</summary>
    [SerializeField] Text[] nowPowerLevelTxts;
    ///<summary> 업그레이드 시 필요한 돈 표시 텍스트 </summary>
    [SerializeField] Text[] moneyNeededTxts;

    ///<summary> 업그레이드 비용 부족 시 표시할 텍스트 </summary>
    [SerializeField] Text moneyLackTxt;
    IEnumerator ShowMoneyLackText()
    {
        moneyLackTxt.text = "보유한 G가 부족합니다!";

        yield return new WaitForSeconds(3f);
        moneyLackTxt.text = "";
    }

    private void Start()
    {
        //텍스트 초기값 설정
        holdingMoneyTxt.text = $"보유한 G:{GameManager.instance.gameData.money}";
        moneyLackTxt.text = "";
        for (int i = 0; i < 4; i++)
        {
            nowPowerLevelTxts[i].text = "현재 강화 수치: " + GameManager.instance.gameData.powLvls[i];
            moneyNeededTxts[i].text = "G: " + GetCost(GameManager.instance.gameData.powLvls[i]);
        }

        SoundMgr.instance.PlayBGM(BGMList.Title);
    }

    ///<summary> 현재 레벨에 따른 업그레이드 비용 반환 </summary>
    int GetCost(int lvl)
    {
        return 100 * lvl;
    }

    ///<summary> 각 업그레이드 버튼에 할당, 업그레이드 시도 </summary>
    public void GetUpgrade(int whatToUpgrade)
    {
        int costNeeded = GetCost(GameManager.instance.gameData.powLvls[whatToUpgrade]);

        if (GameManager.instance.gameData.money >= costNeeded)
        {
            GameManager.instance.Upgrade(costNeeded, whatToUpgrade);
            holdingMoneyTxt.text = $"보유한 G:{GameManager.instance.gameData.money}";
            nowPowerLevelTxts[whatToUpgrade].text = "현재 강화 수치: " + GameManager.instance.gameData.powLvls[whatToUpgrade];
            moneyNeededTxts[whatToUpgrade].text = "G: " + GetCost(GameManager.instance.gameData.powLvls[whatToUpgrade]);
            moneyLackTxt.text = "";

            if(whatToUpgrade <= 2)
                SoundMgr.instance.PlaySFX(SFXList.Up_Punch);
            else
                SoundMgr.instance.PlaySFX(SFXList.Up_Stamina);
        }
        else
            StartCoroutine(ShowMoneyLackText());
    }

    public void Btn_GoToTitle() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);
}
