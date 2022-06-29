using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;

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
        UserState userState = UserState.Load;
        ///<summary> Attack, Dodge State Timer </summary>
        Coroutine timer = null;
        float time = 0;

        #region ShowUI
        [Header("UI")]
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

        [SerializeField] Slider bgmSlider;
        bool isPause = false;
        [SerializeField] GameObject pausePanel;

        ///<summary> 승리 시 표시할 UI Set </summary>
        [SerializeField] GameObject winPanel;
        ///<summary> 승리 시 획득한 돈을 표시하기 위한 텍스트 </summary>
        [SerializeField] Text earnMoneyTxt;
        ///<summary> 패배 시 표시할 UI Set </summary>
        [SerializeField] GameObject losePanel;
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
        float attackStateTime = 10;
        ///<summary> dodge State 지속 시간, 만료 시 피해 입음 </summary>
        float dodgeStateTime = 10;
        ///<summary> dodge State에서 제시되는 커맨드 수 </summary>
        int dodgeCommandCount = 3;

        ///<summary> 남은 스테미나, Attack State 시작 시 초기화 </summary>
        int currStamina = 10;
        #endregion CharacterStatus

        #region Animation
        [Header("Animation")]
        [SerializeField] Animator playerAnimator;
        [SerializeField] Animator[] enemyAnimators;
        [SerializeField] Animator bgAnimator;
        [SerializeField] GameObject winImage;
        [SerializeField] Animator endAnimator;
        #endregion

        private void Start() {
            for(int i =0;i<3;i++)
                enemyAnimators[i].gameObject.SetActive(i == GameManager.instance.gameData.enemy);

            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(false);
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);

            LoadData();

            for(int i = 0;i < 3; i++)
                dmgs[i] = dmgs[i + 4] = GameManager.instance.GetStat(i);
            stamina = GameManager.instance.GetStat(3);

            enemyhpSlider.SetMax(enemyHp);
            staminaSlider.SetMax(stamina);

            bgmSlider.value = PlayerPrefs.GetFloat("BGM", 1);
            SoundMgr.instance.PlayBGM(BGMList.Battle);
            SoundMgr.instance.PlaySFX(SFXList.Bell);
            StartCoroutine(WaitBeforeStart());
        }

        //<summary> 현재 스테이지 정보 불러오기 </summary>
        // https://litjson.net/
        void LoadData()
        {
            JsonData json = JsonMapper.ToObject(Resources.Load<TextAsset>("Stage").text);

            enemyHp = (int)json[GameManager.instance.gameData.stage - 1]["hp"];
            attackStateTime = float.Parse(json[GameManager.instance.gameData.stage -1]["attackTime"].ToString());
            dodgeStateTime = float.Parse(json[GameManager.instance.gameData.stage -1]["dodgeTime"].ToString());
            dodgeCommandCount = (int)json[GameManager.instance.gameData.stage - 1]["dodgeCommand"];
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
            SoundMgr.instance.PlaySFX(SFXList.Announce_Play);

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
        IEnumerator AttackTimer(float timer)
        {
            time = timer;
            while (time > 0)
            {
                timerTxt.text = string.Format("{0:N1}", time);
                yield return new WaitForSeconds(0.1f);
                time -= 0.1f;
            }

            userState = UserState.AttackEnd;
            StartCoroutine(AttackToDodgeDelay());

            yield return null;
        }
        
        ///<summary> 공격 애니메이션 재생 코루틴, 성공 여부는 매개변수로 받음 </summary>
        IEnumerator AttackToDodgeDelay()
        {
            foreach(Image image in attackTokenImages)
                image.gameObject.SetActive(false);
            timerTxt.text = string.Empty;

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
        IEnumerator DodgeTimer(float timer)
        {
            time = timer;
            while (time > 0)
            {
                timerTxt.text = string.Format("{0:N1}", time);
                yield return new WaitForSeconds(0.1f);
                time -= 0.1f;
            }

            SoundMgr.instance.PlaySFX(SFXList.Punch1);
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
            timerTxt.text = string.Empty;

            if (!isSuccess)
            {
                SoundMgr.instance.PlaySFX(SFXList.Hit);
                playerhpImages[--hp].SetActive(false);
                lose = hp <= 0;

                if(lose)
                    playerAnimator.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.5f);

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
                    PlayAnimation();
                    enemyAnimators[GameManager.instance.gameData.enemy].Play("Enemy_Hit");

                    //피해 누적 및 제일 앞 Token 제거
                    enemyHp -= dmgs[(int)inputToken];
                    if(enemyHp < 0)
                        enemyHp = 0;
                    enemyhpSlider.SetValue(enemyHp);

                    SoundMgr.instance.PlaySFX(SFXList.Punch);
                    
                    tokensQueue.RemoveAt(0);
                    tokensQueue.Add(GetAttackToken());
                    ImageUpdate();
                    time = attackStateTime;

                    staminaSlider.SetValue(--currStamina);

                    if(enemyHp <= 0)
                    {
                        userState = UserState.Win;
                        StopCoroutine(timer);
                        StartCoroutine(AttackToWinDelay());
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

                void PlayAnimation()
                {
                    switch(inputToken)
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
            ///<summary> Dodge State에서 버튼 입력 처리 </summary>
            void OnBtnDodgeState(CommandToken inputToken)
            {
                if(tokensQueue[0] == CommandToken.LDucking)
                    enemyAnimators[GameManager.instance.gameData.enemy].Play("Enemy_RA");
                else
                    enemyAnimators[GameManager.instance.gameData.enemy].Play("Enemy_LA");
                SoundMgr.instance.PlaySFX(SFXList.Punch1);

                //입력 성공
                if (inputToken == tokensQueue[0])
                {
                    //제일 앞 토큰 제거
                    tokensQueue.RemoveAt(0);
                    ImageUpdate();
                    time = dodgeStateTime;

                    playerAnimator.Play(inputToken == CommandToken.LDucking ? "Player_LD" : "Player_RD");

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

        IEnumerator AttackToWinDelay()
        {
            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(false);
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);
            enemyAnimators[GameManager.instance.gameData.enemy].gameObject.SetActive(false);

            yield return new WaitForSeconds(0.5f);

            Win();
        }
        void Win()
        {
            playerAnimator.gameObject.SetActive(false);
            endAnimator.gameObject.SetActive(true);
            endAnimator.Play("Player_Win");
            winImage.SetActive(true);
            
            SoundMgr.instance.PlaySFX(SFXList.Announce_Win);

            StartCoroutine(WinDelay());
        }
        IEnumerator WinDelay()
        {
            yield return new WaitForSeconds(2f);

            SoundMgr.instance.PlayBGM(BGMList.Win);
            winPanel.SetActive(true);
            earnMoneyTxt.text = $"{GameManager.instance.gameData.stage * 100} 골드 획득";
            GameManager.instance.EarnMoney(GameManager.instance.gameData.stage * 100);
            GameManager.instance.IncreaseStage();
        }
        
        void Lose()
        {
            foreach(Image i in attackTokenImages)
                i.gameObject.SetActive(false);
            foreach (Image i in dodgeTokenImages)
                i.gameObject.SetActive(false);
 
            playerAnimator.gameObject.SetActive(false);
            bgAnimator.Play("BG_Lose");
            enemyAnimators[GameManager.instance.gameData.enemy].gameObject.SetActive(false);

            endAnimator.gameObject.SetActive(true);
            endAnimator.Play("Player_Lose");

            SoundMgr.instance.PlaySFX(SFXList.Announce_Lose);

            StartCoroutine(LoseDelay());
        }
        IEnumerator LoseDelay()
        {
            yield return new WaitForSeconds(2f);

            losePanel.SetActive(true);
            SoundMgr.instance.PlayBGM(BGMList.Lose);

        }

        ///<summary> 일시 정지, 재시작 버튼 </summary>
        public void OnBtnPause()
        {
            if (userState == UserState.Load || userState == UserState.Win || userState == UserState.Lose) return;

            isPause = !isPause;
            pausePanel.SetActive(isPause);
            Time.timeScale = isPause ? 0 : 1;
        }

        //bgm 슬라이더에 할당, 슬라이더의 값은 0.0001 ~ 1로 범위 제한
        public void SetBGM() { SoundMgr.instance.SetBGM(bgmSlider.value); }
        public void Btn_Retry() {Time.timeScale = 1f; SceneManager.LoadScene(1);}
        public void Btn_GoToTitle() {Time.timeScale = 1f; SceneManager.LoadScene(0);}

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