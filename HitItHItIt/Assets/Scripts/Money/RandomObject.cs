/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomobject : MonoBehaviour
{
    //���� enemy �̹��� ���� (����, ��, ����)

    public GameObject Enemys;
    public Image DrawImage; // ���� �̹����� ����� ������Ʈ
    public Sprite Image1;
    public Sprite Image2;
    public Sprite Image3;
    public int RandomInt; // ���� ����
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