using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] private LocalizedString text;
    [SerializeField] private TextMeshProUGUI objectText;

    [SerializeField] private GenericButton mainMenuButton;
    private const string MAIN_MENU_SCENE_NAME = "Title_Screen";
    void Start()
    {
        ShowTimeResult();
        mainMenuButton.Initialize(ReturnMainMenu);
    }
    
    private void ReturnMainMenu()
    {
        Utils.LoadScene(MAIN_MENU_SCENE_NAME);
    }
    private void ShowTimeResult()
    {
        var timeResult = GameManager.Instance.LevelCurrentTime;
        var minutes = Mathf.FloorToInt(timeResult / 60.0f);
        var seconds = Mathf.FloorToInt(timeResult - (minutes * 60));

        var minutesLocalized = text["levelMinuteTime"] as IntVariable;
        minutesLocalized.Value = minutes;

        var secondsLocalized = text["levelSecondsTime"] as IntVariable;
        secondsLocalized.Value = seconds;

        objectText.text = text.GetLocalizedString();
    }
}
