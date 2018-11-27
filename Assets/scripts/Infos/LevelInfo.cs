using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LevelName", menuName = "Infos/LevelInfo", order = 1)]
public class LevelInfo : ScriptableObject
{
    public string LevelName;
    public string SceneName;
    public Sprite LevelMenuSprite;

    public void LoadLevel()
    {
        SceneManager.LoadScene(SceneName);
    }
}