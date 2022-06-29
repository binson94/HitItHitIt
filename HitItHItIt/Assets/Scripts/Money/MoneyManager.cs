using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Yeol
{
    public class MoneyManager : MonoBehaviour
    {
        ///<summary> 입력할 커맨드가 채워짐 </summary>
        List<CommandToken> tokensQueue = new List<CommandToken>();
        ///<summary> 현재 state 표시, Start -> Attack -> Dodge -> Attack ... </summary>
        UserState userState = UserState.Load;

        #region ShowUI
        [Header("UI")]
        [SerializeField] Text startTxt;
        ///<summary> 0 ~ 3 Left, 4 ~ 7 Right, Jap, Hook, Upper, Ducking 순 </summary>
        [Tooltip("0 ~ 3 Left, 4 ~ 7 Right, Jap, Hook, Upper, Ducking 순")]
        [SerializeField] Sprite[] tokenSprites;
        ///<summary> tokenQueue의 이미지가 할당될 실제 이미지 오브젝트(Attack State) </summary>
        [SerializeField] Image[] attackTokenImages;
        ///<summary> 스테미나 보여주는 슬라이더 </summary>
        [SerializeField] SliderImage staminaSlider;

        ///<summary> 현재까지 획득한 돈 표시 텍스트 </summary>
        [SerializeField] Text currMoneyTxt;

        ///<summary> 돈벌기 모드 종료 시 보여줄 UI Set </summary>
        [SerializeField] GameObject endPanel;
        [SerializeField] Text earnMoneyTxt;
        #endregion ShowUI

        #region CharacterStatus
        ///<summary> 한 Attack State에서 입력할 수 있는 최대 커맨드 수 </summary>
        int stamina = 10;
        ///<summary> 남은 스테미나, Attack State 시작 시 초기화 </summary>
        int currStamina = 10;

        int accumulatedDmg = 0;

        ///<summary> 0 ~ 2 Left, 4 ~ 6 Right, Jap, Hook, Upper 순 각 피해, 3번은 결번 </summary>
        int[] dmgs = new int[7] { 1, 1, 1, 0, 1, 1, 1 };
        #endregion CharacterStatus

        #region Animation
        [Header("Animation")]
        [SerializeField] Animator playerAnimator;

        [SerializeField] Animator resourceAnimator;
        [SerializeField] Image resourceImage;
        [SerializeField] Sprite[] resourceSprites;
        int enemyIdx;
        #endregion

        private void Start()
        {
            foreach (Image i in attackTokenImages)
                i.gameObject.SetActive(false);

            enemyIdx = Random.Range(0, 3);
            resourceImage.sprite = resourceSprites[enemyIdx * 4];

            for(int i = 0;i < 3; i++)
                dmgs[i] = dmgs[i + 4] = GameManager.instance.GetStat(i);
            stamina = GameManager.instance.GetStat(3);

            staminaSlider.SetMax(stamina);

            SoundMgr.instance.PlayBGM(BGMList.Money);
            StartCoroutine(WaitBeforeStart());
        }

        IEnumerator WaitBeforeStart()
        {
            int time = 3;

            while (time > 0)
            {
                startTxt.text = $"{time}초 후 시작";
                yield return new WaitForSeconds(1f);
                time--;
            }


            startTxt.gameObject.SetActive(false);
            StartAttack();
        }
        
        ///<summary> 디버그 용, 키보드 입력과 버튼 대응 </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
                OnBtnClick(0);
            else if (Input.GetKeyDown(KeyCode.D))
                OnBtnClick(1);
            else if (Input.GetKeyDown(KeyCode.W))
                OnBtnClick(2);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                OnBtnClick(4);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnBtnClick(5);
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                OnBtnClick(6);
        }

        ///<summary> Attack State 시작(스테미나 초기화, attack Token Image 생성, 타이머 시작) </summary>
        void StartAttack()
        {
            currStamina = stamina;
            staminaSlider.SetValue(stamina);
            staminaSlider.gameObject.SetActive(true);

            FillAttackQueue();
            userState = UserState.Attack;
            foreach (Image i in attackTokenImages)
                i.gameObject.SetActive(true);
            ImageUpdate();
        }

        ///<summary> 좌우 커맨드 버튼 8개에 대응하는 함수 </summary>
        ///<param name="btnIdx"> 0 ~ 3 Left, 4 ~ 7 Right : 잽, 훅, 어퍼, 더킹 순서, 버튼에 이미 할당되어 있음 </param>
        public void OnBtnClick(int btnIdx)
        {
            if (userState != UserState.Attack)
                return;

            CommandToken inputToken = (CommandToken)btnIdx;

            //입력 성공
            if (inputToken == tokensQueue[0])
            {
                PlayAnimation();
                resourceAnimator.Play("Resource_Hit");

                SoundMgr.instance.PlaySFX(SFXList.Punch);

                //피해 정도에 따라 돈 획득
                accumulatedDmg += dmgs[(int)inputToken];
                currMoneyTxt.text = $"{accumulatedDmg / 10}";

                tokensQueue.RemoveAt(0);
                tokensQueue.Add(GetAttackToken());
                ImageUpdate();

                staminaSlider.SetValue(--currStamina);

                if(currStamina < stamina / 4f)
                    resourceImage.sprite = resourceSprites[enemyIdx * 4 + 3];
                else if(currStamina < stamina / 2f)
                    resourceImage.sprite  = resourceSprites[enemyIdx * 4 + 2];
                else if(currStamina < stamina * 3f / 4)
                    resourceImage.sprite = resourceSprites[enemyIdx * 4 + 1];

                //스테미나 모두 소진 시, 타이머 만료와 같은 동작
                if (currStamina <= 0)
                {
                    userState = UserState.Win;
                    End();
                }

            }
            //입력 실패 -> 누적 피해 사라짐, 공격 애니메이션 재생
            else
            {
                userState = UserState.Win;
                End();
            }

            void PlayAnimation()
            {
                switch (inputToken)
                {
                    case CommandToken.LJap:
                        playerAnimator.Play("player_LJ");
                        break;
                    case CommandToken.LHook:
                        playerAnimator.Play("player_LH");
                        break;
                    case CommandToken.LUpper:
                        playerAnimator.Play("player_LU");
                        break;
                    case CommandToken.RJap:
                        playerAnimator.Play("player_RJ");
                        break;
                    case CommandToken.RHook:
                        playerAnimator.Play("player_RH");
                        break;
                    case CommandToken.RUpper:
                        playerAnimator.Play("player_RU");
                        break;
                }
            }
        }
       


        ///<summary> State에 따라 맞는 Image Token 업데이트 </summary>
        void ImageUpdate()
        {
            if (userState == UserState.Attack)
            {
                for (int i = 0; i < attackTokenImages.Length; i++)
                    attackTokenImages[i].sprite = tokenSprites[(int)tokensQueue[i]];
            }
        }

        void End()
        {
            foreach (Image i in attackTokenImages)
                i.gameObject.SetActive(false);

            earnMoneyTxt.text = $"{accumulatedDmg / 10} 골드 획득";
            GameManager.instance.EarnMoney(accumulatedDmg / 10);
            endPanel.SetActive(true);
        }

        public void Btn_GoToUpgrade() => SceneManager.LoadScene(2);
        public void Btn_GoToTitle() => SceneManager.LoadScene(0);

        #region TokenQueue Actions
        ///<summary> Attack State에서, 토큰 생성하여 채워넣기 </summary>
        void FillAttackQueue()
        {
            tokensQueue.Clear();

            for (int i = 0; i < 10; i++)
                tokensQueue.Add(GetAttackToken());
        }
        ///<summary> Attack State에서 확률에 따라 다음 입력 토큰 생성 </summary>
        CommandToken GetAttackToken()
        {
            int rand = Random.Range(0, 100);
            CommandToken command;

            if (rand < 15)
                command = CommandToken.LJap;
            else if (rand < 20)
                command = CommandToken.RJap;
            else if (rand < 30)
                command = CommandToken.LHook;
            else if (rand < 40)
                command = CommandToken.RHook;
            else if (rand < 70)
                command = CommandToken.LUpper;
            else
                command = CommandToken.RUpper;

            return command;
        }
        #endregion TokenQueue Actions
    }
}