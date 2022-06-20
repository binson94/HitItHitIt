using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    private static GameObject container;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                container = new GameObject();
                container.name = "GameManager";
                _instance = container.AddComponent<GameManager>();
                DontDestroyOnLoad(container);

                _instance.LoadData();
            }

            return _instance;
        }
    }

    GameData _gamedata = new GameData();
    ///<summary> 외부 접근용, 외부에서는 수정 불가하도록 set은 설정하지 않음 </summary>
    public GameData gameData{
        get{
            return _gamedata;
        }
    }
    void LoadData()
    {
        _gamedata.money = PlayerPrefs.GetInt("Money", 500);
        for(int i = 0;i < 4;i++) _gamedata.powLvls[i] = PlayerPrefs.GetInt($"PowLvl{i}", 1);
        _gamedata.stage = PlayerPrefs.GetInt("Stage", 1);
    }
    ///<summary> 전투 승리, 돈벌기 씬에서 호출, 돈 획득 후 데이터 저장 </summary>
    public void EarnMoney(int amount)
    {
        _gamedata.money += amount;
        PlayerPrefs.SetInt("Money", _gamedata.money);
    }
    public void Upgrade(int spend, int skillIdx)
    {
        PlayerPrefs.SetInt("Money", _gamedata.money -= spend);
        PlayerPrefs.SetInt($"PowLvl{skillIdx}",  ++_gamedata.powLvls[skillIdx]);
    }
}

public class GameData
{
    public int money;
    ///<summary> 0 jap, 1 hook, 2 upper, 3 stamina </summary>
    public int[] powLvls = new int[4];
    public int stage;
}
