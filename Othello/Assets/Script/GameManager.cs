using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask boardLayer;

    [SerializeField]
    private Disc discBlackUp;

    [SerializeField]
    private Disc discWhiteUp;

    [SerializeField]
    private GameObject highlightPrefab;

    [SerializeField]
    private UIManager uiManager;

    private Dictionary<Player, Disc> discPrefebs = new Dictionary<Player, Disc>();
    private GameState gameState = new GameState();
    private Disc[,] discs = new Disc[8, 8];
    private List<GameObject> highlights = new List<GameObject>();

    private GamePlayingAlgorithms game_algo = new GamePlayingAlgorithms();

    int turnEndFlag = 0;

    // Start is called before the first frame update
    void Start()
    {
        discPrefebs[Player.Black] = discBlackUp;
        discPrefebs[Player.White] = discWhiteUp;
        AddStartDiscs();
        ShowLegalMoves();
        uiManager.SetPlayerText(gameState.CurrentPlayer);
    }

    // Update is called once per frame
    private void Update()
    {
        if (DifficultyMode.gameMode == GameModes.MultiPlayer)
        {
           if (Input.GetMouseButtonDown(0))
           {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, boardLayer))
             {
                Vector3 impact = hitInfo.point;
                Position boardPos = SceneToBoardPos(impact);
                //print boardPos...
                OnBoardClicked(boardPos);
             }
           } 
        }
        else if (DifficultyMode.gameMode == GameModes.Test)
        {
            GamePlayPCvsPC();
        }
        else
        {
            GamePlayVsPC();
        }
        
    }

    private void ShowLegalMoves()
    {
        foreach (Position boardPos in gameState.LegalMoves.Keys)
        {
            Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.01f;
            GameObject highlight = Instantiate(highlightPrefab, scenePos, Quaternion.identity);
            highlights.Add(highlight);
        }
    }

    private void HideLegalMoves()
    {
        highlights.ForEach(Destroy);
        highlights.Clear();
    }

    private void OnBoardClicked(Position boardPos)
    {
        turnEndFlag = 2;
        if (gameState.MakeMove(boardPos, out MoveInfo moveinfo))
        {
            StartCoroutine(OnMoveMade(moveinfo));
        }
    }

    private IEnumerator OnMoveMade(MoveInfo moveinfo)
    {
        HideLegalMoves();
        yield return ShowMove(moveinfo);
        yield return ShowTurnOutcome(moveinfo);

        // 调用 StableBorad 方法并输出分数
        //Heuristics heuristics = new Heuristics();
        //int staticBoardScore = heuristics.StaticBoard(gameState);
        //Debug.Log($"Current Player Value: {heuristics.currentPlayerValue}");
        //Debug.Log($"Opponent Value: {heuristics.opponentValue}");


        // if is turn to white, Ai play
        if (gameState.CurrentPlayer == Player.White)
        {
            //StartCoroutine(ComputerWhiteMakeMove());
            turnEndFlag = 1;
        }
        if (gameState.CurrentPlayer == Player.Black)
        {
            //StartCoroutine(ComputerBlackMakeMove());
            turnEndFlag = 0;
        }
        ShowLegalMoves();
    }

    private Position SceneToBoardPos(Vector3 scenePos)
    {
        int col = (int)(scenePos.x - 0.25f);
        int row = (int)(scenePos.z - 0.25f);
        int newrow = 7 - row;
        //Debug.Log("col = " + col + "; row = " + row + "; x = " + scenePos.x + "; z = " + scenePos.z);

        return new Position(newrow, col);
    }

    private Vector3 BoardToScenePos(Position boardPos)
    {
        return new Vector3(boardPos.Column + 0.75f, 0, 7 - boardPos.Row + 0.75f);
    }

    private void SpawnDisc(Disc prefab, Position boardPos)
    {
        Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.1f;
        discs[boardPos.Row, boardPos.Column] = Instantiate(prefab, scenePos, Quaternion.identity);
    }

    private void AddStartDiscs()
    {
        foreach (Position boardPos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[boardPos.Row, boardPos.Column];
            SpawnDisc(discPrefebs[player], boardPos);
        }
    }

    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position boardPos in positions)
        {
            discs[boardPos.Row, boardPos.Column].Flip();
        }
    }

    private IEnumerator ShowMove(MoveInfo moveInfo)
    {
        SpawnDisc(discPrefebs[moveInfo.Player], moveInfo.Position);
        yield return new WaitForSeconds(0.33f);
        FlipDiscs(moveInfo.Outflanked);
        yield return new WaitForSeconds(0.83f);
    }

    /*// A Random Ai 和其他的AI
    private IEnumerator ComputerWhiteMakeMove()
    {
        yield return new WaitForSeconds(0.5f); // 

        var legalMoves = gameState.LegalMoves;
        if (legalMoves.Count > 0)
        {         
            // 使用Minimax算法来获取最佳移动
            MoveInfo bestMove = game_algo.MiniMax(gameState, 3, false);
            if (gameState.MakeMove(bestMove.Position, out MoveInfo moveInfo))
            {
                StartCoroutine(OnMoveMade(moveInfo));
            }
        }
    }

    //测试
    private IEnumerator ComputerBlackMakeMove()
    {
        yield return new WaitForSeconds(0.5f); // 

        List<Position> legalMoves = new List<Position>(this.gameState.LegalMoves.Keys);
        if (legalMoves.Count > 0)
        {     
            Position AIMove = GetRandomMove(legalMoves);
            if (gameState.MakeMove(AIMove, out MoveInfo moveInfo))
            {
                StartCoroutine(OnMoveMade(moveInfo));
            }
        }
    }*/
    // random move
    private Position GetRandomMove( List<Position> legalMoves)
    {
        int randomIndex = UnityEngine.Random.Range(0, legalMoves.Count);
        return legalMoves[randomIndex];
    }

    public IEnumerator ShowTurnSkipped(Player skippedPlayer)
    {
        uiManager.SetskippedText(skippedPlayer);
        yield return uiManager.AnimateTopText();
    }

    private IEnumerator ShowGameOver(Player winner)
    {
        uiManager.SetTopText("Neither Player Can Move!");
        yield return uiManager.AnimateTopText();
        yield return uiManager.ShowScoreText();
        yield return new WaitForSeconds(0.5f);

        yield return ShowCounting();
        uiManager.SetWinnerText(winner);
        yield return uiManager.ShowEndScreen();

    }

    public IEnumerator ShowTurnOutcome(MoveInfo moveInfo)
    {
        if (gameState.GameOver)
        {
            yield return ShowGameOver(gameState.Winner);
            yield break;
        }

        Player currentPlayer = gameState.CurrentPlayer;

        if (currentPlayer == moveInfo.Player)
        {
            yield return ShowTurnSkipped(currentPlayer.Opponent());
        }

        uiManager.SetPlayerText(currentPlayer);
    }

    private IEnumerator ShowCounting()
    {
        int black = 0, white = 0;

        foreach (Position pos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[pos.Row, pos.Column];

            if (player == Player.Black)
            {
                black++;
                uiManager.SetBlackScoreText(black);
            }
            else if (player == Player.White)
            {
                white++;
                uiManager.SetWhiteScoreText(white);
            }

            discs[pos.Row, pos.Column].Twitch();
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator RestartGame()
    {
        yield return uiManager.HideEndScreen();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void OnPlayerAgainClicked()
    {
        StartCoroutine(RestartGame());
    }

    public void OnRestartClicked()
    {
        SceneManager.LoadScene(2);
    }


    public void BackToMainMenu()
    {
        SceneManager.LoadScene(1);
    }

    private IEnumerator DelayInSeconds(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
    }

    void GamePlayVsPC()
    {
        if (Input.GetMouseButtonDown(0) && turnEndFlag == 0)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 impact = hitInfo.point;
                Position boardPos = SceneToBoardPos(impact);
                OnBoardClicked(boardPos);
            }
        }
        else if (turnEndFlag == 1)
        {
            StartCoroutine(DelayInSeconds(8));

            List<Position> legalPositionsPC = new List<Position>(this.gameState.LegalMoves.Keys);

            if (legalPositionsPC.Count == 1)
            {
                OnBoardClicked(legalPositionsPC[0]);
            }
            else if (DifficultyMode.gameMode == GameModes.SinglePlayerEasy)
            {
                Position AIMove = GetRandomMove(legalPositionsPC);
                OnBoardClicked(AIMove);
            }
            else if (DifficultyMode.gameMode == GameModes.SinglePlayerMedium)
            {
                MoveInfo tempMoveInfo = game_algo.AlphaBetaPruning(gameState, 3, false);
                OnBoardClicked(tempMoveInfo.Position);
            }
            else if (DifficultyMode.gameMode == GameModes.SinglePlayerHard)
            {
                //Stopwatch stopwatch = Stopwatch.StartNew(); // 开始计时
                MoveInfo tempMoveInfo = game_algo.AlphaBetaPruning(gameState, 3,true);
                //stopwatch.Stop(); // 停止计时
                //Debug.Log($"Alpha-Beta 剪枝算法执行时间: {stopwatch.ElapsedMilliseconds} 毫秒");
                OnBoardClicked(tempMoveInfo.Position);
            }
        }
    }

    void GamePlayPCvsPC()
    {
        //
    }
}


