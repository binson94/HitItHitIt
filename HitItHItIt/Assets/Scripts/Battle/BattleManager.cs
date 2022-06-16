using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yeol
{
    ///<summary> FSM, 사진 참조 </summary>
    public enum UserState
    {
        Load, Attack, AttackEnd, Dodge, DodgeEnd, Win, Lose
    }

    ///<summary> 각 버튼에 해당하는 나열형 </summary>
    public enum CommandToken
    {
        LJap, LHook, LUpper, LDucking, RJap, RHook, RUpper, RDucking
    }

    public class BattleManager : MonoBehaviour
    {
        ///<summary> 입력할 커맨드가 채워짐 </summary>
        List<CommandToken> tokensQueue = new List<CommandToken>();
        ///<summary> 현재 state 표시, Start -> Attack -> Dodge -> Attack ... </summary>
        [SerializeField] UserState userState = UserState.Load;
        ///<summary> Attack, Dodge State Timer </summary>
        Coroutine timer = null;

        #region ShowUI
        [SerializeField] Text startTxt;
        ///<summary> state 남은 시간 표시 텍스트 </summary>
        [SerializeField] Text timerTxt;
        ///<summary> 0 ~ 3 Left, 4 ~ 7 Right, Jap, Hook, Upper, Ducking 순 </summary>
        [Tooltip("0 ~ 3 Left, 4 ~ 7 Right, Jap, Hook, Upper, Ducking 순")]
        [SerializeField] Sprite[] tokenSprites;
        ///<summary> tokenQueue의 이미지가 할당될 실제 이미지 오브젝트(Attack State) </summary>
        [SerializeField] Image[] attackTokenImages;
        ///<summary> tokenQueue의 이미지가 할당될 실제 이미지 오브젝트(Dodge State) </summary>
        [SerializeField] Image[] dodgeTokenImages;

        ///<summary> 플레이어 체력 이미지 3개, 2 -> 1 -> 0 순으로 사라짐 </summary>
        [SerializeField] GameObject[] playerhpImages;
        ///<summary> 적 체력 보여주는 슬라이더 </summary>
        [SerializeField] SliderImage enemyhpSlider;
        ///<summary> 스테미나 보여주는 슬라이더 </summary>
        [SerializeField] SliderImage staminaSlider;

        bool isPause = false;
        [SerializeField] GameObject pausePanel;
        #endregion ShowUI

        #region CharacterStatus
        ///<summary> 한 Attack State에서 입력할 수 있는 최대 커맨드 수 </summary>
        int stamina = 10;
        ///<summary> 0 ~ 2 Left, 4 ~ 6 Right, Jap, Hook, Upper 순 각 피해, 3번은 결번 </summary>
        int[] dmgs = new int[7] { 1, 1, 1, 0, 1, 1, 1 };
        ///<summary> 플레이어 체력 </summary>
        int hp = 3;

        ///<summary> 적 체력 </summary>
        int enemyHp = 20;
        ///<summary> attack State 지속 시간, 만료 시 dodge State로 넘어감 </summary>
        int attackStateTime = 10;
        ///<summary> dodge State 지속 시간, 만료 시 피해 입음 </summary>
        int dodgeStateTime = 10;
        ///<summary> dodge State에서 제시되는 커맨드 수 </summary>
        int dodgeCommandCount = 10;

        ///<summary> 남은 스테미나, Attack State 시작 시 초기화 </summary>
        int currStamina = 10;
        #endregion CharacterStatus

        #region Animation
        [SerializeField] Animator playerAnimator;
        [SerializeField] Animator enemyAnimator;
        #endregion

        private void Start() {
            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(false);
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);

            enemyhpSlider.SetMax(enemyHp);
            staminaSlider.SetMax(stamina);

            SoundMgr.instance.StopBGM();
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
            StartAttackState();
        }
       
       ///<summary> 디버그 용, 키보드 입력과 버튼 대응 </summary>
        private void Update() {
            if(Input.GetKeyDown(KeyCode.S))
                OnBtnClick(0);
            else if(Input.GetKeyDown(KeyCode.D))
                OnBtnClick(1);
            else if(Input.GetKeyDown(KeyCode.W))
                OnBtnClick(2);
            else if(Input.GetKeyDown(KeyCode.A))
                OnBtnClick(3);
            else if(Input.GetKeyDown(KeyCode.DownArrow))
                OnBtnClick(4);
            else if(Input.GetKeyDown(KeyCode.LeftArrow))
                OnBtnClick(5);
            else if(Input.GetKeyDown(KeyCode.UpArrow))
                OnBtnClick(6);
            else if(Input.GetKeyDown(KeyCode.RightArrow))
                OnBtnClick(7);
        }

        #region AttackState
        ///<summary> Attack State 시작(dodge Token Image들 숨김, 누적 피해, 스테미나 초기화, attack Token Image 생성, 타이머 시작) </summary>
        void StartAttackState()
        {
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);

            currStamina = stamina;
            staminaSlider.SetValue(stamina);
            staminaSlider.gameObject.SetActive(true);

            FillAttackQueue();
            userState = UserState.Attack;
            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(true);
            ImageUpdate();

            timer = StartCoroutine(AttackTimer(attackStateTime));
        }
        IEnumerator AttackTimer(int timer)
        {
            while (timer > 0)
            {
                timerTxt.text = timer.ToString();
                yield return new WaitForSeconds(1);
                timer--;
            }

            userState = UserState.AttackEnd;
            StartCoroutine(AttackToDodgeDelay());

            yield return null;
        }
        
        ///<summary> 공격 애니메이션 재생 코루틴, 성공 여부는 매개변수로 받음 </summary>
        IEnumerator AttackToDodgeDelay()
        {
            yield return new WaitForSeconds(1f);

            StartDodgeState();
        }
        #endregion AttackState
        #region DodgeState
        ///<summary> Dodge State 시작(attack Token Image들 숨김, dodge Token Image 생성, 타이머 시작) </summary>
        void StartDodgeState()
        {
            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(false);

            FillDodgeQueue();
            userState = UserState.Dodge;
            foreach(Image i in dodgeTokenImages)
                i.gameObject.SetActive(true);
            ImageUpdate();

            staminaSlider.gameObject.SetActive(false);

            timer = StartCoroutine(DodgeTimer(dodgeStateTime));
        }
        IEnumerator DodgeTimer(int timer)
        {
            while (timer > 0)
            {
                timerTxt.text = timer.ToString();
                timer--;
                yield return new WaitForSeconds(1);
            }

            DodgeTimerExpired();
            yield return null;
        }
        ///<summary> 회피 타이머 만료 또는 잘못된 커맨드 입력 시 호출(회피를 실패한 경우), 피해 입고 lose 판정, 생존 시 Attack State로 전환 </summary>
        void DodgeTimerExpired()
        {
            userState = UserState.DodgeEnd;

            StartCoroutine(DodgeToAttackDelay(false));
        }

        ///<summary> 회피 애니메이션 재생 코루틴, 성공 여부는 매개변수로 받음 </summary>
        IEnumerator DodgeToAttackDelay(bool isSuccess)
        {
            bool lose = false;

            if (!isSuccess)
            {
                playerhpImages[--hp].SetActive(false);
                lose = hp <= 0;
            }

            yield return new WaitForSeconds(1f);

            if (lose)
            {
                userState = UserState.Lose;
                Lose();
            }
            else
                StartAttackState();
        }       
        #endregion DodgeState

        ///<summary> 좌우 커맨드 버튼 8개에 대응하는 함수 </summary>
        ///<param name="btnIdx"> 0 ~ 3 Left, 4 ~ 7 Right : 잽, 훅, 어퍼, 더킹 순서, 버튼에 이미 할당되어 있음 </param>
        public void OnBtnClick(int btnIdx)
        {
            if(isPause) return;

            if (userState == UserState.Attack)
                OnBtnAttackState((CommandToken)btnIdx);
            else if (userState == UserState.Dodge)
                OnBtnDodgeState((CommandToken)btnIdx);

            ///<summary> Attack State에서 버튼 입력 처리 </summary>
            void OnBtnAttackState(CommandToken inputToken)
            {
                //입력 성공
                if (inputToken == tokensQueue[0])
                {
                    if(inputToken < CommandToken.LDucking)
                        playerAnimator.Play("Hand LeftAtk");
                    else
                        playerAnimator.Play("Hand RightAtk");

                    //피해 누적 및 제일 앞 Token 제거
                    enemyHp -= dmgs[(int)inputToken];
                    if(enemyHp < 0)
                        enemyHp = 0;
                    enemyhpSlider.SetValue(enemyHp);
                    
                    tokensQueue.RemoveAt(0);
                    tokensQueue.Add(GetAttackToken());
                    ImageUpdate();

                    staminaSlider.SetValue(--currStamina);

                    if(enemyHp <= 0)
                    {
                        userState = UserState.Win;
                        StopCoroutine(timer);
                        Win();
                    }
                    //스테미나 모두 소진 시, 타이머 만료와 같은 동작
                    else if(currStamina <= 0)
                    {
                        userState = UserState.AttackEnd;
                        StopCoroutine(timer);
                        StartCoroutine(AttackToDodgeDelay());
                    }

                }
                //입력 실패 -> 누적 피해 사라짐, 공격 애니메이션 재생
                else
                {
                    userState = UserState.AttackEnd;
                    StopCoroutine(timer);
                    StartCoroutine(AttackToDodgeDelay());
                }
            }
            ///<summary> Dodge State에서 버튼 입력 처리 </summary>
            void OnBtnDodgeState(CommandToken inputToken)
            {
                if(tokensQueue[0] == CommandToken.LDucking)
                    enemyAnimator.Play("Enemy LeftAtk");
                else
                    enemyAnimator.Play("Enemy RightAtk");

                //입력 성공
                if (inputToken == tokensQueue[0])
                {
                    //제일 앞 토큰 제거
                    tokensQueue.RemoveAt(0);
                    ImageUpdate();

                    //모든 토큰 입력 완료 -> 회피 성공 애니메이션 재생, Attack State로 전환
                    if(tokensQueue.Count <= 0)
                    {
                        userState = UserState.DodgeEnd;
                        StopCoroutine(timer);
                        StartCoroutine(DodgeToAttackDelay(true));
                    }
                }
                //입력 실패 -> 타이머 만료와 같은 동작
                else
                {
                    userState = UserState.DodgeEnd;
                    StopCoroutine(timer);
                    DodgeTimerExpired();
                }
            }
        }

        ///<summary> 일시 정지, 재시작 버튼 </summary>
        public void OnBtnPause()
        {
            if(userState == UserState.Load || userState == UserState.Win || userState == UserState.Lose) return;

            isPause = !isPause;
            pausePanel.SetActive(isPause);
            Time.timeScale = isPause ? 0 : 1;
        }

        ///<summary> 타이틀로 돌아가기 버튼 </summary>
        public void OnBtnBackToTitle() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        ///<summary> State에 따라 맞는 Image Token 업데이트 </summary>
        void ImageUpdate()
        {
            if (userState == UserState.Attack)
            {
                for (int i = 0; i < attackTokenImages.Length; i++)
                    attackTokenImages[i].sprite = tokenSprites[(int)tokensQueue[i]];
            }
            else if (userState == UserState.Dodge)
            {
                int i;
                for (i = 0; i < dodgeTokenImages.Length && i < tokensQueue.Count; i++)
                    dodgeTokenImages[i].sprite = tokenSprites[(int)tokensQueue[i]];
                for (; i < dodgeTokenImages.Length; i++)
                    dodgeTokenImages[i].gameObject.SetActive(false);
            }

        }

        void Win()
        {
            Debug.Log("win");
            SoundMgr.instance.PlayBGM(BGMList.Win);
        }
        void Lose()
        {
            Debug.Log("lose");
            SoundMgr.instance.PlayBGM(BGMList.Lose);
        }

        #region TokenQueue Actions
        ///<summary> Attack State에서, 토큰 생성하여 채워넣기 </summary>
        void FillAttackQueue()
        {
            tokensQueue.Clear();

            for (int i = 0; i < 10; i++)
                tokensQueue.Add(GetAttackToken());
        }
        ///<summary> Attack State에서 확률에 따라 다음 입력 토큰 생성 </summary>
        void FillDodgeQueue()
        {
            tokensQueue.Clear();

            for (int i = 0; i < dodgeCommandCount; i++)
                tokensQueue.Add(GetDodgeToken());
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
        ///<summary> Dodge State에서 확률에 따라 다음 입력 토큰 생성 </summary>
        CommandToken GetDodgeToken() => Random.Range(0, 100) < 50 ? CommandToken.LDucking : CommandToken.RDucking;
        #endregion TokenQueue Actions
    }
}