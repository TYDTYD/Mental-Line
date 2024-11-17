## Mental Line
![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/14203676-ccde-4794-8491-cc9a0d11f12c)
![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/1124ace9-c2ca-4a05-afb9-4637537f931e)

[플레이 영상](https://youtu.be/JhcfNx301ok)
### 프로젝트 소개
- 게임 장르 : 하이퍼 캐쥬얼
- 제작 기간 : 2022.07 ~ 2022.09
- 프로젝트 목표 : 협업을 통한 경험 및 플레이스토어 출시
- 게임 소개 : 멘탈 라인과 함께 그래플링을 하며 나아가는 병아리의 좌충우돌 탐험 이야기

### 개발 규모
- 팀 인원 : 8명 (개발 PM 2, 개발 2, 사업 PM 1, 마케팅 1, 기획 2)
- 나의 역할 : 개발자 (게임 내 모든 로직 개발)

### 겪었던 힘들었던 점들
- 개발 협업이 처음이라 Github를 통해 여러번 코드 충돌을 겪음
- 플레이어 움직임 로직 설계 시 여러 사용자마다 다른 결과가 나와서 당황스러웠음
- 알고보니 고정 프레임으로 설계를 하지 않음으로 인함 문제
- 교훈 : 내 환경에서 결과가 잘 나온다고 만족하면 안됨, 다른 환경에서의 테스트도 중요하고 필요하다.
- 개발 막바지에 겪은 모바일 환경에서의 문제점
- 최적화를 고려하지 않음으로써 생긴 프레임 드랍 및 심각한 버그 초래 (GC의 역할을 깨달음, GC의 부담을 덜어줘야 한다는 점 깨달음)
- 최적화의 중요성을 깨달음. 뒤늦게 최적화를 시도했지만 성능 개선이 더디게 이루어짐 완전한 최적화 실패 => 설계가 매우 중요하다는 것을 깨달음
### 기술 설명서
- GitHub 및 Github desktop을 통한 협업 개발

<details>
  <summary>
    그래플링 로직 개발
  </summary>
<pre>
  <code>
    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        hook.Grapple += Grapple;
        dir = transform.right + 2f * transform.up;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = player.transform.position;
        if (Input.GetMouseButtonDown(0))
        {
            if(Time.timeScale == 1)
            {
                gr.Play();

            }
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        lr.enabled = true;
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, dir, out hit, maxDistance, WhatIsGrappleable))
        {
            hook.gameObject.SetActive(true);
            grapplePoint = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y - (hit.collider.transform.lossyScale.y / 2f));
            lr.positionCount = 2;
            currentGrapplePoint = transform.position;
        }
        else
        {
            hook.gameObject.SetActive(false);
        }
    }

    void DrawRope()
    {
        if (lr.positionCount == 0) return;

        currentGrapplePoint = Vector3.MoveTowards(currentGrapplePoint, grapplePoint, Time.deltaTime * speed);

        lr.SetPosition(0, transform.position+new Vector3(0,0.3f));
        lr.SetPosition(1, currentGrapplePoint);
    }
    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }
    public bool IsGrappling()
    {
        return joint != null;
    }
    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
    public Vector3 GetHookPoint()
    {
        return currentGrapplePoint;
    }
    void Grapple()
    {
        joint = player.gameObject.AddComponent<SpringJoint>();
        
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.transform.position, grapplePoint);
        
        joint.maxDistance = distanceFromPoint;
        joint.minDistance = 0f;
        joint.spring = 5f;
        joint.damper = 10f;
        joint.massScale = 100f;
    }
  </code>
</pre>  
</details>
</pre>  
</details>

<details>
  <summary>
    Google Play 로그인 구현
  </summary>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi.Events;


public class GPGSBinder
{

    static GPGSBinder inst = new GPGSBinder();
    public static GPGSBinder Inst => inst;

    ISavedGameClient SavedGame =>
        PlayGamesPlatform.Instance.SavedGame;

    IEventsClient Events =>
        PlayGamesPlatform.Instance.Events;

    void Init()
    {
        var config = new PlayGamesClientConfiguration.Builder().EnableSavedGames().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }


    public void Login(Action<bool, UnityEngine.SocialPlatforms.ILocalUser> onLoginSuccess = null)
    {
        Init();
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
        {
            onLoginSuccess?.Invoke(success == SignInStatus.Success, Social.localUser);
        });
    }

    public void Logout()
    {
        PlayGamesPlatform.Instance.SignOut();
    }


    public void SaveCloud(string fileName, string saveData, Action<bool> onCloudSaved = null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    var update = new SavedGameMetadataUpdate.Builder().Build();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(saveData);
                    SavedGame.CommitUpdate(game, update, bytes, (status2, game2) =>
                    {
                        onCloudSaved?.Invoke(status2 == SavedGameRequestStatus.Success);
                    });
                }
            });
    }

    public void LoadCloud(string fileName, Action<bool, string> onCloudLoaded = null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(fileName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGame.ReadBinaryData(game, (status2, loadedData) =>
                    {
                        if (status2 == SavedGameRequestStatus.Success)
                        {
                            string data = System.Text.Encoding.UTF8.GetString(loadedData);
                            onCloudLoaded?.Invoke(true, data);
                        }
                        else
                            onCloudLoaded?.Invoke(false, null);
                    });
                }
            });
    }

    public void DeleteCloud(string fileName, Action<bool> onCloudDeleted = null)
    {
        SavedGame.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGame.Delete(game);
                    onCloudDeleted?.Invoke(true);
                }
                else
                    onCloudDeleted?.Invoke(false);
            });
    }


    public void ShowAchievementUI() =>
        Social.ShowAchievementsUI();

    public void UnlockAchievement(string gpgsId, Action<bool> onUnlocked = null) =>
        Social.ReportProgress(gpgsId, 100, success => onUnlocked?.Invoke(success));

    public void IncrementAchievement(string gpgsId, int steps, Action<bool> onUnlocked = null) =>
        PlayGamesPlatform.Instance.IncrementAchievement(gpgsId, steps, success => onUnlocked?.Invoke(success));


    public void ShowAllLeaderboardUI() =>
        Social.ShowLeaderboardUI();

    public void ShowTargetLeaderboardUI(string gpgsId) =>
        ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(gpgsId);

    public void ReportLeaderboard(string gpgsId, long score, Action<bool> onReported = null) =>
        Social.ReportScore(score, gpgsId, success => onReported?.Invoke(success));

    public void LoadAllLeaderboardArray(string gpgsId, Action<UnityEngine.SocialPlatforms.IScore[]> onloaded = null) =>
        Social.LoadScores(gpgsId, onloaded);

    public void LoadCustomLeaderboardArray(string gpgsId, int rowCount, LeaderboardStart leaderboardStart,
        LeaderboardTimeSpan leaderboardTimeSpan, Action<bool, LeaderboardScoreData> onloaded = null)
    {
        PlayGamesPlatform.Instance.LoadScores(gpgsId, leaderboardStart, rowCount, LeaderboardCollection.Public, leaderboardTimeSpan, data =>
        {
            onloaded?.Invoke(data.Status == ResponseStatus.Success, data);
        });
    }


    public void IncrementEvent(string gpgsId, uint steps)
    {
        Events.IncrementEvent(gpgsId, steps);
    }

    public void LoadEvent(string gpgsId, Action<bool, IEvent> onEventLoaded = null)
    {
        Events.FetchEvent(DataSource.ReadCacheOrNetwork, gpgsId, (status, iEvent) =>
        {
            onEventLoaded?.Invoke(status == ResponseStatus.Success, iEvent);
        });
    }

    public void LoadAllEvent(Action<bool, List<IEvent>> onEventsLoaded = null)
    {
        Events.FetchAllEvents(DataSource.ReadCacheOrNetwork, (status, events) =>
        {
            onEventsLoaded?.Invoke(status == ResponseStatus.Success, events);
        });
    }

}
  </code>
