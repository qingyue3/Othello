using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameModes
{
    // Define values for Game Mode enum: None, MultiPlayer, SinglePlayerEasy, SinglePlayerMedium, SinglePlayerHard and PC Vs PC all modes
    None,
    MultiPlayer,
    SinglePlayerEasy,
    SinglePlayerMedium,
    SinglePlayerHard,
    Test,
}

public class DifficultyMode : MonoBehaviour
{
    public static GameModes gameMode = GameModes.None;

    
    public void OnMultiPlayerClicked()
    {
        gameMode = GameModes.MultiPlayer;
        SceneManager.LoadScene(2);
    }

    public void OnEasyModeClicked()
    {      
        gameMode = GameModes.SinglePlayerEasy;
        SceneManager.LoadScene(2);
    }

    public void OnMediumModeClicked()
    {
        gameMode = GameModes.SinglePlayerMedium;
        SceneManager.LoadScene(2);
    }

    public void OnHardModeClicked()
    {
        gameMode = GameModes.SinglePlayerHard;
        SceneManager.LoadScene(2);
    }

    public void OnTestClicked()
    {
        gameMode = GameModes.Test;
        SceneManager.LoadScene(2);
    }

    public void OnBackClicked() 
    {
        SceneManager.LoadScene(0);
    }



}
