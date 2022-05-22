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
 
}


//0."돈벌자!"(혹은 "싸우자!")에서 재화가 담긴 스크립트를 가져와 돈 변수를 찾는다
//1.화면상에서 현재 지닌 재화와 6개의 공격버튼을 제시한다.
//2.강화를 시키는데 충분한 재화를 가진 버튼의 경우, 테두리가 빛나게 한다. 
//3.(강화가 가능하든, 불가능하든)버튼을 누를 시, 강화하겠냐는 버튼과 취소 버튼이 뜬다.
//4.강화하자! 버튼을 누를 시, 요구되는 수량만큼 재화를 차감하고, 공격버튼의 수준을 1+늘려서 해당되는 수준의 공격력을 가지게 한다. 
//그와 동시에 다음 강화를 위한 재화 또한 일정비율로 값을 상승시킨다. 