</pre>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi.Events;

public class GooglePlayLogin : MonoBehaviour
{

    string log;

    private void Start()
    {
        Login();
    }

    public void Login()
    {
        GPGSBinder.Inst.Login((success, localUser) =>
        log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}");
    }
}
  </code>
</pre>
</details>

![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/fa056cc6-f348-4ef0-94f9-16f91e99f5d1)

<details>
  <summary>
    GPGS를 통한 랭킹 시스템 도입
  </summary>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi.Events;

public class GooglePlayAPI : MonoBehaviour
{

    public void RankingE1()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_easy_stage_1);
    }

    public void RankingE2()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_easy_stage_2);
    }

    public void RankingE3()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_easy_stage_3);
    }
    public void RankingE4()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_easy_stage_4);
    }
    public void RankingE5()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_easy_stage_5);
    }

    public void RankingH1()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_hard_stage_1);
    }

    public void RankingH2()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_hard_stage_2);
    }

    public void RankingH3()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_hard_stage_3);
    }

    public void RankingH4()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_hard_stage_4);
    }

    public void RankingH5()
    {
        GPGSBinder.Inst.ShowTargetLeaderboardUI(GPGSIds.leaderboard_hard_stage_5);
    }
}
  </code>
</pre>  
</details>

![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/3b8c07c3-1696-4580-a20c-d26c5f3b3baf)

<details>
  <summary>
    UI/UX 코드
  </summary>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    string log;
    public Button[] lvlbuttons;
    public Text[] textBestScores;
    public GameObject View;
    public GameObject[] LockImages;
    public Sprite Lock;
    public Text StarCheck;
    public GameObject TreasureView;
    public GameObject TreasureButton;
    public Button TreasureGetButton;
    public Sprite OpenTreasure;
    public Sprite EmptyTreasure;
    int Check;
    int S;

    // 버튼 뷰
    [HideInInspector]
    public int levelAt;
    [HideInInspector]
    public int count;
    [HideInInspector]
    public int start;

    // 2차원 배열
    [System.Serializable]
    public class Array2D
    {
        public GameObject[] arr = new GameObject[3];
    }
    public Array2D[] Stars = new Array2D[5];

    
    public AudioSource click;

    // Start is called before the first frame update
    void Start()
    {
        count = PlayerPrefs.GetInt("StageClear1");
        start= PlayerPrefs.GetInt("Play");
        levelAt = PlayerPrefs.GetInt("levelAt", 2);
        S = PlayerPrefs.GetInt("Serotonin", 0);
        for (int i = 0; i<lvlbuttons.Length; i++)
        {
            if (i + 2 > levelAt)
            {
                textBestScores[i].text = "";
                LockImages[i].SetActive(true);
            }
            else
            {
                textBestScores[i].text = $"{i+1}스테이지 / 최고 점수 " + PlayerPrefs.GetInt("BestScore"+(i+1).ToString()).ToString();
                if (PlayerPrefs.GetInt("BestScore" + (i + 1).ToString()) >= 3000)
                {
                    Stars[i].arr[2].SetActive(true);
                    Check += 3;

                }
                else if (PlayerPrefs.GetInt("BestScore" + (i + 1).ToString()) >= 2500)
                {
                    Stars[i].arr[1].SetActive(true);
                    Check += 2;
                }
                else if (PlayerPrefs.GetInt("BestScore" + (i + 1).ToString()) >= 2000)
                {
                    Stars[i].arr[0].SetActive(true);
                    Check += 1;
                }
            }
        }
        StarCheck.text = Check.ToString();

        if (Check != 15)
        {
            TreasureGetButton.interactable = false;
        }
        else
        {
            if (PlayerPrefs.GetInt("TreasureHard", 0) == 0)
            {
                TreasureButton.GetComponent<Image>().sprite = OpenTreasure;
                GPGSBinder.Inst.UnlockAchievement(GPGSIds.achievement_master_of_master, success => log = $"{success}");
            }
            else if (PlayerPrefs.GetInt("TreasureHard", 0) == 1)
            {
                TreasureButton.GetComponent<Image>().sprite = EmptyTreasure;
            }
        }
        
    }


    public void Btn1()
    {
        click.Play();
        SceneManager.LoadScene(2);
        Time.timeScale = 1;
    }

    public void Btn2()
    {
        click.Play();
        if (PlayerPrefs.GetInt("StageClear1") == 0)
        {
            View.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(3);
            Time.timeScale = 1;
        }
    }

    public void Btn3()
    {
        click.Play();
        if (PlayerPrefs.GetInt("StageClear2") == 0)
        {
            View.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(4);
            Time.timeScale = 1;
        }   
    }

    public void Btn4()
    {
        click.Play();
        if (PlayerPrefs.GetInt("StageClear3") == 0)
        {
            View.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(5);
            Time.timeScale = 1;
        }
    }

    public void Btn5()
    {
        click.Play();
        if (PlayerPrefs.GetInt("StageClear4") == 0)
        {
            View.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(6);
            Time.timeScale = 1;
        }
    }

    public void BackBtn()
    {
        click.Play();
        SceneManager.LoadScene(0);
    }

    public void CloseBtn()
    {
        click.Play();
        View.SetActive(false);
    }

    // 홈버튼
    public void HomeBtn()
    {
        click.Play();
        SceneManager.LoadScene(0);
    }
    // 이전 버튼
    public void BeforeBtn()
    {
        click.Play();
        SceneManager.LoadScene(11);
    }

    public void TreasureBtn()
    {
        if (PlayerPrefs.GetInt("TreasureHard", 0) == 0)
        {
            TreasureView.SetActive(true);
        }
    }

    public void TreasureCloseBtn()
    {
        TreasureView.SetActive(false);
    }

    public void TreasureGetBtn()
    {
        S += 500;
        PlayerPrefs.SetInt("Serotonin", S);
        PlayerPrefs.SetInt("TreasureHard", 1);
        TreasureButton.GetComponent<Image>().sprite = EmptyTreasure;
        TreasureView.SetActive(false);
    }
}
  </code>
