using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Avatar : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] ImageController imageController;
    /*
    0...睡眠
    1...通常
    2...喜び
    3...驚愕
    */

    bool isRainbow = false;

    Sequence rainbowColorSeq;
    Tween rainbowScaleTween;

    Tween swayTween;
    Tween hopTween;

    Vector3 baseLocalPos;
    Vector3 baseImageEuler;
    Vector3 baseImageScale;

    Tween superRotateTween;
    bool isSuperSpin = false;

    void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseImageEuler = image.rectTransform.localEulerAngles;
        baseImageScale = image.rectTransform.localScale;

        // 足元を軸に揺れる感じにする
        image.rectTransform.pivot = new Vector2(0.5f, 0.08f);
    }

    void Start()
    {
        StartIdleSway();
    }

    float swayAngle = 4f;
    float swayDuration = 1.1f;

    void StartIdleSway()
    {
        StopIdleSway();

        image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -swayAngle);

        swayTween = image.rectTransform
            .DOLocalRotate(new Vector3(0f, 0f, swayAngle), swayDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    void StopIdleSway()
    {
        if (swayTween != null && swayTween.IsActive())
        {
            swayTween.Kill();
            swayTween = null;
        }
    }

public void SetRensaReaction(int rensaCount)
{
    // -1...GameOver

    if (rensaCount >= 7)
    {
        if (!isRainbow)
        {
            isRainbow = true;
            PlayRainbowAnim();
        }
    }
    else
    {
        if (isRainbow)
        {
            StopRainbowAnim();
        }
    }

    // 10連鎖以上ならZ軸回転
    if (rensaCount >= 10)
    {
        if (!isSuperSpin)
        {
            isSuperSpin = true;
            PlaySuperSpinAnim();
        }
    }
    else
    {
        if (isSuperSpin)
        {
            StopSuperSpinAnim();
        }
    }

    if (rensaCount == -1)
    {
        imageController.ChangeSprite(3);
    }
    else if (rensaCount == 0)
    {
        imageController.ChangeSprite(0);
    }
    else if (rensaCount < 4)
    {
        imageController.ChangeSprite(1);
    }
    else if (rensaCount < 7)
    {
        imageController.ChangeSprite(2);
    }
    else
    {
        imageController.ChangeSprite(3);
    }

    if (rensaCount == 1)
    {
        PlaySmallHop();
    }
}

    void PlaySmallHop()
    {
        if (hopTween != null && hopTween.IsActive())
        {
            hopTween.Kill();
            hopTween = null;
        }

        transform.localPosition = baseLocalPos;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMoveY(baseLocalPos.y + 8f, 0.10f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOLocalMoveY(baseLocalPos.y, 0.12f).SetEase(Ease.InQuad));

        hopTween = seq;
    }

    void PlayRainbowAnim()
    {
        StopRainbowAnimCore();

        image.color = Color.white;
        transform.localScale = Vector3.one;

        rainbowColorSeq = DOTween.Sequence();
        rainbowColorSeq.Append(image.DOColor(Color.red, 0.25f));
        rainbowColorSeq.Append(image.DOColor(new Color(1f, 0.5f, 0f), 0.25f));   // orange
        rainbowColorSeq.Append(image.DOColor(Color.yellow, 0.25f));
        rainbowColorSeq.Append(image.DOColor(Color.green, 0.25f));
        rainbowColorSeq.Append(image.DOColor(Color.cyan, 0.25f));
        rainbowColorSeq.Append(image.DOColor(Color.blue, 0.25f));
        rainbowColorSeq.Append(image.DOColor(new Color(0.7f, 0f, 1f), 0.25f));   // violet
        rainbowColorSeq.SetLoops(-1, LoopType.Restart);

        rainbowScaleTween = transform.DOScale(1.06f, 0.4f).SetLoops(-1, LoopType.Yoyo);
    }

    void StopRainbowAnim()
    {
        isRainbow = false;

        StopRainbowAnimCore();

        image.DOColor(Color.white, 0.2f);
        transform.DOScale(1f, 0.2f);
    }

    void StopRainbowAnimCore()
    {
        if (rainbowColorSeq != null && rainbowColorSeq.IsActive())
        {
            rainbowColorSeq.Kill();
            rainbowColorSeq = null;
        }

        if (rainbowScaleTween != null && rainbowScaleTween.IsActive())
        {
            rainbowScaleTween.Kill();
            rainbowScaleTween = null;
        }
    }

    void PlaySuperSpinAnim()
    {
        StopSuperSpinAnimCore();
        StopIdleSway();

        superRotateTween = image.rectTransform
            .DOLocalRotate(new Vector3(0f, 0f, -360f), 0.8f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void StopSuperSpinAnim()
    {
        isSuperSpin = false;
        StopSuperSpinAnimCore();

        image.rectTransform
            .DOLocalRotate(Vector3.zero, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                StartIdleSway();
            });
    }

    void StopSuperSpinAnimCore()
    {
        if (superRotateTween != null && superRotateTween.IsActive())
        {
            superRotateTween.Kill();
            superRotateTween = null;
        }
    }

    void OnDisable()
    {
        transform.DOKill();
        image.DOKill();
        image.rectTransform.DOKill();

        StopRainbowAnimCore();
        StopSuperSpinAnimCore();
    }
}