using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameBoard : MonoBehaviour
{
    const int BOARD_WIDTH = 8;

    [SerializeField] float gridX;
    [SerializeField] float gridY;
    [SerializeField] Coin[][] cards = new Coin[BOARD_WIDTH][];

    [SerializeField] Coin cardPrefab;
    [SerializeField] GameSceneUI gameSceneUI;

    Stack<Coin> standbyCardStack = new Stack<Coin>();

    [SerializeField] float test_chainOpenedWaitTime = 1;
    [SerializeField] float eraceAnimDilay = 1f;
    [SerializeField] CustomInput onClickCoinMyUI;   //コインを押下した時にIsInputed=trueにする IMyUI
    [SerializeField] float normaChackTime = 1f;
    [SerializeField] float eraceAfterDelayTime = 0.5f;

    [SerializeField] Avatar avatar;

    private void Awake()
    {
        for (int y = 0; y < cards.Length; y++)
        {
            cards[y] = new Coin[BOARD_WIDTH];
            for (int x = 0; x < cards[y].Length; x++)
            {
                InstantiateNewCard(x, y);
            }
        }
        CommonData.SoundId slideSoundId = CommonData.SoundId.CoinSlide + UnityEngine.Random.Range(0, 4);
        SoundManager.PlaySound(slideSoundId);
    }

    RectTransform boardRt;
    Vector2 boardOrigin;

    void Start()
    {
        boardRt = transform as RectTransform;
        boardOrigin = Vector2.zero;
    }

    Vector2 GetCellPos(int x, int y)
    {
        return boardOrigin + new Vector2(x * gridX, y * gridY);
    }

    int instantiateCount = 0;
    void InstantiateNewCard(int x, int y)
    {
        var newCard = Instantiate(cardPrefab, transform);
        newCard.Init(this);
        MoveTo(newCard, x, y);
        newCard.gameObject.name = "Card" + instantiateCount.ToString();
        instantiateCount++;
    }

    [SerializeField] float moveDuration = 1;
    [SerializeField] float moveAfterDelay = 0.5f;

    void MoveTo(Coin card, int x, int y)
    {
        RectTransform rt = card.transform as RectTransform;
        rt.DOAnchorPos(GetCellPos(x, y), moveDuration);

        cards[y][x] = card;
        card.SetIndex(XYtoIndex(x, y));
    }   

    bool processing = false;

    public void OnClickChild(int index)
    {
        if (processing) return;
        processing = true;
        //見た目初期化
        int x = IndexToX(index);
        int y = IndexToY(index);
        cards[y][x].EndOpenYokokuAnim();
        if(y > 0 && cards[y-1][x] != null) cards[y-1][x].EndOpenYokokuAnim();
        if(x > 0 && cards[y][x-1] != null) cards[y][x-1].EndOpenYokokuAnim();
        if(x < BOARD_WIDTH-1 && cards[y][x+1] != null) cards[y][x+1].EndOpenYokokuAnim();
        if(y < BOARD_WIDTH-1 && cards[y+1][x] != null) cards[y+1][x].EndOpenYokokuAnim();

        lastClickIndex = index;
        onClickCoinMyUI.SetInputed();
    }

    int lastClickIndex;

    IEnumerator Toss(int x, int y, int forceValue = -1)
    {
        if (x < 0 || x >= BOARD_WIDTH || y < 0 || y >= BOARD_WIDTH) yield break;
        cards[y][x].SetValue(forceValue != -1 ? forceValue : UnityEngine.Random.Range(0,2), true);
        SoundManager.PlaySound(CommonData.SoundId.CoinReverce);
    }

    [SerializeField] float rensaCoinSoundDilay = 0.3f;

    public IEnumerator MainLoop()
    {
        int onClickX = IndexToX(lastClickIndex);
        int onClockY = IndexToY(lastClickIndex);
        currentTurnRensaCount = 0;
        yield return UseSkill(onClickX, onClockY, 1);
        do
        {
            eracedOnThisLoop = false;
            yield return CheckErace();
            yield return EracePhase();
            yield return ChainOpenPhase();
            yield return DropPhase();
        } while (eracedOnThisLoop);

        //連鎖数に応じたコインを取得する処理
        if (currentTurnRensaCount >= 2)
        {
            yield return gameSceneUI.PlayRensaToScoreAnim();
            gameSceneUI.AddCoin(currentTurnRensaCount);

            int coinSoundCount = currentTurnRensaCount;
            while (coinSoundCount >= 1)
            {
                CommonData.SoundId pocketInCointSound = CommonData.SoundId.PoketInCoin + UnityEngine.Random.Range(0, 4);
                SoundManager.PlaySound(pocketInCointSound);
                coinSoundCount--;
                yield return new WaitForSeconds(rensaCoinSoundDilay);
            }
        }

        StartCoroutine(gameSceneUI.PlayRensaHideAnim());    //連鎖数の表示を消す

        //クリア判定
        yield return new WaitForSeconds(normaChackTime);
        
        if(GameSceneController.instance.Score >= GameSceneController.instance.norma)
        {
            //目標達成
            if(GameSceneController.instance.currentTurnCount >= GameSceneController.instance.maxTurnCount)
            {
                GameSceneController.instance.GameClear();
                yield break;
            }
        }
        else
        {
            //ゲームオーバー
            avatar.SetRensaReaction(-1);
            GameSceneController.instance.GameOver();
            yield break;
        }
        yield return ChargePhase();
        avatar.SetRensaReaction(0);
        processing = false;
    }

    IEnumerator UseSkill(int x, int y, int skillId)
    {
        switch (skillId) 
        {
            case 0: cards[y][x].SetValue(1); break;
            case 1: //十字に表にする
                yield return Toss(x,     y,      1);
                yield return Toss(x+1,   y,      1);
                yield return Toss(x-1,   y,      1);
                yield return Toss(x,     y+1,    1);
                yield return Toss(x,     y-1,    1);
                break;
        }

        yield return new WaitForSeconds(1);
    }

    int currentTurnEraceCount = 0;
    public int currentTurnRensaCount = 0;
    IEnumerator CheckErace()
    {
        isEraceFrag = false;
        currentTurnEraceCount = 0;
        for (int y = 0; y < cards.Length; y++)
        {
            for (int x = 0; x < cards[y].Length; x++)
            {
                if (ShouldErace(x, y, out int connectDirection))
                {
                    if (!isEraceFrag)
                    {
                        //初めてのeraceFlag=true
                        currentTurnRensaCount++;
                        avatar.SetRensaReaction(currentTurnRensaCount);
                        if(currentTurnRensaCount >= 2)
                            StartCoroutine(gameSceneUI.PlayRensaAnim(currentTurnRensaCount));
                    }
                    isEraceFrag = true;
                    SetEraceFrag(x, y, connectDirection);
                    yield return new WaitForSeconds(eraceFlagOneAnimTime);
                }
                //yield return CheckErace(x, y);
            }
        }
        //if (isEraceFrag) yield return new WaitForSeconds(eraceFragAnimTime);
    }

    bool isEraceFrag = false;
    bool eracedOnThisLoop;

    // out int connectDirection は素数の掛け算で表す
    // 2 下
    // 3 左
    // 5 右
    // 7 上
    /// <returns>isEraceFrag</returns>
    bool ShouldErace(int x1, int y1, out int connectDirection)
    {
        bool result = false;
        connectDirection = 1;

        if (cards[y1][x1] == null) return false;
        if (GetValue(x1, y1) == 0) return false;
        int x2 = x1;
        int y2 = y1;

        if (y1 > 0               && IsSame(x1, y1 - 1, x2, y2)) { PlayConectAnim(x1, y1, x2, y2); result = true; connectDirection *= 2;}  //下チェック
        if (x1 > 0               && IsSame(x1 - 1, y1, x2, y2)) { PlayConectAnim(x1, y1, x2, y2); result = true; connectDirection *= 3;}  //左チェック
        if (x1 < BOARD_WIDTH - 1 && IsSame(x1 + 1, y1, x2, y2)) { PlayConectAnim(x1, y1, x2, y2); result = true; connectDirection *= 5;}  //右チェック
        if (y1 < BOARD_WIDTH-1   && IsSame(x1  , y1+1, x2, y2)) { PlayConectAnim(x1, y1, x2, y2); result = true; connectDirection *= 7;}  //上チェック

        return result;
    }

    void PlayConectAnim(int x1, int y1, int x2, int y2)
    {
        //線を表示
        //コインの周囲に枠線を付ける
        //☆のパーティクル
    }

    [SerializeField] float eraceFlagOneAnimTime = 0.5f;
    [SerializeField] float eraceFragAnimTime = 0.5f;    //入手確定演出時間

    //2つのValueを比較し、どちらも1以上の同じ値ならtrueを返す
    bool IsSame(int x1, int y1, int x2, int y2)
    {
        if (cards[y1][x1] == null) return false;
        if (cards[y2][x2] == null) return false;
        int value1 = GetValue(x1, y1);
        int value2 = GetValue(x2, y2);
        if (value1 == 0 || value2 == 0) return false;
        return value1 == value2;
    }

    void SetEraceFrag(int x, int y, int connectDirection)
    {
        eracedOnThisLoop = true;

        StartCoroutine(cards[y][x].PlayConnectAnim(connectDirection));

        cards[y][x].SetEraceFrag(true);

        CommonData.SoundId soundId = (CommonData.SoundId)Mathf.Min((int)(CommonData.SoundId.ConnectSound_00 + currentTurnEraceCount + currentTurnRensaCount * 2 - 2), (int)CommonData.SoundId.ConnectSound_16);
        SoundManager.PlaySound(soundId);

        currentTurnEraceCount++;
        
        //周囲のChainFragをtrueにする
        if (x > 0               ) SetChainFrag(x - 1, y); //左チェック
        if (x < BOARD_WIDTH-1   ) SetChainFrag(x + 1, y); //右チェック
        if (y > 0               ) SetChainFrag(x, y - 1);
        if (y < BOARD_WIDTH-1   ) SetChainFrag(x, y + 1);
    }

    /// <returns>is false to true</returns>
    bool SetChainFrag(int x, int y)
    {
        if (cards[y][x] == null) return false;
        cards[y][x].SetChainFrag(true);
        return true;
    }

    int GetValue(int x, int y)
    {
        if (cards[y][x] == null)
        {
            Debug.LogError($"エラー GetValue {x},{y}");
            return -1;
        }
        return cards[y][x].GetValue();
    }

    IEnumerator EracePhase()
    {
        bool isEraced = false;
        List<Coin> erasedCards = new List<Coin>();
        List<Tween> tweens = new List<Tween>();

        int eraceCount = 0;
        for (int y = 0; y < cards.Length; y++)
        {
            for (int x = 0; x < cards[y].Length; x++)
            {
                if (cards[y][x] == null) continue;
                if (cards[y][x].eraceFrag)
                {
                    Coin card = cards[y][x];
                    cards[y][x] = null;          // 盤面上は先に空ける
                    erasedCards.Add(card);
                    tweens.Add(card.PlayEraceAnim(gameSceneUI.GetBankTransform()).SetDelay(erasedCards.Count * 0.03f));
                    isEraced = true;
                    eraceCount++;
                }
            }
        }

        if (isEraced)
        {
            foreach (var tween in tweens)
            {
                yield return tween.WaitForCompletion();
            }

            foreach (var card in erasedCards)
            {
                standbyCardStack.Push(card);
            }

            gameSceneUI.AddCoin(eraceCount);
            yield return new WaitForSeconds(eraceAfterDelayTime);
        }
    }

    IEnumerator ChainOpenPhase()
    {
        bool isChainOpened = false;
        for (int y = 0; y < cards.Length; y++)
        {
            for (int x = 0; x < cards[y].Length; x++)
            {
                if (cards[y][x] != null && cards[y][x].chainFrag)
                {
                    cards[y][x].SetValueWithTossAnim(cards[y][x].GetValue()==1 ? 1 : UnityEngine.Random.Range(0,2));
                    yield return new WaitForSeconds(0.03f);
                    cards[y][x].SetChainFrag(false);
                    isChainOpened = true;
                }
            }
        }
        if (isChainOpened) yield return new WaitForSeconds(test_chainOpenedWaitTime );
    }

    IEnumerator DropPhase()
    {
        bool isDroped = false;
        for (int y = 0; y < cards.Length; y++)
        {
            for (int x = 0; x < cards[y].Length; x++)
            {
                if (cards[y][x] == null)
                {
                    if (y < BOARD_WIDTH - 1) 
                    {
                        //落とすカードを探す
                        for (int y2 = y + 1; y2 < BOARD_WIDTH; y2++)
                        {
                            if (cards[y2][x] != null)
                            {
                                MoveTo(cards[y2][x], x, y);
                                cards[y][x] = cards[y2][x];
                                cards[y2][x] = null;
                                isDroped = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (isDroped)
        {
            CommonData.SoundId slideSoundId = CommonData.SoundId.CoinSlide + UnityEngine.Random.Range(0, 4);
            SoundManager.PlaySound(slideSoundId);
            yield return new WaitForSeconds(moveDuration + moveAfterDelay);
        }
    }

    IEnumerator ChargePhase()
    {
        bool moved = false;
        for (int y = 0; y < cards.Length; y++)
        {
            for (int x = 0; x < cards[y].Length; x++)
            {
                if (cards[y][x] == null)
                {
                    cards[y][x] = standbyCardStack.Pop();

                    RectTransform rt = cards[y][x].transform as RectTransform;
                    rt.anchoredPosition = GetCellPos(x, y) + new Vector2(0, BOARD_WIDTH * 1.5f * gridY);

                    cards[y][x].Init(this);
                    MoveTo(cards[y][x], x, y);
                    moved = true;
                }
            }
        }

        CommonData.SoundId slideSoundId = CommonData.SoundId.CoinSlide + UnityEngine.Random.Range(0, 4);
        SoundManager.PlaySound(slideSoundId);
        if (moved) yield return new WaitForSeconds(moveDuration);
    }


    public void PlayClearDance()
    {
        for (int y = 0; y < cards.Length; y++)
        {
            for (int x = 0; x < cards[y].Length; x++)
            {
                if (cards[y][x] == null) continue;
                if(cards[y][x].GetValue() == 1)
                    StartCoroutine(cards[y][x].StartClearDance());
            }
        }
    }

    public void OnPointerEnterCard(int index)
    {
        if (processing) return;

        int x = IndexToX(index);
        int y = IndexToY(index);
        
        cards[y][x].StartOpenYokokuAnim();
        if(y > 0 && cards[y-1][x] != null) cards[y-1][x].StartOpenYokokuAnim();
        if(x > 0 && cards[y][x-1] != null) cards[y][x-1].StartOpenYokokuAnim();
        if(x < BOARD_WIDTH-1 && cards[y][x+1] != null) cards[y][x+1].StartOpenYokokuAnim();
        if(y < BOARD_WIDTH-1 && cards[y+1][x] != null) cards[y+1][x].StartOpenYokokuAnim();
    }

    public void OnPointerExitCard(int index)
    {
        if (processing) return;
        int x = IndexToX(index);
        int y = IndexToY(index);
        cards[y][x].EndOpenYokokuAnim();
        if(y > 0 && cards[y-1][x] != null) cards[y-1][x].EndOpenYokokuAnim();
        if(x > 0 && cards[y][x-1] != null) cards[y][x-1].EndOpenYokokuAnim();
        if(x < BOARD_WIDTH-1 && cards[y][x+1] != null) cards[y][x+1].EndOpenYokokuAnim();
        if(y < BOARD_WIDTH-1 && cards[y+1][x] != null) cards[y+1][x].EndOpenYokokuAnim();
    }
    //processing=trueのタイミングで全員のyokokuAnimを止める

    (int, int)IndesToXY(int index) => (index % BOARD_WIDTH, index / BOARD_WIDTH);
    int XYtoIndex(int x, int y) => (y * BOARD_WIDTH + x);
    int IndexToX(int x) => x % BOARD_WIDTH;
    int IndexToY(int y) => y / BOARD_WIDTH;
}