</pre>  
</details>

![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/f629fdf6-2212-4962-bede-7bb05fcb190d)

<details>
  <summary>
    재화 및 사운드 관리 코드
  </summary>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public GameObject FirstDestinationView;
    public GameObject DestinaionView;
    public GameObject[] Player;
    public GameObject PauseView;
    public Text TimeCount;
    public AudioSource click;

    public GameObject StarView;
    void Awake()
    {
        if (Player != null)
        {
            // 스킨 장착 여부
            for (int i = 0; i < Player.Length; i++)
            {
                if (PlayerPrefs.GetInt($"Fit{i}") == 1)
                {
                    Player[i].SetActive(true);
                    //LoadAddressableAsset("Player" + i);
                    SoundManager.instance.BGM[i].Play();
                }
                else
                {
                    Player[i].SetActive(false);
                }
            }
        }
    }

    void Start()
    {
        
    }

    private void LoadAddressableAsset(string key)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(key);

        handle.Completed += (operation) =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                Instantiate(operation.Result);
                Debug.Log("Prefab loaded and instantiated successfully!");
            }
            else
            {
                Debug.LogError($"Failed to load Addressable asset with key: {key}");
            }
        };
    }





    public void ClickHome()
    {
        click.Play();
        // 홈씬 불러오기
        SceneManager.LoadScene(0);
    }

    public void ClickRestart()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(2);
        Time.timeScale = 1;
    }

    public void ClickRestart2()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(3);
        Time.timeScale = 1;
    }

    public void ClickRestart3()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(4);
        Time.timeScale = 1;
    }

    public void ClickRestart4()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(5);
        Time.timeScale = 1;
    }

    public void ClickRestart5()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(6);
        Time.timeScale = 1;
    }

    // 이지모드
    public void ClickRestartE()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(13);
        Time.timeScale = 1;
    }

    public void ClickRestartE2()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(14);
        Time.timeScale = 1;
    }

    public void ClickRestartE3()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(15);
        Time.timeScale = 1;
    }

    public void ClickRestartE4()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(16);
        Time.timeScale = 1;
    }

    public void ClickRestartE5()
    {
        click.Play();
        // 플레이씬 불러오기
        SceneManager.LoadScene(17);
        Time.timeScale = 1;
    }

    public void ClickStory()
    {
        click.Play();
        // 스토리 씬 불러오기
        SceneManager.LoadScene(7);
    }

    public void ClickNext()
    {
        click.Play();
        int nexSceneLoad = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nexSceneLoad);
        Time.timeScale = 1;
        //SceneManager.LoadScene(11);
    }

    public void FirstDestination()
    {
        click.Play();
        FirstDestinationView.SetActive(false);
        DestinaionView.SetActive(true);
    }

    // 5스테이지에서 스테이지 선택 버튼 - DestinationView
    public void Stage()
    {
        click.Play();
        SceneManager.LoadScene(11);
    }

    public void Stop()
    {
        click.Play();
        PauseView.SetActive(true);
        Time.timeScale = 0;
    }

    public void Restart()
    {
        click.Play();
        PauseView.SetActive(false);
        StartCoroutine("BeforeStart");
    }

    IEnumerator BeforeStart()
    {
        Time.timeScale = 0.01f;
        TimeCount.text = "3";
        TimeCount.gameObject.SetActive(true);
        
        yield return YieldInstructionCache.WaitForSeconds(0.01f);
        TimeCount.text = "2";
        yield return YieldInstructionCache.WaitForSeconds(0.01f);
        TimeCount.text = "1";
        yield return YieldInstructionCache.WaitForSeconds(0.01f);
        TimeCount.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void StarBtn()
    {
        click.Play();
        StarView.SetActive(true);
    }
    public void StarCloseBtn()
    {
        click.Play();
        StarView.SetActive(false);
    }
}
  </code>
