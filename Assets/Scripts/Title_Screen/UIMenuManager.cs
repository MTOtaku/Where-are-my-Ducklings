using System.Collections.Generic;
using RockPaperPistol.UI.Elements;
using UnityEngine;

/*
 Aqui vai ser pra gerenciar tudo do Title Screen, 
 se for preciso adicionar algo novo, avise e coloque um comentario para ajudar a entender / não quebrar a logica
 */

public class UIMenuManager : MonoBehaviour {
    [Header("Gerenciador das telas")]
    [Tooltip("Linkar os Objetos de quais estes menus pertencem")]
    
    [Header("Paineis")]
    [SerializeField] private GameObject[] menuPanels;
    [SerializeField] private GameObject titleScreenPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [SerializeField] private GenericButton startButton;
    [SerializeField] private GenericButton optionsButton;
    [SerializeField] private GenericButton creditsButton;
    [SerializeField] private GenericButton quitButton;
    [SerializeField] private List<GenericButton> backsButtons;

    void Start()
    {
        startButton.Initialize(OpenLevelSelectPanel);
        creditsButton.Initialize(OpenSettingsPanel);
        optionsButton.Initialize(OpenCreditsPanel);
        quitButton.Initialize(QuitGame);

        foreach(var backButton in backsButtons)
        {
            backButton.Initialize(BackToTitleScreen);
        }
    }

    //Ativar somente o painel escolhido
    public void OpenPanel(GameObject selectedPanel){
        foreach (GameObject panel in menuPanels) {
            if (panel != null) {
                panel.SetActive(panel == selectedPanel);
            }
        }
    }
    
    public void BackToTitleScreen(){
        OpenPanel(titleScreenPanel);
    }
    
    public void OpenLevelSelectPanel(){
        OpenPanel(levelSelectPanel);
    }
 
    public void OpenSettingsPanel(){
        OpenPanel(settingsPanel);
    }
    
    public void OpenCreditsPanel(){
        OpenPanel(creditsPanel);
    }
    public void QuitGame(){
        Debug.Log("Saindo do jogo");
        Application.Quit();
    }
}
