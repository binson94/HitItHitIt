using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{ 

//0,1,2 = Upper, Jap, Hook, 3 = stamina
    int[] powerLevel = {1, 1, 1, 1};
//임시로 정한 돈의 값.    
    int money = 10000;
    private Text moneyHave;
    public Text[] nowPowerLevel;
    public Text[] moneyNeeded;
    private Text moneyLack;
    IEnumerator ShowMoneyLackText()
    {
        moneyLack.text = "보유한 G가 부족합니다!";

        yield return new WaitForSeconds(3f);
        moneyLack.text = "";
    }

    private void Start()
    {
        moneyHave = GameObject.Find("MoneyText").GetComponent<Text>();
        moneyHave.text = "보유한 G:" + money.ToString();
        moneyLack = GameObject.Find("MoneyLack").GetComponent<Text>();
        moneyLack.text = "";
        for (int a=0 ; a<4  ; a++)
        {
        nowPowerLevel[a].text = "현재 강화 수치: " + powerLevel[a].ToString();
        moneyNeeded[a].text = "G: " + cost(100,powerLevel[a]).ToString();
        }   

    }

    private int level_Up(int a)
    {
        int b = a + 1;
        return b;
    }
//아직 강해지는 정도는 설정하지 않아서 단순히 1을 증가시키는 것으로 설정
//int a = 기존의 파워, int b = 현재 레벨 수준
    private int power(int a)
    {
        int b = 100*(a);
        return b;
    }
//int a = 기존의 가격, int b = 현재 레벨 수준
    private int cost(int a, int b)
    {
        int c = a*b ;
        return c;
    }

private void getUpgrade(int whatToUpgrade)
    {

        int costNeeded = cost(100,powerLevel[whatToUpgrade]);
        if ( money - costNeeded  >= 0)
        {
            powerLevel[whatToUpgrade] += 1; 
            money -= costNeeded;
            nowPowerLevel[whatToUpgrade].text = "현재 강화 수치: " + powerLevel[whatToUpgrade].ToString();
            moneyNeeded[whatToUpgrade].text = "G: " + cost(100,powerLevel[whatToUpgrade]).ToString();
            moneyHave.text = "보유한 G:" + money.ToString();
            moneyLack.text = "";   
        
        }

        else
        {
            StartCoroutine(ShowMoneyLackText());

        }



    }
 
}
