using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yeol
{
    ///<summary> FSM, 사진 참조 </summary>
    public enum UserState
    {
        Load, Attack, AttackAnim, Dodge, DodgeAnim, Win, Lose
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

        #region Show UI
        ///<summary> 0 timer, 1 hp, 2 enemyhp </summary>
        [Tooltip("0 timer, 1 hp, 2enemyhp")]
        [SerializeField] Text[] uiTxts;
        [SerializeField] GameObject startBtn;
        ///<summary> 0 ~ 3 Left, 4 ~ 7 Right, Jap, Hook, Upper, Ducking 순 </summary>
        [Tooltip("0 ~ 3 Left, 4 ~ 7 Right, Jap, Hook, Upper, Ducking 순")]
        [SerializeField] Sprite[] tokenSprites;
        ///<summary> tokenQueue의 이미지가 할당될 실제 이미지 오브젝트(Attack State) </summary>
        [SerializeField] Image[] attackTokenImages;
        ///<summary> tokenQueue의 이미지가 할당될 실제 이미지 오브젝트(Dodge State) </summary>
        [SerializeField] Image[] dodgeTokenImages;
        #endregion

        #region CharacterStatus
        ///<summary> 한 Attack State에서 입력할 수 있는 최대 커맨드 수 </summary>
        int stamina = 10;
        ///<summary> 0 ~ 2 Left, 4 ~ 6 Right, Jap, Hook, Upper 순 각 피해, 3번은 결번 </summary>
        int[] dmgs = new int[7] { 1, 1, 1, 0, 1, 1, 1 };
        ///<summary> 플레이어 체력 </summary>
        int hp = 10;

        ///<summary> 적 공격력, 회피 실패 시 피해 입음 </summary>
        int enemyAtk = 5;
        ///<summary> 적 체력 </summary>
        int enemyHp = 50;
        ///<summary> attack State 지속 시간, 만료 시 dodge State로 넘어감 </summary>
        int attackStateTime = 6;
        ///<summary> dodge State 지속 시간, 만료 시 피해 입음 </summary>
        int dodgeStateTime = 6;
        ///<summary> dodge State에서 제시되는 커맨드 수 </summary>
        int dodgeCommandCount = 10;
        #endregion CharacterStatus
        ///<summary> Attack State에서 올바른 커맨드 입력 시 데미지 누적 </summary>
        int accumulateDamage;
        ///<summary> 남은 스테미나, Attack State 시작 시 초기화 </summary>
        int currStamina;

        private void Start() {
            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(false);
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);

            uiTxts[1].text = hp.ToString();
            uiTxts[2].text= enemyHp.ToString();
        }
       
       ///<summary> 디버그 용, 키보드 입력과 버튼 대응 </summary>
        private void Update() {
            if(Input.GetKeyDown(KeyCode.D))
                OnBtnClick(0);
            else if(Input.GetKeyDown(KeyCode.S))
                OnBtnClick(1);
            else if(Input.GetKeyDown(KeyCode.A))
                OnBtnClick(2);
            else if(Input.GetKeyDown(KeyCode.W))
                OnBtnClick(3);
            else if(Input.GetKeyDown(KeyCode.LeftArrow))
                OnBtnClick(4);
            else if(Input.GetKeyDown(KeyCode.DownArrow))
                OnBtnClick(5);
            else if(Input.GetKeyDown(KeyCode.RightArrow))
                OnBtnClick(6);
            else if(Input.GetKeyDown(KeyCode.UpArrow))
                OnBtnClick(7);
        }

        ///<summary> 전투 시작 버튼 누를 때 호출 </summary>
        public void OnStartBtnClick()
        {
            startBtn.SetActive(false);
            StartAttackState();
        }

        #region AttackState
        ///<summary> Attack State 시작(dodge Token Image들 숨김, 누적 피해, 스테미나 초기화, attack Token Image 생성, 타이머 시작) </summary>
        void StartAttackState()
        {
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);

            accumulateDamage = 0;
            currStamina = stamina;

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
                uiTxts[0].text = timer.ToString();
                yield return new WaitForSeconds(1);
                timer--;
            }

            AttackTimerExpired();
            yield return null;
        }
        ///<summary> 공격 타이머 만료 또는 스테미나 모두 소진 시 호출(공격을 성공한 경우), true로 공격 애니메이션 재생 </summary>
        void AttackTimerExpired()
        {
            userState = UserState.AttackAnim;

            StartCoroutine(PlayAttackAnimation(true));
        }
        
        ///<summary> 공격 애니메이션 재생 코루틴, 성공 여부는 매개변수로 받음 </summary>
        IEnumerator PlayAttackAnimation(bool isSuccess)
        {
            bool win = false;
            if (isSuccess && accumulateDamage > 0)
            {
                PlaySuccessAnim();

                win = GiveDamage();
                uiTxts[2].text = enemyHp.ToString();
            }
            else
                PlayFailAnim();

            yield return new WaitForSeconds(2f);



            if (win)
            {
                userState = UserState.Win;
                Win();
            }
            else
                StartDodgeState();

            bool GiveDamage() => (enemyHp -= accumulateDamage) <= 0;
            void PlaySuccessAnim() { Debug.Log("공격 성공 애니메이션"); }
            void PlayFailAnim() { Debug.Log("공격 실패 애니메이션"); }
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

            timer = StartCoroutine(DodgeTimer(dodgeStateTime));
        }
        IEnumerator DodgeTimer(int timer)
        {
            while (timer > 0)
            {
                uiTxts[0].text = timer.ToString();
                timer--;
                yield return new WaitForSeconds(1);
            }

            DodgeTimerExpired();
            yield return null;
        }
        ///<summary> 회피 타이머 만료 또는 잘못된 커맨드 입력 시 호출(회피를 실패한 경우), 피해 입고 lose 판정, 생존 시 Attack State로 전환 </summary>
        void DodgeTimerExpired()
        {
            userState = UserState.DodgeAnim;

            StartCoroutine(PlayDodgeAnimation(false));
        }

        ///<summary> 회피 애니메이션 재생 코루틴, 성공 여부는 매개변수로 받음 </summary>
        IEnumerator PlayDodgeAnimation(bool isSuccess)
        {
            bool lose = false;

            if (isSuccess)
                PlaySuccessAnim();
            else
            {
                PlayFailAnim();
                lose = GetDamage();
                uiTxts[1].text = hp.ToString();
            }

            yield return new WaitForSeconds(2f);

            if (lose)
            {
                userState = UserState.Lose;
                Lose();
            }
            else
                StartAttackState();

            bool GetDamage() => (hp -= enemyAtk) <= 0;
            void PlaySuccessAnim() { Debug.Log("회피 성공 애니메이션");}
            void PlayFailAnim() { Debug.Log("회피 실패 애니메이션");}
        }       
        #endregion DodgeState

        ///<summary> 좌우 커맨드 버튼 8개에 대응하는 함수 </summary>
        ///<param name="btnIdx"> 0 ~ 3 Left, 4 ~ 7 Right : 잽, 훅, 어퍼, 더킹 순서, 버튼에 이미 할당되어 있음 </param>
        public void OnBtnClick(int btnIdx)
        {
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
                    //피해 누적 및 제일 앞 Token 제거
                    accumulateDamage += dmgs[(int)inputToken];
                    tokensQueue.RemoveAt(0);
                    tokensQueue.Add(GetAttackToken());
                    ImageUpdate();

                    //스테미나 모두 소진 시, 타이머 만료와 같은 동작
                    if(--currStamina <= 0)
                    {
                        userState = UserState.AttackAnim;
                        StopCoroutine(timer);
                        AttackTimerExpired();
                    }
                }
                //입력 실패 -> 누적 피해 사라짐, 공격 애니메이션 재생
                else
                {
                    userState = UserState.AttackAnim;
                    StopCoroutine(timer);
                    StartCoroutine(PlayAttackAnimation(false));
                }
            }
            ///<summary> Dodge State에서 버튼 입력 처리 </summary>
            void OnBtnDodgeState(CommandToken inputToken)
            {
                //입력 성공
                if (inputToken == tokensQueue[0])
                {
                    //제일 앞 토큰 제거
                    tokensQueue.RemoveAt(0);
                    ImageUpdate();

                    //모든 토큰 입력 완료 -> 회피 성공 애니메이션 재생, Attack State로 전환
                    if(tokensQueue.Count <= 0)
                    {
                        userState = UserState.DodgeAnim;
                        StopCoroutine(timer);
                        StartCoroutine(PlayDodgeAnimation(true));
                    }
                }
                //입력 실패 -> 타이머 만료와 같은 동작
                else
                {
                    userState = UserState.DodgeAnim;
                    StopCoroutine(timer);
                    DodgeTimerExpired();
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
        }
        void Lose()
        {
            Debug.Log("lose");
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