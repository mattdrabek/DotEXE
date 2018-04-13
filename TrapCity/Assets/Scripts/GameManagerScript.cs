using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quobject.SocketIoClientDotNet.Client;

public class GameManagerScript : MonoBehaviour
{

    // author: Joe Peaden

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

    private const int NUM_OF_CARDS = 40;
    private Queue<GameObject> deck;
    private GameObject[] playerList;
    public GameObject[] tilesList;
    private int currentPlayerIndex;  // Index of player in playerList
    private int lastPlayerIndex;
    private int numOfDoubles;
    private int numOfPlayers = 6;

    public Socket socket = IO.Socket("http://localhost:3000");


    private void Start()
    {
        // gets all "Card" objects in scene as an array, shuffle, and convert to queue
        GameObject[] deckArray = GameObject.FindGameObjectsWithTag("Card");
        deck = new Queue<GameObject>(Shuffle(deckArray));

        // ===============> Get number of player from title screen and assign to numOfPlayers, let spawner create?      <============= Jared 4/4/18
        playerList = GameObject.FindGameObjectsWithTag("Player");
        currentPlayerIndex = FirstPlayer();

        // add events from the server
        socket.On("new game", (gameData) => 
        {
            // save game data

            // save user data for all players

        });

        StartGame();
    }

    // used to start the game
    //
    // needs to deal money and such ?
    //
    // NOTE:
    // should all player finding and such be done in this method, since players
    // may come into the game or leave in the lobby section before the game
    // actually starts?
    public void StartGame()
    {
        // Assign players their index within playerList (so player objects have that info)
        int index = currentPlayerIndex;
        for (int i = 1; i < numOfPlayers + 1; i++)
            playerList[index].GetComponent<PlayerScript>().SetPlayerIndex(i);

        playerList[currentPlayerIndex].GetComponent<PlayerScript>().StartTurn();
    }

    // starts next player's turn
    public void NextTurn()
    {
        lastPlayerIndex = currentPlayerIndex;
        currentPlayerIndex = (currentPlayerIndex + 1) % playerList.Length;
        numOfDoubles = 0;
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

    public void PlayerRoll()
    {
        playerList[currentPlayerIndex].GetComponent<PlayerScript>().Roll();
    }

    // Returns first player index, calculated randomly for now.
    public int FirstPlayer()
    {
        System.Random num = new System.Random();
        return num.Next(numOfPlayers - 1);
    }

    // Returns top card in deck
    public GameObject GetCard()
    {
        if (deck.Count == 0)
        {
            print("Deck is empty");
            return null;
        }
        return deck.Dequeue();
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

    public int GetNumPlayers()
    {
        return numOfPlayers;
    }

    public int GetNumTiles()
    {
        return tilesList.Length;
    }


}