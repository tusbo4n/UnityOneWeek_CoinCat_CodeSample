using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Bank : MonoBehaviour
{
    [SerializeField] public  TextMeshProUGUI valueText;

    [Header("Count Up Anim")]
    [SerializeField] float countUpDuration = 0.25f;

    [Header("Jump Anim")]
    [SerializeField] float jumpHeight = 12f;
    [SerializeField] float jumpUpDuration = 0.08f;
    [SerializeField] float jumpDownDuration = 0.10f;
    [SerializeField] Image bankImage; //コイン袋の画像

    Coroutine animTween;
    int _value { get => GameSceneController.instance.Score; set => GameSceneController.instance.Score = value; }

    Vector2 baseAnchoredPos;

    void Awake()
    {
        baseAnchoredPos = valueText.rectTransform.anchoredPosition;
    }

    public void AddValue(int addValue)
    {
        PlayUpdateAnim(_value, _value + addValue);
        _value += addValue;
    }

    void PlayUpdateAnim(int beforeValue, int afterValue)
    {
        if (animTween != null)
        {
            StopCoroutine(animTween);
        }

        RectTransform rt = valueText.rectTransform;
        rt.DOKill();
        rt.anchoredPosition = baseAnchoredPos;

        // 数字のジャンプ
        Sequence jumpSeq = DOTween.Sequence();
        jumpSeq.Append(rt.DOAnchorPosY(baseAnchoredPos.y + jumpHeight, jumpUpDuration).SetEase(Ease.OutQuad));
        jumpSeq.Append(rt.DOAnchorPosY(baseAnchoredPos.y, jumpDownDuration).SetEase(Ease.InQuad));
        jumpSeq.OnComplete(() =>
        {
            rt.anchoredPosition = baseAnchoredPos;
        });

        // 袋のジャンプ
        RectTransform bankRt = bankImage.rectTransform;
        Vector2 bankBasePos = bankRt.anchoredPosition;

        bankRt.DOKill();
        bankRt.anchoredPosition = bankBasePos;

        Sequence bankJumpSeq = DOTween.Sequence();
        bankJumpSeq.Append(bankRt.DOAnchorPosY(bankBasePos.y + 8f, 0.06f).SetEase(Ease.OutQuad));
        bankJumpSeq.Append(bankRt.DOAnchorPosY(bankBasePos.y, 0.10f).SetEase(Ease.InQuad));
        bankJumpSeq.OnComplete(() =>
        {
            bankRt.anchoredPosition = bankBasePos;
        });

        animTween = StartCoroutine(CoPlayUpdateAnim(beforeValue, afterValue));
    }


    IEnumerator CoPlayUpdateAnim(int beforeValue, int afterValue)
    {
        float elapsed = 0f;

        valueText.text = beforeValue.ToString();

        while (elapsed < countUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countUpDuration);

            int currentValue = Mathf.RoundToInt(Mathf.Lerp(beforeValue, afterValue, t));
            valueText.text = currentValue.ToString();

            yield return null;
        }

        valueText.text = afterValue.ToString();
        animTween = null;
    }
}