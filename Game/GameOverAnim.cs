using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameOverAnim : MonoBehaviour
{
    [SerializeField] Image backGroundGrad;   // 背景の黒いグラデーション alpha = 0.5
    [SerializeField] RectTransform[] titleTexts;    // それぞれの文字
    /* 0~6
     ゲ
    ー
    ム
    オ
    ー
    バ
    ー
     */
    [SerializeField] RectTransform titleButton; // タイトルに戻るボタン
    [SerializeField] RectTransform retryButton; // リトライに戻るボタン

    [Header("Drop Anim")]
    [SerializeField] float charDropStartInterval = 0.06f;   // 次の文字が降り始めるまでの間隔
    [SerializeField] float charDropDuration = 0.22f;
    [SerializeField] float charBounceUpDuration = 0.10f;
    [SerializeField] float charBounceDownDuration = 0.12f;
    [SerializeField] float charBounceHeight = 18f;

    Vector2[] titleBasePos;
    Vector3[] titleBaseScale;
    Vector2 titleButtonBasePos;
    Vector2 retryButtonBasePos;
    Vector3 titleButtonBaseScale;
    Vector3 retryButtonBaseScale;

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
            titleTexts[i].localRotation = Quaternion.identity;
            titleTexts[i].gameObject.SetActive(false);
        }

        if (titleButton != null)
        {
            titleButton.DOKill();
            titleButton.anchoredPosition = titleButtonBasePos + Vector2.down * 28f;
            titleButton.localScale = Vector3.one;
            titleButton.gameObject.SetActive(false);

            CanvasGroup cg = titleButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = titleButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        if (retryButton != null)
        {
            retryButton.DOKill();
            retryButton.anchoredPosition = retryButtonBasePos + Vector2.down * 28f;
            retryButton.localScale = Vector3.one;
            retryButton.gameObject.SetActive(false);

            CanvasGroup cg = retryButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = retryButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
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

        // タイトル文字を1文字ずつ、上から降ってきてバウンド
        float longestCharAnimTime = 0f;

        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;

            RectTransform rt = titleTexts[i];
            rt.gameObject.SetActive(true);

            Vector2 basePos = titleBasePos[i];
            rt.anchoredPosition = basePos + Vector2.up * 320f;
            rt.localScale = Vector3.one * 0.95f;

            float delay = i * charDropStartInterval;
            float animLength = charDropDuration + charBounceUpDuration + charBounceDownDuration + 0.10f;
            longestCharAnimTime = Mathf.Max(longestCharAnimTime, delay + animLength);

            Sequence charSeq = DOTween.Sequence();
            charSeq.AppendInterval(delay);

            // 落下
            charSeq.Append(rt.DOAnchorPosY(basePos.y, charDropDuration).SetEase(Ease.InQuad));

            // バウンド
            charSeq.Append(rt.DOAnchorPosY(basePos.y + charBounceHeight, charBounceUpDuration).SetEase(Ease.OutQuad));
            charSeq.Append(rt.DOAnchorPosY(basePos.y, charBounceDownDuration).SetEase(Ease.InQuad));

            // 少しだけ潰れて戻る感じ
            charSeq.Join(rt.DOScale(new Vector3(1.08f, 0.92f, 1f), 0.08f).SetEase(Ease.OutQuad));
            charSeq.Append(rt.DOScale(Vector3.one, 0.10f).SetEase(Ease.OutQuad));

            charSeq.SetLink(gameObject);
        }

        // 文字がある程度出揃うまで待つ
        yield return new WaitForSeconds(longestCharAnimTime);

        // ボタンを少し遅れて表示
        Sequence buttonSeq = DOTween.Sequence();

        if (titleButton != null)
        {
            titleButton.gameObject.SetActive(true);
            CanvasGroup cg = titleButton.GetComponent<CanvasGroup>();

            buttonSeq.AppendInterval(0.08f);
            buttonSeq.Append(cg.DOFade(1f, 0.18f));
            buttonSeq.Join(titleButton.DOAnchorPos(titleButtonBasePos, 0.18f).SetEase(Ease.OutQuad));
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            CanvasGroup cg = retryButton.GetComponent<CanvasGroup>();

            buttonSeq.Join(cg.DOFade(1f, 0.18f));
            buttonSeq.Join(retryButton.DOAnchorPos(retryButtonBasePos, 0.18f).SetEase(Ease.OutQuad));
        }

        yield return buttonSeq.WaitForCompletion();

        // ここから常時ループ
        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;

            RectTransform rt = titleTexts[i];
            Vector2 basePos = titleBasePos[i];

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(i * 0.05f);
            seq.Append(rt.DOAnchorPosY(basePos.y - 3f, 1.6f).SetEase(Ease.InOutSine));
            seq.Append(rt.DOAnchorPosY(basePos.y, 1.6f).SetEase(Ease.InOutSine));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }

        if (titleButton != null)
        {
            titleButton
                .DOScale(titleButtonBaseScale * 1.015f, 1.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        if (retryButton != null)
        {
            retryButton
                .DOScale(retryButtonBaseScale * 1.015f, 1.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}