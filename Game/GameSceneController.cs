using System.Collections;
using System.Text;
using UnityEngine;
using unityroom.Api;

public class GameSceneController : SceneControllerBase
{
    public static bool youWin = false;

    public static GameSceneController instance;
    [SerializeField] GameBoard board;
    [SerializeField] GameSceneUI gameSceneUI;
    [SerializeField] CanvasGroup gameOverCanvas;
    [SerializeField] CanvasGroup gameClearCanvas;
    [SerializeField] int defaultNorma = 3;
    [SerializeField] float normaSlope = 2.2f;
    [SerializeField] int threeTurnAddNorma = 5;

    [SerializeField] int[] normaAry;

    [SerializeField] InputWaiter clearWindowInputWaiter;
    [SerializeField] InputWaiter gameOverWindowInputWaiter;

    [SerializeField] ClearAnim clearAnim;
    [SerializeField] GameOverAnim gameOverAnim;
    private int score = 0;
    public int norma = 4;
    public int currentTurnCount = 1;
    public int maxTurnCount = 10;
    

    public int Score { get => score; set => score = value; }

    private void Awake()
    {
        instance = this;
        gameOverCanvas.gameObject.SetActive(false);
        gameClearCanvas.gameObject.SetActive(false);
    }

    protected override IEnumerator Start()
    {
        gameSceneUI.UpdateUI();
        return base.Start();
    }

    protected override IEnumerator _Update(string input)
    {
        switch (input)
        {
            case "GameBord":
                yield return board.MainLoop();
                Debug.Log($"turn:{currentTurnCount}  score:{score} rensa:{board.currentTurnRensaCount}");

                if (isCleared)
                {
                    youWin = true;
                    UnityroomApiClient.Instance.SendScore(1, score, ScoreboardWriteMode.HighScoreDesc);
                    StartCoroutine(clearAnim.Play());
                    board.PlayClearDance();
                    while (true)    //クリア画面入力待ち処理(ゲームループに戻る事はないのでwhiletrueでOK)
                    {
                        yield return clearWindowInputWaiter.WaitInput();
                        switch (clearWindowInputWaiter.LastInput)
                        {
                            case "Title":
                                SoundManager.PlaySound(CommonData.SoundId.NormaDown);
                                SoundManager.PlayBgm(CommonData.BGM_ID.Main);
                                yield return MySceneManager.Instance.LoadSceneAsync(CommonData.SceneName.Title);
                                break;
                            case "Retry":
                                SoundManager.PlaySound(CommonData.SoundId.Start);
                                SoundManager.PlayBgm(CommonData.BGM_ID.Main);
                                yield return MySceneManager.Instance.LoadSceneAsync(CommonData.SceneName.Game);
                                break;
                        }
                    }
                }
                else if (isGameovered)
                {
                    UnityroomApiClient.Instance.SendScore(1, score, ScoreboardWriteMode.HighScoreDesc);
                    StartCoroutine(gameOverAnim.Play());
                    while (true)    //クリア画面入力待ち処理(ゲームループに戻る事はないのでwhiletrueでOK)
                    {
                        yield return gameOverWindowInputWaiter.WaitInput();
                        switch (gameOverWindowInputWaiter.LastInput)
                        {
                            case "Title":
                                SoundManager.PlaySound(CommonData.SoundId.NormaDown);
                                yield return MySceneManager.Instance.LoadSceneAsync(CommonData.SceneName.Title);
                                break;
                            case "Retry":
                                SoundManager.PlaySound(CommonData.SoundId.Start);
                                yield return MySceneManager.Instance.LoadSceneAsync(CommonData.SceneName.Game);
                                break;
                        }
                    }
                }
                else 
                { 
                    UpdateNorma();
                }
                break;
        }
    }

    void UpdateNorma()
    {
        norma = normaAry[currentTurnCount];
        currentTurnCount++;
        //norma = (int)(defaultNorma * normaSlope * currentTurnCount + ((int)(currentTurnCount / 3) * threeTurnAddNorma));
        gameSceneUI.UpdateUI();
        StartCoroutine(gameSceneUI.PlayNormaChangedAnim());
        CommonData.SoundId normaUpSoundId = CommonData.SoundId.NormaUp;
        if(currentTurnCount % 3 == 0) normaUpSoundId = CommonData.SoundId.NormaUp2;
        SoundManager.PlaySound(normaUpSoundId);
    }

    bool isCleared = false;
    public void GameClear()
    {
        isCleared = true;
        gameClearCanvas.gameObject.SetActive(true);
        SoundManager.PlayBgm(CommonData.BGM_ID.Clear);
    }

    bool isGameovered = false;
    public void GameOver()
    {
        isGameovered = true;
        gameOverCanvas.gameObject.SetActive(true);
        StartCoroutine(gameSceneUI.PlayGameOverAnim());
        SoundManager.PlaySound(CommonData.SoundId.Miss);
    }
}
