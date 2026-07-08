using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    [SerializeField] int index;
    [SerializeField] Image image;
    [SerializeField] ImageController imageController;
    [SerializeField] Image connectEffect;
    //0:裏
    //1:表
    //2:裏(トス予告)
    //isOmote => value==1
    [SerializeField] Image connecEffect_2;  //下
    [SerializeField] Image connecEffect_6;  //右
    [SerializeField] Image connecEffect_4;  //左
    [SerializeField] Image connecEffect_8;  //上


    GameBoard gameBoard;
    int value;
    public bool eraceFrag = false;
    public bool chainFrag = false;

    public void Init(GameBoard bord)
    {
        transform.DOKill();
        image.rectTransform.DOKill();

        gameBoard = bord;
        SetValue(0);
        eraceFrag = false;
        chainFrag = false;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        image.rectTransform.localRotation = Quaternion.identity;
        connectEffect.gameObject.SetActive(false);
        connecEffect_2.gameObject.SetActive(false);
        connecEffect_4.gameObject.SetActive(false);
        connecEffect_6.gameObject.SetActive(false);
        connecEffect_8.gameObject.SetActive(false);
        UpdateUI();
    }

    void UpdateUI()
    {
        int spriteIndex = 0;
        if (eraceFrag)
        {
            spriteIndex = 3;
            connectEffect.gameObject.SetActive(true);
        }
        else if (value == 1) spriteIndex = 1;
        else if (chainFrag) spriteIndex = 2;
        imageController.ChangeSprite(spriteIndex);
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }

    public void SetValue(int value, bool playTurnAnim = false)
    {
        this.value = value;
        if (playTurnAnim)
        {
            StartCoroutine(PlayTurnAnim());
        }
        else
        {
            UpdateUI();
        }
    }

    public void SetValueWithTossAnim(int value)
    {
        this.value = value;
        StartCoroutine(PlayTossAnim());
    }

    public int GetValue()
    {
        return value;
    }

    public void OnClick()
    {
        gameBoard.OnClickChild(index);
    }

    public void OnPointerEnter()
    {
        gameBoard.OnPointerEnterCard(index);
    }

    public void OnPointerExit()
    {
        gameBoard.OnPointerExitCard(index);
    }

    Tween openYokokuScaleTween;
    Tween openYokokuFadeTween;
    Vector3 openYokokuBaseScale = Vector3.one;
    Color openYokokuBaseColor = Color.white;
    bool openYokokuCached = false;

    public void StartOpenYokokuAnim()
    {
        if (!openYokokuCached)
        {
            openYokokuBaseScale = image.rectTransform.localScale;
            openYokokuBaseColor = image.color;
            openYokokuCached = true;
        }

        image.DOKill();
        image.rectTransform.DOKill();

        image.rectTransform.DOScale(openYokokuBaseScale * 1.12f, 0.1f).SetEase(Ease.OutQuad);

        Color targetColor = openYokokuBaseColor * 1.2f;
        targetColor.a = openYokokuBaseColor.a;
        image.DOColor(targetColor, 0.1f).SetEase(Ease.OutQuad);
    }

    public void EndOpenYokokuAnim()
    {
        image.DOKill();
        image.rectTransform.DOKill();

        image.rectTransform.DOScale(openYokokuBaseScale, 0.08f).SetEase(Ease.OutQuad);
        image.DOColor(openYokokuBaseColor, 0.08f).SetEase(Ease.OutQuad);
    }

    public void SetEraceFrag(bool value)
    {
        eraceFrag = value;
        UpdateUI();
    }

    public void SetChainFrag(bool value)
    {
        if (eraceFrag) return;
        chainFrag = value;
        UpdateUI();
        //image.color = value ? Color.yellow : Color.white;
    }

    IEnumerator PlayTurnAnim()
    {
        RectTransform rt = image.rectTransform;
        rt.DOKill();

        Vector3 baseScale = rt.localScale;
        Vector3 baseRot = rt.localEulerAngles;

        float halfDuration = 0.09f;

        Sequence seq = DOTween.Sequence();

        seq.Append(rt.DOScaleX(0f, halfDuration).SetEase(Ease.InQuad))
           .Join(rt.DOLocalRotate(new Vector3(0f, 0f, 8f), halfDuration))
           .AppendCallback(() =>
           {
               UpdateUI();
           })
           .Append(rt.DOScaleX(baseScale.x, halfDuration).SetEase(Ease.OutQuad))
           .Join(rt.DOLocalRotate(baseRot, halfDuration))
           .Append(rt.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.08f, 1, 0f))
           .SetLink(gameObject);

        yield return seq.WaitForCompletion();

        rt.localScale = baseScale;
        rt.localEulerAngles = baseRot;
        UpdateUI();
    }

    IEnumerator PlayTossAnim()
    {
        RectTransform rt = image.rectTransform;
        rt.DOKill();

        CommonData.SoundId tossSoundId = CommonData.SoundId.CoinToss + UnityEngine.Random.Range(0, 3);
        SoundManager.PlaySound(tossSoundId);

        Vector3 baseScale = rt.localScale;
        Vector2 basePos = rt.anchoredPosition;

        // 現在見えている面を保存
        int visibleValue = value == 1 ? 0 : 1;

        // 何回ひっくり返るか（3～5回）
        int flipCount = Random.Range(3, 6);

        // 全体時間
        float totalDuration = 0.45f;

        // 1回の反転にかける時間
        float oneFlipDuration = totalDuration / flipCount;

        // 放り投げる高さ
        float jumpHeight = 60f;

        Sequence seq = DOTween.Sequence();
        seq.SetLink(gameObject);

        // 上に放って落ちる感じ
        seq.Join(
            rt.DOAnchorPosY(basePos.y + jumpHeight, totalDuration * 0.45f)
              .SetEase(Ease.OutQuad)
        );
        seq.Append(
            rt.DOAnchorPosY(basePos.y, totalDuration * 0.55f)
              .SetEase(Ease.InQuad)
        );

        // 回転っぽく見せる
        for (int i = 0; i < flipCount; i++)
        {
            bool isLastFlip = (i == flipCount - 1);

            seq.Insert(i * oneFlipDuration,
                rt.DOScaleX(0f, oneFlipDuration * 0.5f).SetEase(Ease.InQuad)
            );

            seq.InsertCallback(i * oneFlipDuration + oneFlipDuration * 0.5f, () =>
            {
                if (isLastFlip)
                {
                    // 最後は引数で与えられた最終値で着地
                    UpdateUI();
                }
                else
                {
                    // 途中は見た目だけ交互に反転
                    visibleValue = (visibleValue == 1) ? 0 : 1;

                    int spriteIndex = 0;
                    if (eraceFrag) spriteIndex = 3;
                    else if (visibleValue == 1) spriteIndex = 1;
                    else if (chainFrag) spriteIndex = 2;

                    imageController.ChangeSprite(spriteIndex);
                }
            });

            seq.Insert(i * oneFlipDuration + oneFlipDuration * 0.5f,
                rt.DOScaleX(baseScale.x, oneFlipDuration * 0.5f).SetEase(Ease.OutQuad)
            );
        }

        // 着地時の軽いバウンド
        seq.Append(rt.DOPunchScale(new Vector3(0.12f, -0.08f, 0f), 0.12f, 1, 0f));
        seq.Join(rt.DOAnchorPosY(basePos.y + 6f, 0.06f).SetLoops(2, LoopType.Yoyo));

        yield return seq.WaitForCompletion();

        CommonData.SoundId landingSoundId = CommonData.SoundId.CoinLanding + UnityEngine.Random.Range(0, 6);
        SoundManager.PlaySound(landingSoundId);

        rt.localScale = baseScale;
        rt.anchoredPosition = basePos;
        UpdateUI();
    }

    //コインが繋がった時のアニメーション
    public IEnumerator PlayConnectAnim(int direction)
    {
        // connectEffect のアニメーションも追加する
        // connectEffectはImage型で、コインの周囲を纏うオーラのような見た目
        if (connectEffect != null)
        {
            RectTransform connectRt = connectEffect.rectTransform;

            connectEffect.DOKill();
            connectRt.DOKill();

            connectEffect.gameObject.SetActive(true);

            Color auraBaseColor = connectEffect.color;
            Color auraColor = auraBaseColor;
            auraColor.a = 0f;
            connectEffect.color = auraColor;

            connectRt.localScale = new Vector3(0.8f, 0.8f, 1f);

            Sequence auraSeq = DOTween.Sequence();
            auraSeq.Append(connectEffect.DOFade(0.9f, 0.06f));
            auraSeq.Join(connectRt.DOScale(1, 0.10f).SetEase(Ease.OutQuad));
            auraSeq.Append(connectEffect.DOFade(0f, 0.10f));
            auraSeq.Join(connectRt.DOScale(1, 0.10f).SetEase(Ease.OutQuad));
        }

        List<Image> targetImages = new List<Image>();

        if (direction % 2 == 0) targetImages.Add(connecEffect_2);
        if (direction % 3 == 0) targetImages.Add(connecEffect_4);
        if (direction % 5 == 0) targetImages.Add(connecEffect_6);
        if (direction % 7 == 0) targetImages.Add(connecEffect_8);

        if (targetImages.Count == 0) yield break;

        Sequence masterSeq = DOTween.Sequence();

        foreach (var image in targetImages)
        {
            if (image == null) continue;

            RectTransform rt = image.rectTransform;

            image.DOKill();
            rt.DOKill();

            image.gameObject.SetActive(true);

            Color baseColor = image.color;
            Color c = baseColor;
            c.a = 0f;
            image.color = c;

            bool isVertical = (image == connecEffect_2 || image == connecEffect_8);

            rt.localScale = isVertical
                ? new Vector3(1f, 0.2f, 1f)
                : new Vector3(0.2f, 1f, 1f);

            Sequence seq = DOTween.Sequence();

            seq.Append(image.DOFade(1f, 0.05f));

            if (isVertical)
                seq.Join(rt.DOScaleY(1, 0.08f).SetEase(Ease.OutQuad));
            else
                seq.Join(rt.DOScaleX(1, 0.08f).SetEase(Ease.OutQuad));

            seq.AppendInterval(0.01f);

            seq.Append(image.DOFade(0f, 0.08f));

            if (isVertical)
                seq.Join(rt.DOScaleY(1, 0.08f).SetEase(Ease.OutQuad));
            else
                seq.Join(rt.DOScaleX(1, 0.08f).SetEase(Ease.OutQuad));

            masterSeq.Join(seq);
        }

        yield return masterSeq.WaitForCompletion();

        if (connectEffect != null)
        {
            RectTransform connectRt = connectEffect.rectTransform;
            connectEffect.DOKill();
            connectRt.DOKill();

            Color c = connectEffect.color;
            c.a = 1f;
            connectEffect.color = c;
            connectRt.localScale = Vector3.one;
            connectEffect.gameObject.SetActive(false);
        }

        foreach (var image in targetImages)
        {
            if (image == null) continue;

            RectTransform rt = image.rectTransform;
            image.DOKill();
            rt.DOKill();

            Color c = image.color;
            c.a = 1f;
            image.color = c;
            rt.localScale = Vector3.one;
            image.gameObject.SetActive(false);
        }
    }

    public Tween PlayEraceAnim(Transform target)
    {
        connectEffect.gameObject.SetActive(false);

        transform.DOKill();
        image.rectTransform.DOKill();

        Vector3 startScale = transform.localScale;
        Vector3 startRot = image.rectTransform.localEulerAngles;

        Sequence seq = DOTween.Sequence();
        seq.SetLink(gameObject);

        // ちょっと気持ちよく膨らむ
        seq.Append(transform.DOScale(startScale * 1.15f, 0.08f).SetEase(Ease.OutQuad));

        // HUDへ飛ぶ
        seq.Append(transform.DOJump(
            target.position,
            1.2f,   // jumpPower
            1,      // numJumps
            0.35f   // duration
        ).SetEase(Ease.InQuad));

        // 飛んでる最中に少し回す
        seq.Join(image.rectTransform.DOLocalRotate(
            new Vector3(0f, 0f, 120f),
            0.35f,
            RotateMode.FastBeyond360
        ));

        // 吸い込まれながら縮む
        seq.Join(transform.DOScale(0f, 0.35f).SetEase(Ease.InBack)).OnComplete(() => 
        {
            CommonData.SoundId pocketInCointSound = CommonData.SoundId.PoketInCoin + UnityEngine.Random.Range(0, 4);
            SoundManager.PlaySound(pocketInCointSound);
        });

        CommonData.SoundId slideSoundId = CommonData.SoundId.CoinSlide + UnityEngine.Random.Range(0, 4);
        SoundManager.PlaySound(slideSoundId);

        return seq;
    }

    public IEnumerator StartClearDance()
    {
        this.imageController.ChangeSprite(3);

        // クリアした後、表のコインなら呼ばれる
        RectTransform rt = image.rectTransform;

        rt.DOKill();

        Vector2 basePos = rt.anchoredPosition;
        Vector3 baseRot = rt.localEulerAngles;
        Vector3 baseScale = rt.localScale;

        // 全員同時だとうるさいので少しずらす
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.6f));

        while (true)
        {
            Sequence seq = DOTween.Sequence();

            float jumpPower = UnityEngine.Random.Range(8f, 14f);
            float rot = UnityEngine.Random.Range(-6f, 6f);

            // ぴょこっと跳ねる
            seq.Append(rt.DOAnchorPosY(basePos.y + jumpPower, 0.18f).SetEase(Ease.OutQuad));
            seq.Join(rt.DOLocalRotate(new Vector3(0f, 0f, rot), 0.18f).SetEase(Ease.OutQuad));
            seq.Join(rt.DOScale(baseScale * 1.06f, 0.12f).SetEase(Ease.OutQuad));

            // 着地
            seq.Append(rt.DOAnchorPosY(basePos.y, 0.20f).SetEase(Ease.InQuad));
            seq.Join(rt.DOLocalRotate(baseRot, 0.20f).SetEase(Ease.InQuad));
            seq.Join(rt.DOScale(baseScale, 0.18f).SetEase(Ease.OutQuad));

            // 少しだけ間を置く
            seq.AppendInterval(UnityEngine.Random.Range(0.25f, 0.7f));

            yield return seq.WaitForCompletion();

            // 念のため毎回戻す
            rt.anchoredPosition = basePos;
            rt.localEulerAngles = baseRot;
            rt.localScale = baseScale;
        }
    }
}
