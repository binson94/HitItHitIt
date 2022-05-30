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
                _instance = container.AddComponent(typeof(GameManager)) as GameManager;
                DontDestroyOnLoad(container);

                LoadData();
            }

            return _instance;
        }
    }

    static GameData _gamedata = new GameData();
    public GameData gameData
    {
        get
        {
            return _gamedata;
        }
    }

    static void LoadData()
    {
        _gamedata.money = PlayerPrefs.GetInt("Money", 0);
        for(int i = 0;i < 4;i++) _gamedata.powLvls[i] = PlayerPrefs.GetInt($"PowLvl{i}");
        _gamedata.stage = PlayerPrefs.GetInt("Stage", 1);
    }

    void SaveData()
    {
        PlayerPrefs.SetInt("Money", _gamedata.money);
        for(int i = 0;i < 4;i++) PlayerPrefs.SetInt($"PowLvl{i}", _gamedata.powLvls[i]);
        PlayerPrefs.SetInt("Stage", _gamedata.stage);
    }
}

public class GameData
{
    public int money;
    public int[] powLvls = new int[4];
    public int stage;
}
