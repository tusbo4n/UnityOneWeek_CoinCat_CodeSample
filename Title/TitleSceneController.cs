using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TitleSceneController : SceneControllerBase
{
    [SerializeField] GameObject omakeText1;
    [SerializeField] GameObject omakeText2;

    private void Awake()
    {
        omakeText1.SetActive(GameSceneController.youWin);
        omakeText2.SetActive(GameSceneController.youWin);
    }

    protected override IEnumerator _Update(string input)
    {
        switch (input)
        {
            case "Start":
                SoundManager.PlaySound(CommonData.SoundId.Start);
                yield return MySceneManager.Instance.LoadSceneAsync(CommonData.SceneName.Game);
                break;
        }
        yield return true;
    }

    static bool playedBGM = false;  //WebGLでは画面をクリックするまでBGMを再生できず、それが原因で1回目のループが崩れてしまうのを防ぐ為に使用するフラグ
    private void Update()
    {
        if (!playedBGM && MyInput.Instance.GetKeyDown(MyInput.MyKeyCode.Submit))
        {
            OnClickScreen();
            playedBGM = true;
        }
    }

    void OnClickScreen()
    {
        //WebGLでは画面をクリックするまでBGMを再生できず、それが原因で1回目のループが崩れてしまうのを防ぐ処理
        SoundManager.PlayBgmWithLoopSetting(CommonData.BGM_ID.Main, 0, 0, 0);
    }
}
