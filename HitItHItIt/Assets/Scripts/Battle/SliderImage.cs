using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yeol
{
    ///<summary> 스테미나 슬라이더, 적 체력 슬라이더를 위한 클래스 </summary>
    public class SliderImage : MonoBehaviour
    {
        Image fillImage;
        int maxValue;
        int currValue;

        //시작 시 자식 중 fillImage 얻음
        void Start() => fillImage = transform.GetChild(1).GetComponent<Image>();
        
        public void SetMax(int max) => currValue = maxValue = max;
        public void SetValue(int value)
        {
            currValue = value;
            fillImage.fillAmount = (float)value / maxValue;
        }
    }
}