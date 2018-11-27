using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviour
{
    public TextMeshProUGUI textObject;
    public Image charImage;
    private int playerId;
    private int networkId;

    public void Init(int playerId, int networkId, string playerName, int charId, Color color)
    {
        this.playerId = playerId;
        this.networkId = networkId;
        textObject.text = playerName;
        GetComponent<Image>().color = color;
        charImage.sprite = InfoManager.Instance.Characters[charId].CharacterSprite;
    }

    public void ClickButton()
    {
        if (NetworkManager.Instance.MyNetworkId == networkId)
        {
            NetworkManager.Instance.Players[playerId].playerColor = new Color(Random.value, Random.value, Random.value);
            NetworkManager.Instance.Players[playerId].SendUpdatedData();
        }
    }
}