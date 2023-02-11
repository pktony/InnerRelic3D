using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// 사운드 설정, 랭킹 관련 정보를 관리하는 매니저
/// </summary>
public class SettingManager : Singleton<SettingManager>
{
    // Save 관련 -------------------------------------------
    // Setting
    private readonly string settingSaveFileName = "SettingData.json";
    // Rank
    private readonly string rankSaveFileName = "RankData.json";
    private RankData rankData;
    private LeaderBoard_Home board_Home;
    private LeaderBoard_InGame board_InGame;
    private const int rankCount = 6;

    public float[] Scores { get; private set; }
    public SettingData SettingData { get; private set; }
    public PanelResizer Panel { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        rankData = new();
        Scores = new float[rankCount];
        SceneManager.sceneLoaded += Initialize;

        Application.targetFrameRate = 30;
    }

    private void Initialize(Scene arg0, LoadSceneMode arg1)
    {
        if(!LoadSettingValues())
        {// 최초 로드데이터가 없으면 새로 저장 후 불러오기
            SaveSettingValues();
            LoadSettingValues();
        }
        SettingData = new();
        SoundManager.Inst.MasterVol = SettingData.masterVolume;
        SoundManager.Inst.MusicVolume = SettingData.musicVolume;

        // 랭킹 데이터 불러와서 입력 
        rankData = LoadRankDatas();
        Scores = rankData.scores;

        if (arg0 == SceneManager.GetSceneByName("Home"))
        {
            Panel = FindObjectOfType<PanelResizer>();
            board_Home = FindObjectOfType<LeaderBoard_Home>();
            board_Home.RefreshLeaderBoard();
        }
        else if(arg0 == SceneManager.GetSceneByName("Stage"))
        {
            board_InGame = FindObjectOfType<LeaderBoard_InGame>();
            board_InGame.RefreshLeaderBoard();
        }
    }

    public void SaveSettingValues()
    {
        // 클래스를 Json 형식으로 전환
        string ToJsonData = JsonUtility.ToJson(SettingData);
        // https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
        string filePath = $"{Application.persistentDataPath}/{settingSaveFileName}";

        // 파일 생성 
        File.WriteAllText(filePath, ToJsonData);
        print("저장 완료");
    }

    private bool LoadSettingValues()
    {
        bool result = false;

        string filePath = $"{Application.persistentDataPath}/{settingSaveFileName}";
        Debug.Log(filePath);
        if (File.Exists(filePath))
        {// 이미 있으면
            // 저장된 파일 읽어오고 Json을 클래스 형식으로 전환
            string FromJsonData = File.ReadAllText(filePath);
            SettingData = JsonUtility.FromJson<SettingData>(FromJsonData);
            //print("불러오기 완료");
            result = true;
            return result;
        }
        else
        {
            print("불러오기 실패");
            return result;
        }
    }


    // 랭킹 관련 세이브 함수 -------------------------------------------------------
    /// <summary>
    /// 게임 처음 시작할 때 랭킹을 불러 올거고,
    /// 그 정보를 리더보드에 띄운다
    ///
    /// 새로운 랭킹이 갱신되면,
    /// 새로운 값을 추가 또는 수정한다
    /// Save 하기 전에 정렬한다 
    /// </summary>
    /// <param name="newRank"></param>
    public void SaveGameRank()
    {
        string ToJsonData = JsonUtility.ToJson(rankData);
        string filePath = $"{Application.persistentDataPath}/{rankSaveFileName}";

        // 파일 생성 
        File.WriteAllText(filePath, ToJsonData);
        //print("랭킹 저장 완료");
    }

    public RankData LoadRankDatas()
    {
        string filePath = $"{Application.persistentDataPath}/{rankSaveFileName}";
        if (File.Exists(filePath))
        {// 이미 있으면
            string FromJsonData = File.ReadAllText(filePath);
            rankData = JsonUtility.FromJson<RankData>(FromJsonData);
            //print("랭킹 불러오기 완료");
            return rankData;
        }
        else
        {
            rankData.scores = Scores;
            SaveGameRank();
            //print("랭킹 기록 없음 ");
            return rankData;
        }
    }

    // 10 5 3 0
    // 7
    public void CheckHighScores(float newScore)
    {
        for(int i = 0; i < rankCount; i++)
        {
            if (Scores[i] <= newScore)
            {// 새로운 점수가 더 낮거나 같으면 해당 index에 새로운 점수 저장 
                for(int j = rankCount - 1; j > i; j--)
                {// 한칸씩 뒤로 밀기 
                    Scores[j] = Scores[j - 1];
                }
                Scores[i] = newScore;
                SaveGameRank();
                UIManager.Inst.LeaderBoard_InGame.RefreshLeaderBoard();
                UIManager.Inst.InfoPanel.ShowPanel(
                    DataManager.Inst.textManager.GetStringData("new_high_score"));
                SoundManager.Inst.PlaySound_UI(UIClips.Fanfare);
                break;
            }
            else
            {// 갱신할 필요 없음 

            }    
        }
    }
}
