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


![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/fa056cc6-f348-4ef0-94f9-16f91e99f5d1)
![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/f629fdf6-2212-4962-bede-7bb05fcb190d)
![image](https://github.com/TYDTYD/Alone_Or_Together_ver2/assets/48386074/3b8c07c3-1696-4580-a20c-d26c5f3b3baf)
