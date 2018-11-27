using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListing : MonoBehaviour
{
    public Image charImage;
    private int charId;

    public void Init(int charId)
    {
        charImage.sprite = InfoManager.Instance.Characters[charId].CharacterSprite;
        this.charId = charId;
    }

    public void ClickButton()
    {
        int playerId = 0;

        foreach (var player in NetworkManager.Instance.Players)
        {
            if (player.Value.networkId == NetworkManager.Instance.MyNetworkId)
                playerId = player.Key;
        }

        NetworkManager.Instance.Players[playerId].characterId = charId;
        NetworkManager.Instance.Players[playerId].SendUpdatedData();
    }
}