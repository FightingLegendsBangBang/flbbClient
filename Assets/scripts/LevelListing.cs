using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using UnityEngine.UI;

public class LevelListing : MonoBehaviour
{
    private int levelId;

    public void Init(int levelId)
    {
        this.levelId = levelId;

        GetComponent<Image>().sprite = InfoManager.Instance.Levels[levelId].LevelMenuSprite;
    }

    public void ButtonClick()
    {
        Debug.Log(levelId);

        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort) 301);
        writer.Put(levelId);
        NetworkManager.Instance.Send(writer, DeliveryMethod.ReliableOrdered);
    }
}