/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomobject : MonoBehaviour
{
    //랜덤 enemy 이미지 생성 (나무, 돌, 광석)

    public GameObject Enemys;
    public Image DrawImage; // 랜덤 이미지를 출력할 오브젝트
    public Sprite Image1;
    public Sprite Image2;
    public Sprite Image3;
    public int RandomInt; // 랜덤 변수
    void Update()
    {
        RandomInt = Random.Range(0, 3);
    }
    public void choose()
    {
        Enemys.SetActive(true);
        if (RandomInt == 1) DrawImage.sprite = Image1;
        else if (RandomInt == 2) DrawImage.sprite = Image2;
        else if (RandomInt == 3) DrawImage.sprite = Image3;

    }


}
*/