﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Player info
    public int cash;
    public string playerName;
    public int numProperties;
    public Text playerInfoText;
    public int playerIndexText;
    public List<GameObject> ownedTiles;

    private int timeInJail;
    private int playerIndex;
    private int playerLocationIndex;

    // State 
    private bool myTurn;
    private enum State { Active, Rolling, Waiting };
    State state;

    // Movement
    public float moveTime;
    private bool stillMoving;

    // References
    private GameManagerScript gm;
    private DieScript die;
    private BoardScript board;



    // Init
    private void Start()
    {
        // setting initial state
        state = State.Waiting;
        // reference to die
        die = DieScript.instance();
        //reference to board
        board = BoardScript.instance();
        // reference to the game manager
        gm = GameManagerScript.instance();

        // inital assigns
        timeInJail = 0;
        playerLocationIndex = 0;
        //playerInfoText.text = "init";
        cash = 1500;
        numProperties = 0;
        stillMoving = false;
    }

    // Rolls die, if double then state == active,
    // else state == waiting and return control
    // to gamemanger
    public void Roll()
    {
        state = State.Rolling;
        int roll;

        // Roll and move
        while (state == State.Rolling)
        {
            roll = die.RollDie();
            if (!die.wasLastDouble())
                state = State.Active;

            // Update player
            MovePlayer(roll);
        }

        EndTurn();
    }

    // Moves player to target destination
    private void MovePlayer(int rolled)
    {
        // finds how many tiles on board
        int len = gm.GetComponent<GameManagerScript>().tilesList.Length;

        // Move tile by tile
        StartCoroutine(SmoothMovement(rolled, len));
    }

    // Moves player to each tile smoothly
    private IEnumerator SmoothMovement(int distToMove, int numTiles)
    {
        // Wait until previous roll is finished before moving (multiple instances)
        while (stillMoving)
            yield return new WaitForSeconds(0.1f);
   
        stillMoving = true;

        // Move player tile by tile
        for (int i = 1; i <= distToMove; i++)
        {
            // Find target position
            Vector3 targetPos = gm.GetComponent<GameManagerScript>().tilesList[(playerLocationIndex + i) % numTiles].transform.position;

            StartCoroutine(MoveOverSeconds(targetPos, moveTime));
            yield return new WaitForSeconds(moveTime + 0.1f);
        }

        // Update player's location after move
        playerLocationIndex = (playerLocationIndex + distToMove) % numTiles;
        stillMoving = false;
    }

    // Moves a player uniformly from one location to another
    private IEnumerator MoveOverSeconds(Vector3 end, float  desiredTime)
    {
        float elapsedTime = 0;
        Vector3 startPos = transform.position;

        // Move player by time
        while (elapsedTime < desiredTime)
        {
            transform.position = Vector3.Lerp(startPos, end, (elapsedTime / desiredTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = end;
    }

    // Starts player turn
    public void StartTurn()
    {
        state = State.Active;
        //button.SetActive(true);
        myTurn = true;
    }

    // Ends player turn
    private void EndTurn()
    {
        state = State.Waiting;
        //button.SetActive(false);
        myTurn = false;
        gm.NextTurn();
    }

    // Request trade from another player, return true if accepted, false otherwise
    public bool RequestTrade(int playerReciever)
    {
        //person = GetComponent<PlayerPrefs>;
        //return person.RecieveTrade()

        //trade button disabled when not turn by gamemanager

        return false;

    }

    public bool RecieveTrade(int playerRequester)
    {
        //creates box in front of the parameter
        //yes / no
        //returns true/false
        //if true open trade method

        return false;
    }

    // returns money from player requested
    public void PayMe()
    {
        // Note: will have to check for if the tile belongs to the interface
        // IBuyTileScript, otherwise the tile will not have the GetRent function
        // as it is a method of the interface, not TileScript.

        /*
        // Looks for any money to be gained (penality if can't)
        int propLocationIndex = gm.GetPlayerList()[gm.GetLastPlayerIndex()].GetComponent<PlayerScript>().playerLocationIndex;
        TileScript lastPlayerLocation = board.GetTile(propLocationIndex).GetComponent<TileScript>();
        if (lastPlayerLocation.GetOwner().GetComponent<PlayerScript>().GetIndex() == this.playerIndex)
        {
            //gets money
            cash += lastPlayerLocation.GetRent();
            //make other player pay
        }
        else
        {
            //placeholder for penalty for pressing pay me when you shouldn't
            cash -= 50;
        }
        */
    }

    public int GetIndex()
    {
        return playerIndex;
    }

    public int GetCash()
    {
        return this.cash;
    }

    // SET the cash to a value
    public void SetCash(int cash)
    {
        this.cash = cash;
    }

    // ADD a value to the cash amount
    public void AddCash(int cash)
    {
        this.cash += cash;
    }

    void Update()
    {
        //UpdateText();
    }

    public void UpdateText()
    {
        playerInfoText.text = playerName + " cash: $" + cash;
        playerInfoText.text += "\n";
        playerInfoText.text += "properties: " + numProperties;
        if (ownedTiles.Count != 0)
        {
            playerInfoText.text += "\nOwned Properties:\n";
            foreach(GameObject tile in ownedTiles)
            {
                playerInfoText.text += " " + tile.name + "\n";
            }
        }
    }
    
    public void SetPlayerIndex(int index)
    {
        this.playerIndex = index;
    }
}
