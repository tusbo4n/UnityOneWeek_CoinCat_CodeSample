using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ClearAnim : MonoBehaviour
{
    [SerializeField] Image backGroundGrad;   // 背景の白いグラデーション alpha = 0.5
    [SerializeField] RectTransform[] titleTexts;    // それぞれの文字
    /* 0~6
     ゲ
    ー
    ム
    ク
    リ
    ア
    ！
     */
    [SerializeField] RectTransform titleButton; // タイトルに戻るボタン
    [SerializeField] RectTransform retryButton; // リトライに戻るボタン
    [SerializeField] Image leftCoinCat;         // 左の猫
    [SerializeField] Image rightCoinCat;        // 右の猫

    Vector2[] titleBasePos;
    Vector3[] titleBaseScale;
    Vector2 titleButtonBasePos;
    Vector2 retryButtonBasePos;
    Vector3 titleButtonBaseScale;
    Vector3 retryButtonBaseScale;
    Vector2 leftCoinBasePos;
    Vector2 rightCoinBasePos;
    Vector3 leftCoinBaseRot;
    Vector3 rightCoinBaseRot;

    bool cached = false;

    void CacheBaseState()
    {
        if (cached) return;
        cached = true;

        titleBasePos = new Vector2[titleTexts.Length];
        titleBaseScale = new Vector3[titleTexts.Length];
        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;
            titleBasePos[i] = titleTexts[i].anchoredPosition;
            titleBaseScale[i] = titleTexts[i].localScale;
        }

        if (titleButton != null)
        {
            titleButtonBasePos = titleButton.anchoredPosition;
            titleButtonBaseScale = titleButton.localScale;
        }

        if (retryButton != null)
        {
            retryButtonBasePos = retryButton.anchoredPosition;
            retryButtonBaseScale = retryButton.localScale;
        }

        if (leftCoinCat != null)
        {
            leftCoinBasePos = leftCoinCat.rectTransform.anchoredPosition;
            leftCoinBaseRot = leftCoinCat.rectTransform.localEulerAngles;
        }

        if (rightCoinCat != null)
        {
            rightCoinBasePos = rightCoinCat.rectTransform.anchoredPosition;
            rightCoinBaseRot = rightCoinCat.rectTransform.localEulerAngles;
        }
    }

    void ResetState()
    {
        if (backGroundGrad != null)
        {
            backGroundGrad.DOKill();
            Color c = backGroundGrad.color;
            c.a = 0f;
            backGroundGrad.color = c;
        }

        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;
            titleTexts[i].DOKill();
            titleTexts[i].anchoredPosition = titleBasePos[i];
            titleTexts[i].localScale = titleBaseScale[i];
            titleTexts[i].gameObject.SetActive(false);
        }

        if (titleButton != null)
        {
            titleButton.DOKill();
            titleButton.anchoredPosition = titleButtonBasePos + Vector2.down * 40f;
            titleButton.localScale = Vector3.one;
            titleButton.gameObject.SetActive(false);

            CanvasGroup cg = titleButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = titleButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        if (retryButton != null)
        {
            retryButton.DOKill();
            retryButton.anchoredPosition = retryButtonBasePos + Vector2.down * 40f;
            retryButton.localScale = Vector3.one;
            retryButton.gameObject.SetActive(false);

            CanvasGroup cg = retryButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = retryButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        if (leftCoinCat != null)
        {
            RectTransform rt = leftCoinCat.rectTransform;
            rt.DOKill();
            leftCoinCat.DOKill();
            rt.anchoredPosition = leftCoinBasePos;
            rt.localEulerAngles = leftCoinBaseRot;
            rt.localScale = Vector3.one * 0.8f;
            Color c = leftCoinCat.color;
            c.a = 0f;
            leftCoinCat.color = c;
        }

        if (rightCoinCat != null)
        {
            RectTransform rt = rightCoinCat.rectTransform;
            rt.DOKill();
            rightCoinCat.DOKill();
            rt.anchoredPosition = rightCoinBasePos;
            rt.localEulerAngles = rightCoinBaseRot;
            rt.localScale = Vector3.one * 0.8f;
            Color c = rightCoinCat.color;
            c.a = 0f;
            rightCoinCat.color = c;
        }
    }

    public IEnumerator Play()
    {
        CacheBaseState();
        ResetState();

        if (backGroundGrad != null)
        {
            backGroundGrad.DOFade(0.5f, 0.35f).SetEase(Ease.OutQuad);
        }

        // タイトル文字を順番に出す
        Sequence showSeq = DOTween.Sequence();

        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;

            RectTransform rt = titleTexts[i];
            rt.gameObject.SetActive(true);
            rt.localScale = Vector3.one * 0.4f;
            rt.anchoredPosition = titleBasePos[i] + Vector2.up * 16f;

            showSeq.AppendInterval(0.05f);
            showSeq.Append(rt.DOScale(1.18f, 0.10f).SetEase(Ease.OutBack));
            showSeq.Join(rt.DOAnchorPos(titleBasePos[i], 0.12f).SetEase(Ease.OutQuad));
            showSeq.Append(rt.DOScale(1f, 0.08f).SetEase(Ease.OutQuad));
        }

        // 猫を出す
        if (leftCoinCat != null)
        {
            RectTransform rt = leftCoinCat.rectTransform;
            showSeq.AppendInterval(0.05f);
            showSeq.Join(leftCoinCat.DOFade(1f, 0.18f));
            showSeq.Join(rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack));
            showSeq.Join(rt.DOAnchorPos(leftCoinBasePos, 0.18f).SetEase(Ease.OutQuad));
        }

        if (rightCoinCat != null)
        {
            RectTransform rt = rightCoinCat.rectTransform;
            showSeq.Join(rightCoinCat.DOFade(1f, 0.18f));
            showSeq.Join(rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack));
            showSeq.Join(rt.DOAnchorPos(rightCoinBasePos, 0.18f).SetEase(Ease.OutQuad));
        }

        // ボタンを出す
        if (titleButton != null)
        {
            titleButton.gameObject.SetActive(true);
            CanvasGroup cg = titleButton.GetComponent<CanvasGroup>();
            showSeq.AppendInterval(0.08f);
            showSeq.Append(cg.DOFade(1f, 0.18f));
            showSeq.Join(titleButton.DOAnchorPos(titleButtonBasePos, 0.18f).SetEase(Ease.OutBack));
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            CanvasGroup cg = retryButton.GetComponent<CanvasGroup>();
            showSeq.Join(cg.DOFade(1f, 0.18f));
            showSeq.Join(retryButton.DOAnchorPos(retryButtonBasePos, 0.18f).SetEase(Ease.OutBack));
        }

        yield return showSeq.WaitForCompletion();

        // ここから常時ループ
        // タイトル文字：ごく小さく波打つ
        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;

            RectTransform rt = titleTexts[i];
            Vector2 basePos = titleBasePos[i];

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(i * 0.06f);
            seq.Append(rt.DOAnchorPosY(basePos.y + 4f, 1.4f).SetEase(Ease.InOutSine));
            seq.Append(rt.DOAnchorPosY(basePos.y, 1.4f).SetEase(Ease.InOutSine));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }

        // 左猫
        if (leftCoinCat != null)
        {
            RectTransform rt = leftCoinCat.rectTransform;
            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOAnchorPosY(leftCoinBasePos.y + 6f, 1.8f).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(new Vector3(0, 0, -4f), 1.8f));
            seq.Append(rt.DOAnchorPosY(leftCoinBasePos.y, 1.8f).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(leftCoinBaseRot, 1.8f));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }

        // 右猫
        if (rightCoinCat != null)
        {
            RectTransform rt = rightCoinCat.rectTransform;
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.5f);
            seq.Append(rt.DOAnchorPosY(rightCoinBasePos.y + 6f, 1.8f).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(new Vector3(0, 0, 4f), 1.8f));
            seq.Append(rt.DOAnchorPosY(rightCoinBasePos.y, 1.8f).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(rightCoinBaseRot, 1.8f));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }

        // ボタン：ごく弱い呼吸
        if (titleButton != null)
        {
            titleButton
                .DOScale(titleButtonBaseScale * 1.03f, 1.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        if (retryButton != null)
        {
            retryButton
                .DOScale(retryButtonBaseScale * 1.03f, 1.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}