using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

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
    
    ///<summary> Json Data Load </summary>
    // https://litjson.net/ - json 데이터 유니티 오브젝트로 바꿔주는 플러그인
    // https://www.convertcsv.com/csv-to-json.htm - 엑셀 파일 json string으로 바꿔주는 플러그인
    public void LoadData()
    {
        _gamedata.money = PlayerPrefs.GetInt("Money", 0);
        for(int i = 0;i < 4;i++) _gamedata.powLvls[i] = PlayerPrefs.GetInt($"PowLvl{i}", 1);
        _gamedata.stage = PlayerPrefs.GetInt("Stage", 1);
        _gamedata.enemy = PlayerPrefs.GetInt("Enemy", 0);

        JsonData json = JsonMapper.ToObject(Resources.Load<TextAsset>("Stat").text);

        for(int skill = 0;skill < 4;skill++)
            for(int lvl = 1;lvl <= 12;lvl++)
            {
                _gamedata.stats[skill, lvl] = (int)json[skill]["stat"][lvl - 1];
                _gamedata.upCosts[skill, lvl] = (int)json[skill]["cost"][lvl - 1];
            }
    }
    
    ///<summary> 현재 업그레이드 비용 반환 </summary>
    ///<param name="skill"> 스킬 idx(0 jap, 1 hook, 2 upper, 3 stamina) </param>
    public int GetCost(int skill)
    {
        return _gamedata.upCosts[skill, _gamedata.powLvls[skill]];
    }
    ///<summary> 현재 공격력 반환 </summary>
    ///<param name="skill"> 스킬 idx(0 jap, 1 hook, 2 upper, 3 stamina) </param>
    public int GetStat(int skill)
    {
        return _gamedata.stats[skill, _gamedata.powLvls[skill]];
    }
    
    ///<summary> 스테이지 클리어 시 다음 스테이지로 넘어감 </summary>
    public void IncreaseStage()
    {
        _gamedata.stage = Mathf.Min(_gamedata.stage + 1, 12);
        PlayerPrefs.SetInt("Stage", _gamedata.stage);
        _gamedata.enemy = (_gamedata.enemy + 1) % 3;
        PlayerPrefs.SetInt("Enemy", _gamedata.enemy);
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
    public int enemy;

    ///<summary> 0 jap, 1 hook, 2 upper, 3 stamina </summary>
    public int[,] stats = new int[4, 13];
    ///<summary> 0 jap, 1 hook, 2 upper, 3 stamina </summary>
    public int[,] upCosts = new int[4, 13];
}
