using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    internal bool NeedUpdate = false;
    private NetworkManager nwm;
    public static LobbyManager Instance;
    public GameObject playerListing;
    public GameObject characterListing;
    public GameObject levelListing;
    public Transform playerList;
    public Transform characterList;
    public Transform LevelList;
    public GameObject LobbyMenu;
    public GameObject LevelMenu;
    public Button ButtonStartGame;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        nwm = NetworkManager.Instance;
        foreach (Transform child in playerList)
            Destroy(child.gameObject);

        foreach (Transform child in characterList)
            Destroy(child.gameObject);

        foreach (Transform child in LevelList)
            Destroy(child.gameObject);

        for (int i = 1; i < InfoManager.Instance.Characters.Length; i++)
        {
            Instantiate(characterListing, characterList, false).GetComponent<CharacterListing>().Init(i);
        }

        for (int i = 0; i < InfoManager.Instance.Levels.Length; i++)
        {
            Instantiate(levelListing, LevelList, false).GetComponent<LevelListing>().Init(i);
        }

        ButtonStartGame.gameObject.SetActive(false);

        NeedUpdate = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (NeedUpdate)
        {
            foreach (Transform child in playerList)
            {
                Destroy(child.gameObject);
            }

            ButtonStartGame.interactable = true;
            ButtonStartGame.gameObject.SetActive(true);
            foreach (var player in nwm.Players)
            {
                Instantiate(playerListing, playerList, false).GetComponent<PlayerListing>()
                    .Init(player.Value.playerId, player.Value.networkId, player.Value.playerName,
                        player.Value.characterId, player.Value.playerColor);

                if (player.Value.characterId == 0)
                    ButtonStartGame.interactable = false;

                if (player.Value.networkId == nwm.MyNetworkId && !player.Value.isHost)
                    ButtonStartGame.gameObject.SetActive(false);
            }

            NeedUpdate = false;
        }
    }

    public void ButtonLevelBackClick()
    {
        LobbyMenu.transform.SetAsLastSibling();
    }

    public void ButtonStartGameClick()
    {
        LevelMenu.transform.SetAsLastSibling();
    }
}