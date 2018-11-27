using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Infos/CharacterInfo", order = 1)]
public class CharacterInfo : ScriptableObject
{
    public string CharacterName;
    public int CharacterPrefabId;
    public Sprite CharacterSprite;

    public GameObject GetCharacterPrefab()
    {
        return NetworkManager.Instance.networkPrefabs[CharacterPrefabId];
    }
}