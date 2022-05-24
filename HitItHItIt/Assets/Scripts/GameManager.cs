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
            }

            return _instance;
        }
    }

    public void LoadData()
    {
        Debug.Log("데이터 로드");
    }
}
