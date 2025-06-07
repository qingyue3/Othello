using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heuristics  
{
    private GameState state;
    public int currentPlayerValue;
    public int opponentValue;
    private int staticBoardScore;
    private int currentPlayerCoins;
    private int opponentCoins;
    private int coin_parity_score;
    private int currentPlayerMoves;
    private int opponentMoves;
    private int mobility_score;
    private int currentPlayerCorners;
    private int opponentCorners;
    private int corner_score;
    //private int currentPlayerStability;
    //private int opponentStability;
    //private int stability_score;
    public Heuristics()
    {
        this.state = new GameState();
        this.currentPlayerValue = 0;
        this.opponentValue = 0;
        this.staticBoardScore = 0;
        this.currentPlayerCoins = 0;
        this.opponentCoins = 0;
        this.coin_parity_score = 0;
        this.currentPlayerMoves = 0;
        this.opponentMoves = 0;
        this.mobility_score = 0;
        this.currentPlayerCorners = 0;
        this.opponentCorners = 0;
        this.corner_score = 0;
        //this.currentPlayerStability = 0;
        //this.opponentStability = 0;
        //this.stability_score = 0;
    }

    public int StaticBoard(GameState state)
    {
        // 定义每个格子的稳定性分数
        int[,] boardScores = new int[,] {
    {100, -20, 10,  5,  5, 10, -20, 100},
    {-20, -50, -2, -2, -2, -2, -50, -20},
    { 10,  -2,  0,  1,  1,  0,  -2,  10},
    {  5,  -2,  1,  2,  2,  1,  -2,   5},
    {  5,  -2,  1,  2,  2,  1,  -2,   5},
    { 10,  -2,  0,  1,  1,  0,  -2,  10},
    {-20, -50, -2, -2, -2, -2, -50, -20},
    {100, -20, 10,  5,  5, 10, -20, 100}
        };
        //遍历每个位置
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (state.Board[row, col] == state.CurrentPlayer)
                {
                    this.currentPlayerValue += boardScores[row, col];
                }
                else if (state.Board[row, col] == state.CurrentPlayer.Opponent())
                {
                    this.opponentValue += boardScores[row, col];
                }
            }
        }

        // 计算 staticBoardScore  
        //Debug.Log("Current Player Value: " + this.currentPlayerValue);
        //Debug.Log("Opponent Value: " + this.opponentValue);
        //会输出遍历的每一个可能的 staticBoardScore
        try
        {
            this.staticBoardScore = (100 * (this.currentPlayerValue - this.opponentValue)) / (this.currentPlayerValue + this.opponentValue);
        }
        catch (DivideByZeroException)
        {
            this.staticBoardScore = 0;
        }
       
        return this.staticBoardScore;               
    }

    // Calculate the coin parity score for the given game state
    public int CoinParity(GameState state)
    {
        // Calculate the number of coins for the current player and the opponent
        this.currentPlayerCoins = state.DiscCount[state.CurrentPlayer];
        this.opponentCoins = state.DiscCount[state.CurrentPlayer.Opponent()];

        // Calculate the coin parity score using the formula
        this.coin_parity_score = (100 * (this.currentPlayerCoins - this.opponentCoins)) / (this.currentPlayerCoins + this.opponentCoins);

        return this.coin_parity_score;
    }

    // Calculate the mobility score for the given game state
    public int Mobility(GameState state)
    {
        // Find the legal moves for the current player
        Dictionary<Position, List<Position>> legalMoves = state.FindLegalMoves(state.CurrentPlayer);

        // Calculate the number of legal moves for the current player
        currentPlayerMoves = legalMoves.Count;

        // Calculate the number of legal moves for the opponent
        Player opponent = state.CurrentPlayer.Opponent();
        Dictionary<Position, List<Position>> opponentLegalMoves = state.FindLegalMoves(opponent);
        opponentMoves = opponentLegalMoves.Count;

        // Calculate the mobility score using the formula
        this.mobility_score = (100 * (this.currentPlayerMoves - this.opponentMoves)) / (this.currentPlayerMoves + this.opponentMoves);

        return this.mobility_score;
    }

    // Calculate the corner score for the given game state
    public int Corner(GameState state)
    {
        // Define the corner positions
        Position[] cornerPositions = { new Position(0, 0), new Position(0, 7), new Position(7, 0), new Position(7, 7) };

        // Calculate the number of corners for the current player and the opponent
        foreach (var position in cornerPositions)
        {
            if (state.Board[position.Row, position.Column] == state.CurrentPlayer)
                this.currentPlayerCorners++;
            else if (state.Board[position.Row, position.Column] == state.CurrentPlayer.Opponent())
                this.opponentCorners++;
        }

        try
        {
            // Calculate the corner score using the formula
            this.corner_score = (100 * (this.currentPlayerCorners - this.opponentCorners)) / (this.currentPlayerCorners + this.opponentCorners);

        }
        catch (DivideByZeroException)
        {
            // Handle the division by zero exception
            // Return a default value or handle the situation accordingly
            this.corner_score = 0;
        }

        return this.corner_score;
    }

}
