using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    public CharacterInfo[] Characters;
    public LevelInfo[] Levels;

    public static InfoManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
}