</pre>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AssetReference[] assetReference;

    public List<AudioSource> BGM;
    public AssetReference[] assetReference_SFX;
    public List<AudioSource> SFX;

    GameObject BackgroundMusic;
    AudioSource backmusic;

    void LoadAndInstatiatePrefab()
    {

        for (int i = 0; i < assetReference.Length; i++)
        {
            Debug.Log("출력");
            AsyncOperationHandle<GameObject> handle = assetReference[i].LoadAssetAsync<GameObject>();
            handle.Completed += OnLoadCompleted;
        }

        for (int i = 0; i < assetReference_SFX.Length; i++)
        {
            AsyncOperationHandle<GameObject> handle = assetReference_SFX[i].LoadAssetAsync<GameObject>();
            handle.Completed += OnLoadSFXCompleted;
        }
    }

    void OnLoadCompleted(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            BGM.Add(handle.Result.GetComponent<AudioSource>());
        }
    }

    void OnLoadSFXCompleted(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            SFX.Add(handle.Result.GetComponent<AudioSource>());
        }
    }

    void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAndInstatiatePrefab();
        }
        else
        {
            Destroy(gameObject);
        }
        
        BackgroundMusic = GameObject.Find("HomeBGM");
        backmusic = BackgroundMusic.GetComponent<AudioSource>();
        if (backmusic.isPlaying) return; 
        else
        {
            backmusic.Play();
            DontDestroyOnLoad(BackgroundMusic); 
        }

        
    }

    private void Start()
    {

        for (int i = 0; i < BGM.Count; i++)
        {
            BGM[i].dopplerLevel = 0.0f;
        }
        for (int i = 0; i < SFX.Count; i++)
        {
            SFX[i].dopplerLevel = 0.0f;
        }
    }

    private void Update()
    {
        if(PlayerPrefs.GetInt("sound") == 1)
        {
            for (int i = 0; i < BGM.Count; i++)
            {
                BGM[i].volume = 0.5f;
                //backmusic.Play();
            }
        }
        else if (PlayerPrefs.GetInt("sound") == 0)
        {
            for (int i = 0; i < BGM.Count; i++)
            {
                BGM[i].volume = 0.0f;
            }
        }

        if (PlayerPrefs.GetInt("sfx") == 1)
        {
            for (int i = 0; i < SFX.Count; i++)
            {
                SFX[i].volume = 0.5f;
            }
        }
        else if (PlayerPrefs.GetInt("sfx") == 0)
        {
            for (int i = 0; i < SFX.Count; i++)
            {
                SFX[i].volume = 0.0f;
            }
        }


        // 홈 사운드 0 1 11 12
        for (int i = 2; i < 11; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                backmusic.mute = true;
                backmusic.time = 0f;
            }
        }
        for (int i = 13; i < 18; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                backmusic.mute = true;
                backmusic.time = 0f;
            }
        }
        for (int i = 0; i < 2; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                if(PlayerPrefs.GetInt("sound") == 1)
                {
                    backmusic.mute = false;
                    backmusic.volume = 0.5f;
                }
            }
        }
        for (int i = 11; i < 13; i++)
        {
            if (SceneManager.GetActiveScene().buildIndex == i)
            {
                if (PlayerPrefs.GetInt("sound") == 1)
                {
                    backmusic.mute = false;
                    backmusic.volume = 0.5f;
                }
            }
        }



    }
    public void HomeBGM()
    {
        PlayerPrefs.SetInt("sound",1);
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            backmusic.mute = false;
            backmusic.volume = 0.5f;
            backmusic.time = 0f;
        }
       
    }
    public void HomeBGMOff()
    {
        backmusic.mute = true;
    }

    private void OnDestroy()
    {
        if (assetReference != null)
        {
            for (int i = 0; i < assetReference.Length; i++)
            {
                assetReference[i].ReleaseAsset();
            }
        }

        if (assetReference_SFX != null)
        {
            for (int i = 0; i < assetReference_SFX.Length; i++)
            {
                assetReference_SFX[i].ReleaseAsset();
            }
        }
        

    }

}
  </code>
</pre>
</details>

<details>
  <summary>
    게임 데이터 관리 코드
  </summary>
<pre>
  <code>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using GoogleMobileAds.Api;

public class MainManager : MonoBehaviour
{
    // 설정
    public GameObject SettingView;
    public GameObject HelpView;

    // 종료
    public GameObject EscapeView;

    // 사운드
    [SerializeField] AudioSource music;
    public GameObject soundOn;
    public GameObject soundOff;
    public GameObject sfxOn;
    public GameObject sfxOff;
    public GameObject shakeOn;
    public GameObject shakeOff;

    // 도파민 세로토민 개수
    public Text Dopamine;
    public Text Serotonin;
    private int D;
    private int DD;
    private int S;
    private int SS;

    // 출석체크 뷰 / 버튼
    public Button[] DayBtns;
    public GameObject DayView;
    public GameObject View1;
    public GameObject View2;
    // 출석체크 완료 이미지
    public GameObject[] Btnimg;

    // *일차 뷰
    public GameObject Views1;
    public GameObject Views2;
    public GameObject Views3;
    public GameObject Views4;
    public GameObject Views5;
    public GameObject Views6;
    public GameObject Views7;

    // 사운드
    public AudioSource click;

    // day = 다음 00시
    DateTime day = DateTime.Today.AddDays(1);
    TimeSpan ts = new TimeSpan(10, 0, 0);
    DateTime Now = DateTime.Now;

    // 리워드 광고
    private RewardedAd rewardedAd;
    private RewardedAd rewardedAd2;
    private RewardedAd rewardedAd3;
    private RewardedAd rewardedAd4;
    private RewardedAd rewardedAd5;
    private RewardedAd rewardedAd6;
    private RewardedAd rewardedAd7;
    public Canvas myCan;

    // 실제 광고 ID
    private string rewardID; // = "ca-app-pub-4101740431730513/5910670487";

    private bool rewarded = false;
    private bool rewarded2 = false;
    private bool rewarded3 = false;
    private bool rewarded4 = false;
    private bool rewarded5 = false;
    private bool rewarded6 = false;
    private bool rewarded7 = false;

    DateTime dtNowTime = DateTime.Now;

