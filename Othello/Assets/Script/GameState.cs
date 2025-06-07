
using System.Collections.Generic;
using UnityEditor;

public class GameState
{
    public const int Rows = 8;
    public const int Cols = 8;

    public Player[,] Board { get; set; }
    public Dictionary<Player, int> DiscCount { get; set; }
    public Player CurrentPlayer { get; set; }
    public bool GameOver { get; set; }
    public Player Winner { get; set; }
    public Dictionary<Position, List<Position>> LegalMoves { get; set; }

    public GameState()
    {
        Board = new Player[Rows, Cols];
        Board[3, 3] = Player.White;
        Board[3, 4] = Player.Black;
        Board[4, 3] = Player.Black;
        Board[4, 4] = Player.White;

        DiscCount = new Dictionary<Player, int>()
        {
            { Player.White, 2 },
            { Player.Black, 2 }
        };

        CurrentPlayer = Player.Black;
        LegalMoves = FindLegalMoves(CurrentPlayer);
    }

    public bool MakeMove(Position pos, out MoveInfo moveinfo)
    {
        if (!LegalMoves.ContainsKey(pos))
        {
            moveinfo = null;
            return false;
        }

        Player movePlayer = CurrentPlayer;
        List<Position> outflanked = LegalMoves[pos];

        Board[pos.Row, pos.Column] = movePlayer;
        FlipDiscs(outflanked);
        UpdateDiscCounts(movePlayer, outflanked.Count);
        Passturn();

        moveinfo = new MoveInfo { Player = movePlayer, Position = pos, Outflanked = outflanked };
        return true;
    }

    public IEnumerable<Position> OccupiedPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Board[r, c] != Player.None)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }

    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position pos in positions)
        {
            Board[pos.Row, pos.Column] = Board[pos.Row, pos.Column].Opponent();
        }
    }

    private void UpdateDiscCounts(Player movePlayer, int outflankedCount)
    {
        DiscCount[movePlayer] += outflankedCount + 1;
        DiscCount[movePlayer.Opponent()] -= outflankedCount;
    }

    private void ChangePlayer()
    {
        CurrentPlayer = CurrentPlayer.Opponent();
        LegalMoves = FindLegalMoves(CurrentPlayer);
    }

    private Player FindWinner()
    {
        if (DiscCount[Player.Black] > DiscCount[Player.White])
        {
            return Player.Black;
        }
        if (DiscCount[Player.Black] < DiscCount[Player.White])
        {
            return Player.White;
        }
        return Player.None;
    }

    private void Passturn()
    {
        ChangePlayer();
        if (LegalMoves.Count > 0)
        {
            return;
        }
        ChangePlayer();
        if (LegalMoves.Count == 0)
        {
            CurrentPlayer = Player.None;
            GameOver = true;
            Winner = FindWinner();
        }
    }

    private bool isInsideBoard(int r, int c)
    {
        return r >= 0 && c >= 0 && r < Rows && c < Cols;
    }

    private List<Position> OutflankedInDir(Position pos, Player player, int rDelta, int cDelta)
    {
        List<Position> outflanked = new List<Position>();
        int r = pos.Row + rDelta;
        int c = pos.Column + cDelta;
        //rDelta and cDelta are variables used to control the search direction, which will be discussed later.
        while (isInsideBoard(r, c) && Board[r, c] != Player.None)//empty position
        {
            if (Board[r, c] == player.Opponent())//flank opponent's discs
            {
                outflanked.Add(new Position(r, c));
                r += rDelta;
                c += cDelta;
            }
            else
            {
                return outflanked;
            }
        }
        return new List<Position>();
    }

    private List<Position> Outflanked(Position pos, Player player)
    {
        List<Position> outflanked = new List<Position>();
        for (int rDelta = -1; rDelta <= 1; rDelta++)
        {
            for (int cDelta = -1; cDelta <= 1; cDelta++)
            {
                if (rDelta == 0 && cDelta == 0)
                {
                    continue;
                }
                outflanked.AddRange(OutflankedInDir(pos, player, rDelta, cDelta));
            }
        }
        return outflanked;
    }

    private bool IsMoveLegal(Player player, Position pos, out List<Position> outflanked)
    {
        if (Board[pos.Row, pos.Column] != Player.None)
        {
            outflanked = null;
            return false;
        }
        outflanked = Outflanked(pos, player);
        return outflanked.Count > 0;
    }

    public Dictionary<Position, List<Position>> FindLegalMoves(Player player)
    {
        Dictionary<Position, List<Position>> legalMoves = new Dictionary<Position, List<Position>>();
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Position pos = new Position(r, c);
                if (IsMoveLegal(player, pos, out List<Position> outflanked))
                {
                    legalMoves[pos] = outflanked;
                }
            }
        }
        return legalMoves;
    }
}
