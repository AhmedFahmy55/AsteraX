using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenPanel : MonoBehaviour {

    public void StartGame()
    {
        AsteraX.StartGame();
    }

    public void DeleteSaveFile()
    {
        SaveGameManager.DeleteSave();
    }

}
