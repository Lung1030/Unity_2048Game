using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int gridSize = 4;  // 4x4 棋盤
    public Tile[,] grid;      // 儲存數字格子

    [Header("UI References")]
    public GameObject tilePrefab; // 預製物件（方格）
    public Transform boardParent; // 棋盤父物件 (放在Canvas下)
    public Text scoreText;
    public Text bestScoreText;
    public Text timeText;
    public Button restartButton;
    public GameObject gameOverPanel; // 遊戲結束面板（可選）
    public Button infoButton;
    public GameObject infoPanel;
    public Button closeButton;

    [Header("Audio Settings")]
    public Button volumeButton;
    public AudioSource audioSource;
    public Sprite Onsprite;  // 音效開啟時的圖片
    public Sprite Offsprite; // 音效關閉時的圖片
    private bool mute_vol = false;

    [Header("Game State")]
    private int score = 0;
    private int bestScore = 0;
    private float timer = 0f;
    private bool isGameOver = false;
    private bool hasWon = false; // 追蹤是否已勝利
    private bool hasMoved = false; // 追蹤是否有移動發生

    void Start()
    {
        // 載入最高分數
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        
        // 載入音量設置
        mute_vol = PlayerPrefs.GetInt("MuteVolume", 0) == 1;
        InitializeVolumeSettings();

        grid = new Tile[gridSize, gridSize];
        CreateBoard();

        // 初始化遊戲
        AddRandomTile();
        AddRandomTile();

        UpdateUI();

        // 綁定按鈕事件
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (infoButton != null)
            infoButton.onClick.AddListener(show_info);
        if (closeButton != null)
            closeButton.onClick.AddListener(close_info);
        if (volumeButton != null)
            volumeButton.onClick.AddListener(volume_oc);
    }

    void InitializeVolumeSettings()
    {
        if (audioSource != null)
        {
            // 設置音量：靜音時為0，開啟時為0.5
            audioSource.volume = mute_vol ? 0f : 0.5f;
        }
        
        // 設置正確的按鈕圖片
        UpdateVolumeButtonImage();
        
        Debug.Log("音量設置初始化完成 - 靜音狀態: " + mute_vol + " - 音量: " + (audioSource != null ? audioSource.volume.ToString() : "無AudioSource"));
    }

    void UpdateVolumeButtonImage()
    {
        if (volumeButton != null)
        {
            Image buttonImage = volumeButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (mute_vol && Offsprite != null)
                {
                    buttonImage.sprite = Offsprite;
                }
                else if (!mute_vol && Onsprite != null)
                {
                    buttonImage.sprite = Onsprite;
                }
            }
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            // 更新計時器
            timer += Time.deltaTime;
            UpdateTimeDisplay();
            
            // 處理玩家輸入
            HandleInput();
        }
    }

    void HandleInput()
    {
        bool moved = false;
        
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            moved = Move(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            moved = Move(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            moved = Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            moved = Move(Vector2Int.right);
        
        if (moved)
        {
            AddRandomTile();
            UpdateUI();
            
            // 先檢查是否達到2048（勝利條件）
            if (!hasWon && CheckWin())
            {
                hasWon = true;
                EndGameWithWin();
            }
            // 再檢查遊戲是否結束（失敗條件）
            else if (IsGameOver())
            {
                EndGame();
            }
        }
    }

    void CreateBoard()
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject obj = Instantiate(tilePrefab, boardParent);
                Tile tile = obj.GetComponent<Tile>();
                grid[x, y] = tile;
                tile.SetNumber(0); // 預設為空
                
                // 設置格子位置（如果需要手動排列）
                // RectTransform rect = obj.GetComponent<RectTransform>();
                // rect.anchoredPosition = new Vector2(x * 110, -y * 110);
            }
        }
    }

    void AddRandomTile()
    {
        List<Vector2Int> emptyPositions = GetEmptyPositions();
        
        if (emptyPositions.Count > 0)
        {
            Vector2Int pos = emptyPositions[Random.Range(0, emptyPositions.Count)];
            int num = (Random.value < 0.9f) ? 2 : 4; // 90% 機率 2, 10% 機率 4
            grid[pos.x, pos.y].SetNumber(num);
        }
    }

    List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y].Number == 0)
                {
                    emptyPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return emptyPositions;
    }

    bool Move(Vector2Int direction)
    {
        hasMoved = false;
        
        if (direction == Vector2Int.up)
            MoveUp();
        else if (direction == Vector2Int.down)
            MoveDown();
        else if (direction == Vector2Int.left)
            MoveLeft();
        else if (direction == Vector2Int.right)
            MoveRight();
        
        return hasMoved;
    }

    void MoveUp()
    {
        for (int x = 0; x < gridSize; x++)
        {
            int[] column = new int[gridSize];
            for (int y = 0; y < gridSize; y++)
            {
                column[y] = grid[x, y].Number;
            }
            
            int[] newColumn = ProcessLine(column);
            
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y].Number != newColumn[y])
                {
                    hasMoved = true;
                    grid[x, y].SetNumber(newColumn[y]);
                }
            }
        }
    }

    void MoveDown()
    {
        for (int x = 0; x < gridSize; x++)
        {
            int[] column = new int[gridSize];
            for (int y = 0; y < gridSize; y++)
            {
                column[gridSize - 1 - y] = grid[x, y].Number; // 反向讀取
            }
            
            int[] newColumn = ProcessLine(column);
            
            for (int y = 0; y < gridSize; y++)
            {
                int newValue = newColumn[gridSize - 1 - y]; // 反向寫入
                if (grid[x, y].Number != newValue)
                {
                    hasMoved = true;
                    grid[x, y].SetNumber(newValue);
                }
            }
        }
    }

    void MoveLeft()
    {
        for (int y = 0; y < gridSize; y++)
        {
            int[] row = new int[gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                row[x] = grid[x, y].Number;
            }
            
            int[] newRow = ProcessLine(row);
            
            for (int x = 0; x < gridSize; x++)
            {
                if (grid[x, y].Number != newRow[x])
                {
                    hasMoved = true;
                    grid[x, y].SetNumber(newRow[x]);
                }
            }
        }
    }

    void MoveRight()
    {
        for (int y = 0; y < gridSize; y++)
        {
            int[] row = new int[gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                row[gridSize - 1 - x] = grid[x, y].Number; // 反向讀取
            }
            
            int[] newRow = ProcessLine(row);
            
            for (int x = 0; x < gridSize; x++)
            {
                int newValue = newRow[gridSize - 1 - x]; // 反向寫入
                if (grid[x, y].Number != newValue)
                {
                    hasMoved = true;
                    grid[x, y].SetNumber(newValue);
                }
            }
        }
    }

    int[] ProcessLine(int[] line)
    {
        int[] result = new int[gridSize];
        int index = 0;
        
        // 先移動所有非零數字到左側
        for (int i = 0; i < gridSize; i++)
        {
            if (line[i] != 0)
            {
                result[index] = line[i];
                index++;
            }
        }
        
        // 合併相同的數字
        for (int i = 0; i < gridSize - 1; i++)
        {
            if (result[i] != 0 && result[i] == result[i + 1])
            {
                result[i] *= 2;
                score += result[i]; // 加分
                result[i + 1] = 0;
                
                // 將後面的數字前移
                for (int j = i + 1; j < gridSize - 1; j++)
                {
                    result[j] = result[j + 1];
                    result[j + 1] = 0;
                }
            }
        }
        
        return result;
    }

    bool CheckWin()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y].Number == 2048)
                    return true;
            }
        }
        return false;
    }

    bool IsGameOver()
    {
        // 檢查是否還有空格
        if (GetEmptyPositions().Count > 0)
            return false;
        
        // 檢查是否還能合併
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int current = grid[x, y].Number;
                
                // 檢查右邊
                if (x < gridSize - 1 && grid[x + 1, y].Number == current)
                    return false;

                // 檢查下面
                if (y < gridSize - 1 && grid[x, y + 1].Number == current)
                    return false;
            }
        }
        
        return true;
    }

    void EndGameWithWin()
    {
        isGameOver = true;
        
        // 更新最高分數
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }
        
        UpdateUI();
        
        // 顯示勝利訊息
        Debug.Log("恭喜！你達到了2048！最終分數: " + score);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void EndGame()
    {
        isGameOver = true;
        
        // 更新最高分數
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }
        
        UpdateUI();
        
        // 顯示遊戲結束訊息
        Debug.Log("遊戲結束！最終分數: " + score);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        
        if (bestScoreText != null)
            bestScoreText.text = "Best: " + bestScore;
    }

    void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(timer / 3600);
            int minutes = Mathf.FloorToInt((timer % 3600) / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            
            if (hours > 0)
            {
                timeText.text = string.Format("Time {0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
            else
            {
                timeText.text = string.Format("Time {0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    public void show_info()
    {
        infoPanel.SetActive(true);
    }

    public void close_info()
    {
        infoPanel.SetActive(false);
    }

    public void volume_oc()
    {
        // 切換靜音狀態
        mute_vol = !mute_vol;
        
        if (audioSource != null)
        {
            if (mute_vol)
            {
                // 靜音：音量設為0
                audioSource.volume = 0f;
                Debug.Log("音效已關閉 - 音量: 0");
            }
            else
            {
                // 開啟音效：音量設為0.5
                audioSource.volume = 0.5f;
                Debug.Log("音效已開啟 - 音量: 0.5");
            }
        }
        
        // 更新按鈕圖片
        UpdateVolumeButtonImage();
        
        // 保存音量設置
        PlayerPrefs.SetInt("MuteVolume", mute_vol ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void RestartGame()
    {
        // 重置遊戲狀態
        score = 0;
        timer = 0f;
        isGameOver = false;
        hasWon = false; // 重置勝利狀態
        hasMoved = false;

        // 清空所有格子
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y].SetNumber(0);
            }
        }

        // 添加初始格子
        AddRandomTile();
        AddRandomTile();

        // 更新UI
        UpdateUI();

        // 隱藏遊戲結束面板
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log("遊戲重新開始！");
    }

    // 公開方法，供外部調用（如UI按鈕）
    public int GetScore() => score;
    public int GetBestScore() => bestScore;
    public float GetTime() => timer;
    public bool GetGameOverState() => isGameOver;
}