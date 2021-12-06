using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameStateHandler : MonoBehaviour
{
    public static GameStateHandler current;

    public int scoreMultiplier = 10;

    [Header("UI References")]
    public TextMeshProUGUI timeTracker;
    public TextMeshProUGUI scoreTracker;

    [Header("Score Screen")]
    public GameObject scoreScreen;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreModifier;

    [Header("Game Over UI")]
    public GameObject gameOverUI;

    public int currentTime;
    public int playerScore;
    int lastTime = 0;
    int displayScore;

    bool addingScore = false;
    bool gameEnded = false;
    bool scoreScreenOpen = false;

    // References
    Player player;

    private void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        player = FindObjectOfType<Player>();
        gameOverUI.SetActive(false);
        playerScore = 0;
        scoreScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.playerStats.GetCurrentHealth() <= 0)
        {
            GameOver();
            if (scoreScreenOpen)
            {
                displayScore = (int)Mathf.MoveTowards(displayScore, playerScore, 1000f * Time.unscaledDeltaTime);
                Debug.Log("Updating final score display.");
                UpdateFinalScoreDisplay();
            }
        }
        else
        {
            displayScore = (int)Mathf.MoveTowards(displayScore, playerScore, 1000f * Time.deltaTime);
            UpdateScoreDisplay();
        }
    }

    public void FixedUpdate()
    {
        if (player.playerStats.GetCurrentHealth() <= 0)
        {
            return;
        }
        currentTime = (int)(Time.time - GameManager.current.elapsedTime - GameManager.current.elapsedTimeGame);
        timeTracker.text = "Time: " + currentTime;
        lastTime = currentTime;
    }

    public void AddScore(int amount)
    {
        playerScore += amount;
    }

    public void UpdateScoreDisplay()
    {
        scoreTracker.text = "Score: " + displayScore;
    }

    public void UpdateFinalScoreDisplay()
    {
        scoreText.text = displayScore.ToString();
    }

    public void GameOver()
    {
        if (!gameEnded)
        {
            PauseGame();
            player.gameObject.SetActive(false);
            gameOverUI.SetActive(true);
            GameManager.current.elapsedTimeGame = currentTime;
            gameEnded = true;
        }
    }

    public void CalculateFinalScore()
    {
        displayScore = playerScore;
        scoreText.text = displayScore.ToString();
        GameManager.current.scoreModifiers[0].score = currentTime * scoreMultiplier;
        StartCoroutine(ShowScoreModifiers(GameManager.current.scoreModifiers, 0f));
    }

    IEnumerator ShowScoreModifiers(List<ScoreModifier> modifiers, float time = 4f)
    {
        yield return new WaitForSecondsRealtime(time);
        scoreModifier.text = modifiers[0].name + ": " + modifiers[0].score;
        Debug.Log("Score step 1.");
        playerScore += modifiers[0].score;
        Debug.Log("Score step 2.");
        modifiers.RemoveAt(0);
        if (modifiers.Count > 0)
        {
            Debug.Log("Showing next modifier.");
            StartCoroutine(ShowScoreModifiers(modifiers));
        }
    }

    public void ScoreScreen()
    {
        Debug.Log("Showing score screen.");
        scoreScreenOpen = true;
        gameOverUI.SetActive(false);
        scoreScreen.SetActive(true);
        CalculateFinalScore();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        ResumeGame();
        gameOverUI.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
}
