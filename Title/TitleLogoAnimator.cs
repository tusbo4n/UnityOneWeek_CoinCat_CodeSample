using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleLogoAnimator : MonoBehaviour
{
    [SerializeField] RectTransform[] titleTexts;
    /* 0~12
    つ
    な
    が
    れ
    ！
    コ
    イ
    ン
    キ
    ャ
    ッ
    ト
    ！
    */

    [SerializeField] Image leftCoinCat;
    [SerializeField] Image rightCoinCat;

    [SerializeField] float textMoveY = 3f;
    [SerializeField] float textDuration = 1.4f;
    [SerializeField] float textDelayStep = 0.08f;

    [SerializeField] float coinMoveY = 4f;
    [SerializeField] float coinRotZ = 3f;
    [SerializeField] float coinDuration = 2.2f;

    void Start()
    {
        PlayLoopAnim();
    }

    void PlayLoopAnim()
    {
        foreach (var rt in titleTexts)
        {
            if (rt == null) continue;
            rt.DOKill();
        }

        if (leftCoinCat != null) leftCoinCat.rectTransform.DOKill();
        if (rightCoinCat != null) rightCoinCat.rectTransform.DOKill();

        // 文字はごく小さく、ゆっくり波打つ
        for (int i = 0; i < titleTexts.Length; i++)
        {
            if (titleTexts[i] == null) continue;

            RectTransform rt = titleTexts[i];
            Vector2 basePos = rt.anchoredPosition;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(i * textDelayStep);
            seq.Append(rt.DOAnchorPosY(basePos.y + textMoveY, textDuration).SetEase(Ease.InOutSine));
            seq.Append(rt.DOAnchorPosY(basePos.y, textDuration).SetEase(Ease.InOutSine));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }

        // 左コインキャットはごく控えめに浮く
        if (leftCoinCat != null)
        {
            RectTransform rt = leftCoinCat.rectTransform;
            Vector2 basePos = rt.anchoredPosition;
            Vector3 baseRot = rt.localEulerAngles;

            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOAnchorPosY(basePos.y + coinMoveY, coinDuration).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(new Vector3(0f, 0f, -coinRotZ), coinDuration));
            seq.Append(rt.DOAnchorPosY(basePos.y, coinDuration).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(baseRot, coinDuration));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }

        // 右コインキャットは少し遅れて動く
        if (rightCoinCat != null)
        {
            RectTransform rt = rightCoinCat.rectTransform;
            Vector2 basePos = rt.anchoredPosition;
            Vector3 baseRot = rt.localEulerAngles;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.7f);
            seq.Append(rt.DOAnchorPosY(basePos.y + coinMoveY, coinDuration).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(new Vector3(0f, 0f, coinRotZ), coinDuration));
            seq.Append(rt.DOAnchorPosY(basePos.y, coinDuration).SetEase(Ease.InOutSine));
            seq.Join(rt.DOLocalRotate(baseRot, coinDuration));
            seq.SetLoops(-1, LoopType.Restart);
            seq.SetLink(gameObject);
        }
    }

    void OnDisable()
    {
        foreach (var rt in titleTexts)
        {
            if (rt == null) continue;
            rt.DOKill();
        }

        if (leftCoinCat != null) leftCoinCat.rectTransform.DOKill();
        if (rightCoinCat != null) rightCoinCat.rectTransform.DOKill();
    }
}