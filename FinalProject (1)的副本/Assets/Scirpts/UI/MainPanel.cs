using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    private Button btn_Start;
    private Button btn_Shop;
    private Button btn_Rank;
    private Button btn_Sound;
    private Button btn_Reset;
    private ManagerVars vars;

    private void Awake()
    {
        vars = ManagerVars.GetManagerVars();
        EventCenter.AddListener(EventDefine.ShowMainPanel, Show);
        EventCenter.AddListener<int>(EventDefine.ChangeSkin, ChangeSkin);
        Init();
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventDefine.ShowMainPanel, Show);
        EventCenter.RemoveListener<int>(EventDefine.ChangeSkin, ChangeSkin);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Start()
    {
        if (GameData.IsAgainGame)
        {
            EventCenter.Broadcast(EventDefine.ShowGamePanel);
            gameObject.SetActive(false);
        }

        Sound();
        ChangeSkin(GameManager.Instance.GetCurrentSelectedSkin());
    }
    private void ChangeSkin(int skinIndex)
    {
        btn_Shop.transform.GetChild(0).GetComponent<Image>().sprite =
            vars.skinSpriteList[skinIndex];
    }

    //initialize and get the buttons
    private void Init()
    {
        btn_Start = transform.Find("btn_Start").GetComponent<Button>();
        btn_Start.onClick.AddListener(OnStartButtonClick);
        btn_Shop = transform.Find("Btns/btn_Shop").GetComponent<Button>();
        btn_Shop.onClick.AddListener(OnShopButtonClick);
        btn_Rank = transform.Find("Btns/btn_Rank").GetComponent<Button>();
        btn_Rank.onClick.AddListener(OnRankButtonClick);
        btn_Sound = transform.Find("Btns/btn_Sound").GetComponent<Button>();
        btn_Sound.onClick.AddListener(OnSoundButtonClick);
        btn_Reset = transform.Find("Btns/btn_Reset").GetComponent<Button>();
        btn_Reset.onClick.AddListener(OnResetButtonClick);
    }

    //Once the button be clicked, invoked by onClick
    private void OnStartButtonClick()
    {
        EventCenter.Broadcast(EventDefine.PlayClikAudio);
        GameManager.Instance.IsGameStarted = true;
        EventCenter.Broadcast(EventDefine.ShowGamePanel);
        gameObject.SetActive(false);
    }

    private void OnShopButtonClick()
    {
        EventCenter.Broadcast(EventDefine.PlayClikAudio);
        EventCenter.Broadcast(EventDefine.ShowShopPanel);
        gameObject.SetActive(false);
    }
    private void OnRankButtonClick()
    {
        EventCenter.Broadcast(EventDefine.PlayClikAudio);
        EventCenter.Broadcast(EventDefine.ShowRankPanel);
    }
    private void OnSoundButtonClick()
    {
        EventCenter.Broadcast(EventDefine.PlayClikAudio);

        GameManager.Instance.SetIsMusicOn(!GameManager.Instance.GetIsMusicOn());
        Sound();
    }
    private void Sound()
    {
        if (GameManager.Instance.GetIsMusicOn())
        {
            btn_Sound.transform.GetChild(0).GetComponent<Image>().sprite = vars.musicOn;
        }
        else
        {
            btn_Sound.transform.GetChild(0).GetComponent<Image>().sprite = vars.musicOff;
        }
        EventCenter.Broadcast(EventDefine.IsMusicOn, GameManager.Instance.GetIsMusicOn());
    }
    private void OnResetButtonClick()
    {
        EventCenter.Broadcast(EventDefine.PlayClikAudio);
        EventCenter.Broadcast(EventDefine.ShowResetPanel);
    }
}
