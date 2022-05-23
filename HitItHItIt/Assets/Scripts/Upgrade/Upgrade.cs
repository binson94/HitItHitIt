using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    const int RUpper = 0;
    const int RJap = 1; 
    const int RHook = 2;
    const int LUpper = 3;
    const int LJap = 4;
    const int LHook = 5;
    const int stamina = 6;

    int initialCost = 100;    
    public GameObject panel;
    private bool UpgradeDisplay = false;


//0,1,2 = RUpper, RJap, RHook, 3,4,5 = LUpperm LJap, LHook로 설정
    int[] powerLevel = {1, 1, 1, 1, 1, 1,1};

    public int level_Up(int a)
    {
        int b = a + 1;
        return b;
    }
//아직 강해지는 정도는 설정하지 않아서 단순히 1을 증가시키는 것으로 설정
//int a = 기존의 파워, int b = 현재 레벨 수준
    public int power(int a)
    {
        int b = 100*(a);
        return b;
    }
//int a = 기존의 가격, int b = 현재 레벨 수준
    public int cost(int a, int b)
    {
        int c = a*b ;
        return c;
    }

    public void Game(int whatToUpgrade)
    {

        int costNeeded = cost(initialCost,powerLevel[whatToUpgrade]);
        int money = GameObject.Find("MoneyMge").GetComponent<TEMP_Money>().moneyToken;
        if(UpgradeDisplay == false)
        {
            panel.SetActive(true);
            UpgradeDisplay = true;
            if ( money - costNeeded  >= 0)
            {
                powerLevel[whatToUpgrade] += 1; 
                money -= costNeeded;
                Debug.Log("강화 성공, 남은 돈은 다음과 같습니다.:");
                Debug.Log(money);
                Debug.Log("현재 강화레벨:");               
                Debug.Log(powerLevel[whatToUpgrade]);
            }
            else
            {
                Debug.Log("돈이 부족합니다");
            }
        }
        else
        {
            panel.SetActive(false);
            UpgradeDisplay = false;
        }    


    }
 
}