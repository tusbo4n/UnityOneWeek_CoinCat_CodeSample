using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using TMPro;

public class GameSceneUI : MonoBehaviour
{
    [SerializeField] Image bankUI;
    [SerializeField] Bank bank;
    [SerializeField] TextMeshProUGUI normaValueText;
    [SerializeField] TextMeshProUGUI turnText;  // "n / m"

    [SerializeField] TextMeshProUGUI rensaValueText;
    [SerializeField] TextMeshProUGUI rensaSuffix;

    [SerializeField] RectTransform rensaRoot;
    Vector2 rensaRootBaseAnchoredPos;
    Vector3 rensaRootBaseScale;
    void Awake()
    {
        if (rensaRoot == null)
            rensaRoot = rensaValueText.transform.parent as RectTransform;

        if (rensaRoot != null)
        {
            rensaRootBaseAnchoredPos = rensaRoot.anchoredPosition;
            rensaRootBaseScale = rensaRoot.localScale;
        }
    }

    public Transform GetBankTransform()
    {
        return bankUI.transform;
    }

    public void AddCoin(int value)
    {
        bank.AddValue(value);
    }

    public void UpdateUI()
    {
        normaValueText.text = GameSceneController.instance.norma.ToString();
        turnText.text = GameSceneController.instance.currentTurnCount + " / " + GameSceneController.instance.maxTurnCount.ToString();
    }

    public IEnumerator PlayNormaChangedAnim()
    {
        RectTransform rt = normaValueText.rectTransform;

        rt.DOKill();
        normaValueText.DOKill();

        Vector3 baseScale = Vector3.one;
        Color baseColor = normaValueText.color;
        Color flashColor = new Color(1f, 0.9f, 0.2f, 1f);

        rt.localScale = baseScale;
        normaValueText.color = baseColor;

        Sequence seq = DOTween.Sequence();

        // 拡大して戻る
        seq.Join(
            rt.DOScale(1.6f, 0.18f)
              .SetEase(Ease.OutBack)
        );
        seq.Append(
            rt.DOScale(1f, 0.18f)
              .SetEase(Ease.InQuad)
        );

        // 色を点滅
        seq.Join(
            normaValueText.DOColor(flashColor, 0.08f)
                .SetLoops(4, LoopType.Yoyo)
        );

        yield return seq.WaitForCompletion();

        rt.localScale = baseScale;
        normaValueText.color = baseColor;
    }

    public IEnumerator PlayGameOverAnim()
    {
        TextMeshProUGUI bankValueText = bank.valueText;

        bankValueText.DOKill();
        normaValueText.DOKill();
        normaValueText.rectTransform.DOKill();

        Color bankBaseColor = bankValueText.color;
        Color normaBaseColor = normaValueText.color;

        Color bankFlashColor = new Color(1f, 0.9f, 0.2f, 1f);   // 黄色寄り
        Color normaFlashColor = new Color(0.5f, 0.8f, 1f, 1f); // 達していない感じの青

        Sequence seq = DOTween.Sequence();
        seq.SetLink(gameObject);

        seq.Join(
            bankValueText.DOColor(bankFlashColor, 0.25f)
                .SetLoops(-1, LoopType.Yoyo)
        );

        seq.Join(
            normaValueText.DOColor(normaFlashColor, 0.25f)
                .SetLoops(-1, LoopType.Yoyo)
        );

        normaValueText.rectTransform
            .DOScale(1.1f, 0.25f)
            .SetLoops(-1, LoopType.Yoyo);

        yield return null;
    }

