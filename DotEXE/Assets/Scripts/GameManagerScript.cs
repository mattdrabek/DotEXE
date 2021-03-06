// Joe and Jared
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    // The following block is singleton pattern, basically
    // it should give an access point to it from all gameobjects by calling
    // GameManagerScript.instance();
    // see http://gameprogrammingpatterns.com/singleton.html for details
    private static GameManagerScript instance_;
    private GameManagerScript() { }
    public static GameManagerScript instance()
    {
        if (instance_ == null)
            instance_ = GameObject.FindObjectOfType<GameManagerScript>();
        return instance_;
    }
    ////////////////////////////////

    public GameObject emptyPlayer;      // Player object to spawn on game start
    public Sprite[] avatarSprites;      // Array of avatar images
    public GameObject[] tilesList;      // Tiles manually placed in Array
    public GameObject[] playerInfoButtons;
    public Text currentPlayer;

    public Button upgradeButton;        // Needs link to button script
    public Button emptyTile;            // Base tile button before hooked up
    public GameObject tileButtonParent; // Where tile buttons get placed
    public Text console;                // Where tiles get their methods from
    public GameObject tradeWindow;
    public GameObject select_player_plz;
    public GameObject info_player;

    private bool hasBeenPaid;
    private Button[] allTileButtons;    // Every tile button in order
    private Queue<GameObject> deck;     // Where all cards will be placed
    private GameObject[] playerList;    // Array of created player objects
    private int currentPlayerIndex;     // Index of player in playerList
    private int lastPlayerIndex;
    private int numOfDoubles;
    private int numOfPlayers;
    private const int MAX_PLAYERS = 6;

    /*            GAME LOAD              */

    private void Start()
    {
        if(GameObject.Find("NumPlayers").GetComponent<NumPlayersScript>().GetNumPlayers() != 0)
        {
            numOfPlayers = GameObject.Find("NumPlayers").GetComponent<NumPlayersScript>().GetNumPlayers();
        }
        else
        {
            numOfPlayers = 6;
        }
        // Gets all "Card" objects in scene as an array, shuffle, and convert to queue
        GameObject[] deckArray = GameObject.FindGameObjectsWithTag("Card");
        deck = new Queue<GameObject>(deckArray);

        CreatePlayers();
        SpawnTileButtons();
        AdjustPlayerInfoButtons();

        // Starts the game
        SetCurrentPlayerText(playerList[currentPlayerIndex].GetComponent<PlayerScript>().GetName());
        playerList[currentPlayerIndex].GetComponent<PlayerScript>().StartTurn();
    }

    // shuffles the original deck of cards
    private GameObject[] Shuffle(GameObject[] cards)
    {
        // throw all cards in bag
        List<GameObject> bag = new List<GameObject>();
        foreach (GameObject card in cards)
        {
            bag.Add(card);
        }
        // randomly pick from list and put back in array
        System.Random rand = new System.Random();
        int index = 0;
        foreach (GameObject card in bag)
        {
            // pick random card from bag
            int selectedCard = rand.Next(cards.Length);
            // add selected back to array
            cards[index] = bag[selectedCard];
            // remove card from bag
            bag.Remove(cards[index]);
            index++;
        }
        return cards;
    }

    // Spawn players on start up (Based off of numOfPlayers)
    private void CreatePlayers()
    {
        playerList = new GameObject[numOfPlayers];

        // Create avatars and connect them to player objects, then place on GO
        GameObject temp;
        for (int i = 0; i < numOfPlayers; i++)
        {
            temp = Instantiate(emptyPlayer);
            temp.GetComponent<SpriteRenderer>().sprite = avatarSprites[i % avatarSprites.Length];
            temp.GetComponent<Transform>().SetPositionAndRotation(tilesList[0].GetComponent<Transform>().position, new Quaternion(0, 0, 0, 0));
            temp.GetComponent<PlayerScript>().SetName("Player" + (i+1));
            temp.GetComponent<PlayerScript>().SetPlayerIndex(i);
            playerList[i] = temp;
        }

        // Assign first player
        currentPlayerIndex = FirstPlayer();
    }

    // Returns first player index, calculated randomly for now.
    private int FirstPlayer()
    {
        System.Random num = new System.Random();
        return num.Next(numOfPlayers - 1);
    }

    // Initializes all tile buttons
    private void SpawnTileButtons()
    {
        Button[] temp = new Button[tilesList.Length];

        // For every title on board create a button
        for (int i = 0; i < tilesList.Length; i++)
        {
            Button spawn = Instantiate(emptyTile);

            // Get location of tile to be over
            spawn.transform.position = tilesList[i].transform.position;

            // Change size and rotation to that of tile
            float w = tilesList[i].transform.GetComponent<RectTransform>().rect.width * 100;
            float h = tilesList[i].transform.GetComponent<RectTransform>().rect.height * 100;
            Quaternion r = tilesList[i].transform.GetComponent<RectTransform>().rotation;
            spawn.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            spawn.GetComponent<RectTransform>().rotation = r;
            spawn.GetComponent<RectTransform>().localScale = new Vector3(.01f, .01f, .01f);

            // Set up onClick
            spawn.onClick.AddListener(() => { spawn.GetComponent<TileButtonScript>().WriteTileToConsole(); });
            spawn.GetComponent<TileButtonScript>().SetTileNum(i);
            spawn.GetComponent<TileButtonScript>().SetTextObject(console);

            if (tilesList[i].tag == "PropertyTile")
                spawn.GetComponentInChildren<Text>().text = "0";

            // Set parent to linked object and place in array
            spawn.transform.SetParent(tileButtonParent.transform);
            temp[i] = spawn;
        }

        allTileButtons = temp;
    }

    // Only see how many buttons for players (assume's player is last player)
    private void AdjustPlayerInfoButtons()
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (i >= numOfPlayers)
            {
                playerInfoButtons[i].SetActive(false);
                continue;
            }

            playerInfoButtons[i].GetComponent<PlayerInfoButtonScript>().SetPlayer(playerList[i]);

        }
    }




    /*             GAME PLAYING           */

    // Starts next player's turn
    public void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playerList.Length;
        hasBeenPaid = false;
        numOfDoubles = 0;
        playerList[currentPlayerIndex].GetComponent<PlayerScript>().StartTurn();
    }

    // Method that RollDiceButton should call
    public void PlayerRoll()
    {
        playerList[currentPlayerIndex].GetComponent<PlayerScript>().Roll();
    }

    // Returns top card in deck and then places card at bottom
    public GameObject GetCard()
    {
        if (deck.Count == 0)
        {
            print("Deck is empty");
            return null;
        }

        GameObject card = deck.Dequeue();
        deck.Enqueue(card);
        return card;
    }

    // To return card to bottom of deck
    public void AddCard(GameObject card)
    {
        deck.Enqueue(card);
    }

    // increment numOfDoubles
    public void IncDouble()
    {
        numOfDoubles++;
    }

    // Handling pay me logic
    public void PayMe()
    {
        if (hasBeenPaid == true)
        {
            InfoScript.instance().Displayer("Player has already been charged.");
            return;
        }

        // If last player to go has landed on an owned property
        int lastPlayerLoc = playerList[lastPlayerIndex].GetComponent<PlayerScript>().GetLocIndex();

        // If tile doesn't implement IBuyTile, avoid error
        bool isOwned = false;
        try
        {
            isOwned = tilesList[lastPlayerLoc].GetComponent<IBuyTile>().IsOwned();
        }
        catch (Exception e)
        {
            InfoScript.instance().Displayer("This tile cannot be owned!");
        }

        // Pay money!
        if (isOwned)
        {
            tilesList[lastPlayerLoc].GetComponent<IBuyTile>().PayPlayer(playerList[lastPlayerIndex]);
            hasBeenPaid = true;
        }
    }

    // enable the trade window if a player has been selected to trade with
    public void EnableTradeWindow()
    {
        // check if player has been selected in playerinfo screen
        if(info_player != null)
            tradeWindow.SetActive(true);
        else
            select_player_plz.SetActive(true);
    }




    /*        GETTERS AND SETTERS       */

    public int GetNumDoubles()
    {
        return numOfDoubles;
    }

    public GameObject[] GetPlayerList()
    {
        return playerList;
    }

    public int GetLastPlayerIndex()
    {
        return lastPlayerIndex;
    }

    public void SetLastPlayerIndex(int lpi)
    {
        lastPlayerIndex = lpi;
    }

    public int GetNumPlayers()
    {
        return numOfPlayers;
    }

    public int GetNumTiles()
    {
        return tilesList.Length;
    }

    public GameObject GetCurrentPlayer()
    {
        return playerList[currentPlayerIndex];
    }

    public GameObject GetTile(int index)
    {
        return tilesList[index];
    }

    public Button GetTileButton(int index)
    {
        return allTileButtons[index];
    }

    public void SetUpgradeScript(int index)
    {
        upgradeButton.GetComponent<UpgradeScript>().SetIndex(index);
    }

    public void SetCurrentPlayerText(string s)
    {
        currentPlayer.text = "Current Player:\n" + s;
    }
}
