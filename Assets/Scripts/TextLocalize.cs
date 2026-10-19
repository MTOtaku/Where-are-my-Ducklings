using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TextLocalize : MonoBehaviour
{
    [SerializeField] private LocalizedString text;
    [SerializeField] private TextMeshProUGUI objectText;

    private void OnEnable()
    {
        ChangedLocalization();
        GameEvents.Localization.OnChangeLocalization += ChangedLocalization;
    } 

    private void OnDisable()
    {
        GameEvents.Localization.OnChangeLocalization -= ChangedLocalization;
    }

    protected virtual void ChangedLocalization()
    {
        objectText.text = text.GetLocalizedString();
    }
}
