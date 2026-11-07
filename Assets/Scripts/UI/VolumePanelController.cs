using System.Collections;
using FMOD.Studio;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class VolumePanelController : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string VCA_MASTER_PATH = "vca:/geral";
    private string VCA_MUSIC_PATH = "vca:/musicas";

    private string VCA_SFX_PATH = "vca:/efeitos";

    private VCA masterVCA;
    private VCA sfxVCA;

    private VCA musicVCA;


    void Start()
    {
        masterVCA = Utils.GetFmodVCA(VCA_MASTER_PATH);
        musicVCA = Utils.GetFmodVCA(VCA_MUSIC_PATH);
        sfxVCA = Utils.GetFmodVCA(VCA_SFX_PATH);
        masterSlider.value = Utils.GetVolumeFromVCA(masterVCA);
        musicSlider.value = Utils.GetVolumeFromVCA(musicVCA);
        sfxSlider.value = Utils.GetVolumeFromVCA(sfxVCA);
        

    }
    public void ChangeVolumeMaster(float value)
    {
        Utils.SetVolumeInVCA(value, masterVCA);
    }

    public void ChangeVolumeMusic(float value)
    {
        Utils.SetVolumeInVCA(value, musicVCA);
    }

    public void ChangeVolumeSFX(float value)
    {
        Utils.SetVolumeInVCA(value, sfxVCA);
    }
}