    // 출석체크 리워드 광고 
    // 리워드 광고
    public void UserChoseToWatchAd()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 100);
            PlayerPrefs.SetInt("SSerotonin", SS + 100);
            Views1.SetActive(false);
            PlayerPrefs.SetInt("Day1", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd() // 광고 다시 로드하는 함수
    {
        rewardedAd = new RewardedAd(rewardID);

        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(request);
    }

    public void HandleRewardedAdClosed(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 100);
        PlayerPrefs.SetInt("SSerotonin", SS + 100);
        Views1.SetActive(false);
        PlayerPrefs.SetInt("Day1", 1);
    }

    // 리워드 광고  2
    public void UserChoseToWatchAd2()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 100);
            PlayerPrefs.SetInt("SSerotonin", SS + 100);
            Views2.SetActive(false);
            PlayerPrefs.SetInt("Day2", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd2.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd2.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd2() // 광고 다시 로드하는 함수
    {
        rewardedAd2 = new RewardedAd(rewardID);

        rewardedAd2.OnUserEarnedReward += HandleUserEarnedReward2;
        rewardedAd2.OnAdClosed += HandleRewardedAdClosed2;

        AdRequest request2 = new AdRequest.Builder().Build();
        rewardedAd2.LoadAd(request2);
    }

    public void HandleRewardedAdClosed2(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd2();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward2(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded2 = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 100);
        PlayerPrefs.SetInt("SSerotonin", SS + 100);
        Views2.SetActive(false);
        PlayerPrefs.SetInt("Day2", 1);
    }

    // 리워드 광고  3
    public void UserChoseToWatchAd3()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 200);
            PlayerPrefs.SetInt("SSerotonin", SS + 200);
            Views3.SetActive(false);
            PlayerPrefs.SetInt("Day3", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd3.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd3.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd3() // 광고 다시 로드하는 함수
    {
        rewardedAd3 = new RewardedAd(rewardID);

        rewardedAd3.OnUserEarnedReward += HandleUserEarnedReward3;
        rewardedAd3.OnAdClosed += HandleRewardedAdClosed3;

        AdRequest request3 = new AdRequest.Builder().Build();
        rewardedAd3.LoadAd(request3);
    }

    public void HandleRewardedAdClosed3(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd3();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward3(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded3 = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 200);
        PlayerPrefs.SetInt("SSerotonin", SS + 200);
        Views3.SetActive(false);
        PlayerPrefs.SetInt("Day3", 1);
    }

    // 리워드 광고  4
    public void UserChoseToWatchAd4()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 100);
            PlayerPrefs.SetInt("SSerotonin", SS + 100);
            Views4.SetActive(false);
            PlayerPrefs.SetInt("Day4", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd4.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd4.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd4() // 광고 다시 로드하는 함수
    {
        rewardedAd4 = new RewardedAd(rewardID);

        rewardedAd4.OnUserEarnedReward += HandleUserEarnedReward4;
        rewardedAd4.OnAdClosed += HandleRewardedAdClosed4;

        AdRequest request4 = new AdRequest.Builder().Build();
        rewardedAd4.LoadAd(request4);
    }

    public void HandleRewardedAdClosed4(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd4();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward4(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded4 = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 100);
        PlayerPrefs.SetInt("SSerotonin", SS + 100);
        Views4.SetActive(false);
        PlayerPrefs.SetInt("Day4", 1);
    }

    // 리워드 광고  5
    public void UserChoseToWatchAd5()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 100);
            PlayerPrefs.SetInt("SSerotonin", SS + 100);
            Views5.SetActive(false);
            PlayerPrefs.SetInt("Day5", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd5.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd5.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd5() // 광고 다시 로드하는 함수
    {
        rewardedAd5 = new RewardedAd(rewardID);

        rewardedAd5.OnUserEarnedReward += HandleUserEarnedReward5;
        rewardedAd5.OnAdClosed += HandleRewardedAdClosed5;

        AdRequest request5 = new AdRequest.Builder().Build();
        rewardedAd5.LoadAd(request5);
    }

    public void HandleRewardedAdClosed5(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd5();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward5(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded5 = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 100);
        PlayerPrefs.SetInt("SSerotonin", SS + 100);
        Views5.SetActive(false);
        PlayerPrefs.SetInt("Day5", 1);
    }

    // 리워드 광고  6
    public void UserChoseToWatchAd6()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 300);
            PlayerPrefs.SetInt("SSerotonin", SS + 300);
            Views6.SetActive(false);
            PlayerPrefs.SetInt("Day6", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd6.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd6.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd6() // 광고 다시 로드하는 함수
    {
        rewardedAd6 = new RewardedAd(rewardID);

        rewardedAd6.OnUserEarnedReward += HandleUserEarnedReward6;
        rewardedAd6.OnAdClosed += HandleRewardedAdClosed6;

        AdRequest request6 = new AdRequest.Builder().Build();
        rewardedAd6.LoadAd(request6);
    }

    public void HandleRewardedAdClosed6(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd6();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward6(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded6 = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 300);
        PlayerPrefs.SetInt("SSerotonin", SS + 300);
        Views6.SetActive(false);
        PlayerPrefs.SetInt("Day6", 1);
    }

    // 리워드 광고  7
    public void UserChoseToWatchAd7()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Noads") == 1)
        {
            PlayerPrefs.SetInt("Serotonin", S + 1000);
            PlayerPrefs.SetInt("SSerotonin", SS + 1000);
            Views7.SetActive(false);
            PlayerPrefs.SetInt("Day7", 1);
        }
        else if (PlayerPrefs.GetInt("Noads") == 0)
        {
            if (rewardedAd7.IsLoaded()) // 광고가 로드 되었을 때
            {
                rewardedAd7.Show(); // 광고 보여주기
                myCan.sortingOrder = -1;
            }
        }
    }

    public void CreateAndLoadRewardedAd7() // 광고 다시 로드하는 함수
    {
        rewardedAd7 = new RewardedAd(rewardID);

        rewardedAd7.OnUserEarnedReward += HandleUserEarnedReward7;
        rewardedAd7.OnAdClosed += HandleRewardedAdClosed7;

        AdRequest request7 = new AdRequest.Builder().Build();
        rewardedAd7.LoadAd(request7);
    }

    public void HandleRewardedAdClosed7(object sender, System.EventArgs args)
    {  // 사용자가 광고를 닫았을 때
        CreateAndLoadRewardedAd7();  // 광고 다시 로드
    }

    private void HandleUserEarnedReward7(object sender, Reward e)
    {  // 광고를 다 봤을 때
        rewarded7 = true; // 변수 true
        PlayerPrefs.SetInt("Serotonin", S + 1000);
        PlayerPrefs.SetInt("SSerotonin", SS + 1000);
        Views7.SetActive(false);
        PlayerPrefs.SetInt("Day7", 1);
    }



    // Start is called before the first frame update
    void Start()
    {
        // 출석체크 리워드 광고 
        rewardedAd = new RewardedAd(rewardID);
        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(request); // 광고 로드

        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;
        // 사용자가 광고를 중간에 닫았을 때

        rewardedAd2 = new RewardedAd(rewardID);
        AdRequest request2 = new AdRequest.Builder().Build();
        rewardedAd2.LoadAd(request); // 광고 로드

        rewardedAd2.OnUserEarnedReward += HandleUserEarnedReward2;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd2.OnAdClosed += HandleRewardedAdClosed2;
        // 사용자가 광고를 중간에 닫았을 때

        rewardedAd3 = new RewardedAd(rewardID);
        AdRequest request3 = new AdRequest.Builder().Build();
        rewardedAd3.LoadAd(request); // 광고 로드

        rewardedAd3.OnUserEarnedReward += HandleUserEarnedReward3;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd3.OnAdClosed += HandleRewardedAdClosed3;
        // 사용자가 광고를 중간에 닫았을 때

        rewardedAd4 = new RewardedAd(rewardID);
        AdRequest request4 = new AdRequest.Builder().Build();
        rewardedAd4.LoadAd(request); // 광고 로드

        rewardedAd4.OnUserEarnedReward += HandleUserEarnedReward4;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd4.OnAdClosed += HandleRewardedAdClosed4;
        // 사용자가 광고를 중간에 닫았을 때

        rewardedAd5 = new RewardedAd(rewardID);
        AdRequest request5 = new AdRequest.Builder().Build();
        rewardedAd5.LoadAd(request); // 광고 로드

        rewardedAd5.OnUserEarnedReward += HandleUserEarnedReward5;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd5.OnAdClosed += HandleRewardedAdClosed5;
        // 사용자가 광고를 중간에 닫았을 때

        rewardedAd6 = new RewardedAd(rewardID);
        AdRequest request6 = new AdRequest.Builder().Build();
        rewardedAd6.LoadAd(request); // 광고 로드

        rewardedAd6.OnUserEarnedReward += HandleUserEarnedReward6;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd6.OnAdClosed += HandleRewardedAdClosed6;
        // 사용자가 광고를 중간에 닫았을 때

        rewardedAd7 = new RewardedAd(rewardID);
        AdRequest request7 = new AdRequest.Builder().Build();
        rewardedAd7.LoadAd(request); // 광고 로드

        rewardedAd7.OnUserEarnedReward += HandleUserEarnedReward7;
        // 사용자가 광고를 끝까지 시청했을 때
        rewardedAd7.OnAdClosed += HandleRewardedAdClosed7;
        // 사용자가 광고를 중간에 닫았을 때

        // 출석체크 리워드 광고 끝 ---------



        // 사운드
        PlayerPrefs.GetInt("sound", 1);
        PlayerPrefs.GetInt("sfx", 1);

        // 처음 플레이 여부
        PlayerPrefs.GetInt("Play", 0);
        // 각 스테이지 클리어 여부 - 하드모드
        PlayerPrefs.GetInt("StageClear1", 0);
        PlayerPrefs.GetInt("StageClear2", 0);
        PlayerPrefs.GetInt("StageClear3", 0);
        PlayerPrefs.GetInt("StageClear4", 0);
        PlayerPrefs.GetInt("StageClear5", 0);
        // 각 스테이지 클리어 여부 - 이지모드
        PlayerPrefs.GetInt("StageClearE1", 0);
        PlayerPrefs.GetInt("StageClearE2", 0);
        PlayerPrefs.GetInt("StageClearE3", 0);
        PlayerPrefs.GetInt("StageClearE4", 0);
        PlayerPrefs.GetInt("StageClearE5", 0);
        // 도파민, 세로토닌 저장
        D=PlayerPrefs.GetInt("Dopamine", 0);
        // 누적 도파민
        DD = PlayerPrefs.GetInt("DDopamine", 0);
        S =PlayerPrefs.GetInt("Serotonin", 0);
        // 누적 세로토닌
        SS =PlayerPrefs.GetInt("SSerotonin", 0);
        // 각 스테이지 최고 점수 - 하드모드
        PlayerPrefs.GetInt("BestScore1", 0);
        PlayerPrefs.GetInt("BestScore2", 0);
        PlayerPrefs.GetInt("BestScore3", 0);
        PlayerPrefs.GetInt("BestScore4", 0);
        PlayerPrefs.GetInt("BestScore5", 0);
        // 각 스테이지 최고 점수 - 이지모드
        PlayerPrefs.GetInt("BestScoreE1", 0);
        PlayerPrefs.GetInt("BestScoreE2", 0);
        PlayerPrefs.GetInt("BestScoreE3", 0);
        PlayerPrefs.GetInt("BestScoreE4", 0);
        PlayerPrefs.GetInt("BestScoreE5", 0);
        // 아이템 스킨 저장
        PlayerPrefs.GetInt("Item1", 0);
        PlayerPrefs.GetInt("Item2", 0);
        PlayerPrefs.GetInt("Item3", 0);
        PlayerPrefs.GetInt("Item4", 0);
        PlayerPrefs.GetInt("Item5", 0);
        // 출석체크 저장
        PlayerPrefs.GetInt("Day1", 0);
        PlayerPrefs.GetInt("Day2", 0);
        PlayerPrefs.GetInt("Day3", 0);
        PlayerPrefs.GetInt("Day4", 0);
        PlayerPrefs.GetInt("Day5", 0);
        PlayerPrefs.GetInt("Day6", 0);
        PlayerPrefs.GetInt("Day7", 0);
        // 날짜 바뀌는 여부
        PlayerPrefs.GetInt("DayCheck", 0);
        PlayerPrefs.GetInt("DayCheck2", 0);
        PlayerPrefs.GetInt("MonthCheck", 0);
        PlayerPrefs.GetInt("YearCheck", 0);
        // 스킨 장착 여부
        PlayerPrefs.GetInt("Fit0", 0);
        PlayerPrefs.GetInt("Fit1", 0);
        PlayerPrefs.GetInt("Fit2", 0);
        PlayerPrefs.GetInt("Fit3", 0);
        PlayerPrefs.GetInt("Fit4", 0);
        PlayerPrefs.GetInt("Fit5", 0);
        PlayerPrefs.GetInt("Fit6", 0);
        PlayerPrefs.GetInt("Fit7", 0);
        PlayerPrefs.GetInt("Fit8", 0);
        PlayerPrefs.GetInt("Fit9", 0);
        PlayerPrefs.GetInt("Fit10", 0);
        PlayerPrefs.GetInt("Fit11", 0);
        PlayerPrefs.GetInt("Fit12", 0);
        PlayerPrefs.GetInt("Fit13", 0);
        PlayerPrefs.GetInt("Fit14", 0);
        PlayerPrefs.GetInt("Fit15", 0);
        PlayerPrefs.GetInt("Fit16", 0);
        PlayerPrefs.GetInt("Fit17", 0);
        PlayerPrefs.GetInt("Fit18", 0);
        // 보물상자
        PlayerPrefs.GetInt("TreasureEasy", 0);
        PlayerPrefs.GetInt("TreasureHard", 0);
        // 광고 제거
        PlayerPrefs.GetInt("Noads", 0);

        if (PlayerPrefs.GetInt("Play") == 0)
        {
            PlayerPrefs.SetInt("Fit0", 1);
        }
        

        Count();

        for (int i = 1; i < 7; i++)
        {
            DayBtns[i].interactable = false;
        }
    }

    // 초기화 버튼
    public void ResetBtn()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // 사운드
        PlayerPrefs.SetInt("sound", 1);
        PlayerPrefs.SetInt("sfx", 1);
    }

    
    // 시간 추가
    public void AddDay()
    {
        /* // 버튼 누르면 1시간 추가
         Now = Now + ts;
         Debug.Log(Now);*/

        PlayerPrefs.SetInt("DayCheck", 10);
        PlayerPrefs.SetInt("DayCheck2", 10);
    }
    // 현재 시간 출력 버튼
    public void TimeBtn()
    {
        Debug.Log(Now);
    }

    // Update is called once per frame
    void Update()
    {
        // 리워드 광고
        if (rewarded)
        {
            rewarded = false;
        }
        if (rewarded2)
        {
            rewarded2 = false;
        }
        if (rewarded3)
        {
            rewarded3 = false;
        }
        if (rewarded4)
        {
            rewarded4 = false;
        }
        if (rewarded5)
        {
            rewarded5 = false;
        }
        if (rewarded6)
        {
            rewarded6 = false;
        }
        if (rewarded7)
        {
            rewarded7 = false;
        }

        // 사운드
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            soundOff.SetActive(true);
            soundOn.SetActive(false);
        }
        else if (PlayerPrefs.GetInt("sound") == 0)
        {
            soundOff.SetActive(false);
            soundOn.SetActive(true);
        }
        if (PlayerPrefs.GetInt("sfx") == 1)
        {
            sfxOff.SetActive(true);
            sfxOn.SetActive(false);
        }
        else if (PlayerPrefs.GetInt("sfx") == 0)
        {
            sfxOff.SetActive(false);
            sfxOn.SetActive(true);
        }


            // 7일차 모두 받았을 때
        if ((PlayerPrefs.GetInt("Day1") == 1) && (PlayerPrefs.GetInt("Day2") == 1) && (PlayerPrefs.GetInt("Day3") == 1) && (PlayerPrefs.GetInt("Day4") == 1) && 
            (PlayerPrefs.GetInt("Day5") == 1) && (PlayerPrefs.GetInt("Day6") == 1) && (PlayerPrefs.GetInt("Day7") == 1))
        {
            if (DateTime.Now.Day != PlayerPrefs.GetInt("DayCheck"))
            {
                PlayerPrefs.SetInt("Day1", 0);
                PlayerPrefs.SetInt("Day2", 0);
                PlayerPrefs.SetInt("Day3", 0);
                PlayerPrefs.SetInt("Day4", 0);
                PlayerPrefs.SetInt("Day5", 0);
                PlayerPrefs.SetInt("Day6", 0);
                PlayerPrefs.SetInt("Day7", 0);
                PlayerPrefs.SetInt("DayCheck1", DateTime.Now.Day);
            }

            /*if (DateTime.Now.Hour == 24)
            {
                PlayerPrefs.SetInt("Day1", 0);
                PlayerPrefs.SetInt("Day2", 0);
                PlayerPrefs.SetInt("Day3", 0);
                PlayerPrefs.SetInt("Day4", 0);
                PlayerPrefs.SetInt("Day5", 0);
                PlayerPrefs.SetInt("Day6", 0);
                PlayerPrefs.SetInt("Day7", 0);
            }*/

        }


        for (int i = 1; i < 7; i++)
        {

            //if (DateTime.Now.Day != PlayerPrefs.GetInt("DayCheck") || DateTime.Now.Month != PlayerPrefs.GetInt("MonthCheck") && (DayBtns[i - 1].interactable == true))
            //if ((DateTime.Now.Day != PlayerPrefs.GetInt("DayCheck")) && (DayBtns[i - 1].interactable == false))
            if ((DateTime.Now.Day != PlayerPrefs.GetInt("DayCheck")) && (PlayerPrefs.GetInt($"Day{i}") == 1))
            {
                DayBtns[i].interactable = true;
                PlayerPrefs.SetInt("DayCheck", DateTime.Now.Day);
                PlayerPrefs.SetInt("MonthCheck", DateTime.Now.Month);
            }

            /*if (DateTime.Now.Hour == 24 && (DayBtns[i - 1].interactable == true))
            {
                DayBtns[i].interactable = true;
            }*/


        }

        // 와르르 보물상자 하루 지나면 초기화
        if (DateTime.Now.Day != PlayerPrefs.GetInt("DayCheck2"))
        {
            PlayerPrefs.SetInt("OPEN", 0);
            PlayerPrefs.SetInt("DayCheck2", DateTime.Now.Day);
        }




        // 저장된 날짜와 오늘 날짜가 다르다면
        /*
        if (DateTime.Now.Day != PlayerPrefs.GetInt("DayCheck") || DateTime.Now.Month!=PlayerPrefs.GetInt("MonthCheck"))
        {
            DayBtns[0].interactable = true;
            PlayerPrefs.SetInt("DayCheck", DateTime.Now.Day);
            PlayerPrefs.SetInt("MonthCheck", DateTime.Now.Month);

        }*/


        // 안드로이드 back버튼으로 게임 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscapeView.SetActive(true);
        }

        // 출석체크 완료 버튼 색상 변경
        for (int i = 0; i < 7; i++)
        {
            int j = i + 1;
            if (PlayerPrefs.GetInt($"Day{j}") == 1)
            {
                DayBtns[i].interactable = false;
                Btnimg[i].SetActive(true);
            }
        }
        for (int i = 0; i < 7; i++)
        {
            int j = i + 1;
            if (PlayerPrefs.GetInt($"Day{j}") == 0)
            {
                Btnimg[i].SetActive(false);
            }
        }

        if (PlayerPrefs.GetInt("Day1") == 0)
        {
            DayBtns[0].interactable = true;
        }

    }

    // 게임종료
    public void ClickEscapeYes()
    {
        click.Play();
        Application.Quit();
    }
    public void ClickEscapeNo()
    {
        click.Play();
        EscapeView.SetActive(false);
    }

    // 게임시작 버튼
    public void ClickStart()
    {
        click.Play();
        if (PlayerPrefs.GetInt("Play", 0) == 0)
        {
            PlayerPrefs.SetInt("Play", 1);
            SceneManager.LoadScene(8);
            Time.timeScale = 1;
        }
        else
        {
            SceneManager.LoadScene(11);
            Time.timeScale = 1;
        }
    }

    // 상점 버튼
    public void ClickStore()
    {
        click.Play();
        SceneManager.LoadScene(9);
    }

    // 스토리 버튼
    public void ClickStory()
    {
        click.Play();
        SceneManager.LoadScene(7);
    }

    // 설정 창 열기
    public void ClickSetting()
    {
        click.Play();
        SettingView.SetActive(true);
    }

    // 설정 창 닫기
    public void ClickSettingClose()
    {
        click.Play();
        SettingView.SetActive(false);
    }

    // 버그 신고 창 열기
    public void ClickHelp()
    {
        click.Play();
        HelpView.SetActive(true);
    }

    // 버그 신고 창 닫기
    public void ClickHelpClose()
    {
        click.Play();
        HelpView.SetActive(false);
    }

    public void Count()
    {
        Dopamine.text = D.ToString();
        Serotonin.text = S.ToString();
    }

    // 홈화면에서 출석체크 버튼
    public void DayBtn()
    {
        click.Play();
        DayView.SetActive(true);
    }

    // 출석체크 뷰 닫기 버튼
    public void DayCloseBtn()
    {
        click.Play();
        DayView.SetActive(false);
    }

    // day버튼
    public void DayBtn1()
    {
        click.Play();
        Views1.SetActive(true);
    }

    // Views 닫기 버튼
    public void ViewsCloseBtn1()
    {
        click.Play();
        Views1.SetActive(false);
    }

    public void DayBtn2()
    {
        click.Play();
        Views2.SetActive(true);
    }
    public void ViewsCloseBtn2()
    {
        click.Play();
        Views2.SetActive(false);
    }

    public void DayBtn3()
    {
        click.Play();
        Views3.SetActive(true);
    }
    public void ViewsCloseBtn3()
    {
        click.Play();
        Views3.SetActive(false);
    }

    public void DayBtn4()
    {
        click.Play();
        Views4.SetActive(true);
    }
    public void ViewsCloseBtn4()
    {
        click.Play();
        Views4.SetActive(false);
    }

    public void DayBtn5()
    {
        click.Play();
        Views5.SetActive(true);
    }
    public void ViewsCloseBtn5()
    {
        click.Play();
        Views5.SetActive(false);
    }

    public void DayBtn6()
    {
        click.Play();
        Views6.SetActive(true);
    }
    public void ViewsCloseBtn6()
    {
        Views6.SetActive(false);
        click.Play();
    }

    public void DayBtn7()
    {
        Views7.SetActive(true);
        click.Play();
    }
    public void ViewsCloseBtn7()
    {
        Views7.SetActive(false);
        click.Play();
    }


    // 출석체크 세로토닌 보상 받기
    public void GetBtn1()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 50);
        PlayerPrefs.SetInt("SSerotonin", SS + 50);
        Views1.SetActive(false);
        PlayerPrefs.SetInt("Day1", 1);
    }

    public void GetBtn2()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 50);
        PlayerPrefs.SetInt("SSerotonin", SS + 50);
        Views2.SetActive(false);
        PlayerPrefs.SetInt("Day2", 1);
    }

    public void GetBtn3()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 100);
        PlayerPrefs.SetInt("SSerotonin", SS + 100);
        Views3.SetActive(false);
        PlayerPrefs.SetInt("Day3", 1);
    }

    public void GetBtn4()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 50);
        PlayerPrefs.SetInt("SSerotonin", SS + 50);
        Views4.SetActive(false);
        PlayerPrefs.SetInt("Day4", 1);
    }

    public void GetBtn5()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 50);
        PlayerPrefs.SetInt("SSerotonin", SS + 50);
        Views5.SetActive(false);
        PlayerPrefs.SetInt("Day5", 1);
    }

    public void GetBtn6()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 150);
        PlayerPrefs.SetInt("SSerotonin", SS + 150);
        Views6.SetActive(false);
        PlayerPrefs.SetInt("Day6", 1);
    }

    public void GetBtn7()
    {
        click.Play();
        PlayerPrefs.SetInt("Serotonin", S + 500);
        PlayerPrefs.SetInt("SSerotonin", SS + 500);
        Views7.SetActive(false);
        PlayerPrefs.SetInt("Day7", 1);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //PlayerPrefs.SetInt("Set", 1);
    }

    // 사운드
    public void SoundOFF()
    {
        PlayerPrefs.SetInt("sound", 0);
        soundOff.SetActive(false);
        soundOn.SetActive(true);
        click.Play();
    }
    public void SoundON()
    {
        PlayerPrefs.SetInt("sound", 1);
        soundOff.SetActive(true);
        soundOn.SetActive(false);
        click.Play();
    }

    public void SfxOFF()
    {
        PlayerPrefs.SetInt("sfx", 0);
        sfxOff.SetActive(false);
        sfxOn.SetActive(true);
        click.Play();
    }
    public void SfxON()
    {
        PlayerPrefs.SetInt("sfx", 1);
        sfxOff.SetActive(true);
        sfxOn.SetActive(false);
        click.Play();
    }

    public void Ranking()
    {
        click.Play();
        SceneManager.LoadScene(18);
    }
    
}
  </code>
</pre>
</details>
