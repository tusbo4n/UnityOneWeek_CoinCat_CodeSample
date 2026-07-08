using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//タイトル画面の文字やキャラクターをクリックすると音が鳴る隠し機能
public class TitleSoundTrigger : MonoBehaviour
{
    [SerializeField] CommonData.SoundId soundId = CommonData.SoundId.ConnectSound_00;

    private void Awake()
    {
        var trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => PlaySound());
        trigger.triggers.Add(entry);
    }

    public void PlaySound()
    {
        SoundManager.PlaySound(soundId);
    }
}