    public IEnumerator PlayRensaAnim(int rensaValue)
    {
        if (rensaValue == 0) yield break;

        rensaValueText.text = rensaValue.ToString();

        RectTransform rootRt = rensaValueText.transform.parent as RectTransform;
        if (rootRt == null) yield break;

        if (rensaRoot == null) yield break;

        rensaRoot.DOKill();
        rensaRoot.anchoredPosition = rensaRootBaseAnchoredPos;
        rensaRoot.localScale = rensaRootBaseScale;

        rootRt.DOKill();
        rensaValueText.DOKill();
        rensaSuffix.DOKill();

        rensaValueText.gameObject.SetActive(true);
        rensaSuffix.gameObject.SetActive(true);
        rootRt.gameObject.SetActive(true);

        Vector3 rootBaseScale = Vector3.one;
        rootRt.localScale = rootBaseScale;

        Color baseColor;
        Color flashColor;

        // 連鎖数に応じて色を変える
        if (rensaValue <= 1)
        {
            baseColor = new Color32(255, 255, 255, 255);
            flashColor = new Color32(255, 245, 170, 255);
        }
        else if (rensaValue <= 3)
        {
            baseColor = new Color32(255, 230, 120, 255);
            flashColor = new Color32(255, 255, 220, 255);
        }
        else if (rensaValue <= 5)
        {
            baseColor = new Color32(255, 170, 80, 255);
            flashColor = new Color32(255, 240, 180, 255);
        }
        else
        {
            baseColor = new Color32(255, 110, 110, 255);
            flashColor = new Color32(255, 230, 230, 255);
        }

        rensaValueText.color = baseColor;
        rensaSuffix.color = baseColor;

        // 連鎖数が多いほど大きく
        float maxScale = Mathf.Clamp(1f + rensaValue * 0.12f, 1f, 1.8f);

        Sequence seq = DOTween.Sequence();

        // 親ごと拡大
        seq.Join(rootRt.DOScale(maxScale, 0.12f).SetEase(Ease.OutBack));

        // 色を点滅
        seq.Join(rensaValueText.DOColor(flashColor, 0.08f).SetLoops(4, LoopType.Yoyo));
        seq.Join(rensaSuffix.DOColor(flashColor, 0.08f).SetLoops(4, LoopType.Yoyo));

        // 少し落ち着かせる
        seq.Append(rootRt.DOScale(maxScale * 0.92f, 0.10f).SetEase(Ease.OutQuad));

        yield return seq.WaitForCompletion();

        rensaValueText.color = baseColor;
        rensaSuffix.color = baseColor;
    }

    public IEnumerator PlayRensaToScoreAnim()
    {
        Transform target = bank.transform;
        RectTransform rootRt = rensaValueText.transform.parent as RectTransform;
        RectTransform targetRt = target as RectTransform;
        RectTransform parentRt = rootRt != null ? rootRt.parent as RectTransform : null;

        if (rootRt == null || targetRt == null || parentRt == null) yield break;

        rootRt.DOKill();
        rensaValueText.DOKill();
        rensaSuffix.DOKill();

        Vector3 baseScale = rootRt.localScale;

        Vector2 targetPos;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetRt.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenPos, null, out targetPos);

        Sequence seq = DOTween.Sequence();

        seq.Append(rootRt.DOScale(baseScale * 1.1f, 0.08f).SetEase(Ease.OutQuad));
        seq.Append(rootRt.DOAnchorPos(targetPos, 0.35f).SetEase(Ease.InQuad));
        seq.Join(rootRt.DOScale(baseScale * 0.4f, 0.35f).SetEase(Ease.InBack));
        seq.Join(rensaValueText.DOFade(0f, 0.35f));
        seq.Join(rensaSuffix.DOFade(0f, 0.35f));

        yield return seq.WaitForCompletion();

        rootRt.localScale = Vector3.one;

        Color c1 = rensaValueText.color;
        Color c2 = rensaSuffix.color;
        c1.a = 1f;
        c2.a = 1f;
        rensaValueText.color = c1;
        rensaSuffix.color = c2;

        rootRt.gameObject.SetActive(false);
    }

    public IEnumerator PlayRensaHideAnim()
    {
        RectTransform rootRt = rensaValueText.transform.parent as RectTransform;
        if (rootRt == null) yield break;

        rootRt.DOKill();
        rensaValueText.DOKill();
        rensaSuffix.DOKill();

        Color valueBaseColor = rensaValueText.color;
        Color suffixBaseColor = rensaSuffix.color;

        Sequence seq = DOTween.Sequence();

        seq.Join(rensaValueText.DOFade(0f, 0.15f));
        seq.Join(rensaSuffix.DOFade(0f, 0.15f));
        seq.Join(rootRt.DOScale(0.8f, 0.15f).SetEase(Ease.InBack));

        yield return seq.WaitForCompletion();

        // 次回用に戻す
        rootRt.localScale = Vector3.one;

        valueBaseColor.a = 1f;
        suffixBaseColor.a = 1f;
        rensaValueText.color = valueBaseColor;
        rensaSuffix.color = suffixBaseColor;

        rootRt.gameObject.SetActive(false);
    }
}
