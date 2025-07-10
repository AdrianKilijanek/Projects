using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ShipGameManager : MonoBehaviour
{
    public CharacterController ship;
    public Transform startPoint;
    public Transform endPoint;
    public GameObject pauseScreen;
    public GameObject endScreen;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI angleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI currentScoreText;

    public Button[] restartButtons;
    public Button[] mainMenuButtons;
    public Button continueButton;
    public Button pauseButton;

    public float gameTime = 180f;
    private int points = 0;
    private float timeRemaining;
    private bool isGameOver = false;
    private bool isPaused = false;
    private bool canDetectEndpoint = true;

        private List<Vector3> validPositions = new List<Vector3>
    {
        new Vector3(400, 34, 350),
        new Vector3(258, 34, 295),
        new Vector3(423, 34, 210),
        new Vector3(470, 34, 309),
        new Vector3(431, 34, 538),
        new Vector3(541, 34, 538),
        new Vector3(324, 34, 324),
    };

    private Vector3 lastPosition;

    public float endpointMoveRange = 100f; // Max movement range
    public LayerMask waterLayer; // Water plane layer

    // Stationary ship spawning
    public GameObject[] stationaryShipPrefabs;
    public int numberOfShips = 5;

    void Start()
    {
        timeRemaining = gameTime;
        points = 0;
        pauseScreen.SetActive(false);
        endScreen.SetActive(false);
        UpdatePointsText();
        UpdateTimerText();
        UpdateHighScoreText();

        foreach (Button button in restartButtons)
        {
            button.onClick.AddListener(RestartLevel);
        }

        foreach (Button button in mainMenuButtons)
        {
            button.onClick.AddListener(GoToMainMenu);
        }

        continueButton.onClick.AddListener(ContinueGame);
        pauseButton.onClick.AddListener(TogglePause);

        //SpawnStationaryShips();
    }

    void Update()
    {
        if (isGameOver || isPaused) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
        }

        UpdateTimerText();
        UpdateSpeedAndAngleText();
        CheckEndPoint();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

private void CheckEndPoint()
{
    if (endPoint == null)
    {
        Debug.LogError("EndPoint is not assigned in the Inspector!");
        return;
    }

    float distanceToEndPoint = Vector3.Distance(ship.transform.position, endPoint.position);
    Debug.Log("Distance to EndPoint: " + distanceToEndPoint);

    // Only process detection if allowed by the cooldown flag
    if (canDetectEndpoint && distanceToEndPoint < 30f)
    {
        Debug.Log("Endpoint reached! Teleporting...");
        points++;
        UpdatePointsText();

        // Disable further detections until after cooldown
        canDetectEndpoint = false;
        StartCoroutine(MoveEndPoint());
    }
}

    IEnumerator MoveEndPoint()
    {
        if (endPoint == null)
        {
            Debug.LogError("EndPoint is not assigned!");
            yield break;
        }

        Vector3 newPosition = GetNewPosition();

        if (newPosition != Vector3.zero)
        {
            yield return new WaitForSeconds(0.5f);  // Brief delay for effect
            endPoint.position = newPosition;
            lastPosition = newPosition;
            Debug.Log("Endpoint moved to: " + newPosition);
        }
        else
        {
            Debug.LogWarning("No valid new position found!");
        }

        yield return new WaitForSeconds(3f); // Cooldown before allowing new detection
        canDetectEndpoint = true;
    }

    Vector3 GetNewPosition()
    {
        // Create a list of possible positions excluding the last used one
        List<Vector3> availablePositions = new List<Vector3>(validPositions);
        availablePositions.Remove(lastPosition);

        if (availablePositions.Count == 0)
        {
            Debug.LogWarning("No available positions left!");
            return Vector3.zero;
        }

        // Pick a random position from the remaining ones
        return availablePositions[Random.Range(0, availablePositions.Count)];
    }
    void UpdatePointsText()
    {
        pointsText.text = "Points: " + points;
        currentScoreText.text = "Current Score: " + points;
    }

    void UpdateSpeedAndAngleText()
    {
        speedText.text = "Speed: " + BluetoothManager.Instance.GetSpeed().ToString();
        angleText.text = "Angle: " + BluetoothManager.Instance.GetAngle().ToString();
    }

    void UpdateTimerText()
    {
        float minutes = Mathf.Floor(timeRemaining / 60);
        float seconds = Mathf.Floor(timeRemaining % 60);
        timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }

    void UpdateHighScoreText()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;
    }

    void EndGame()
    {
        isGameOver = true;
        endScreen.SetActive(true);
        finalScoreText.text = "Final Score: " + points;

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (points > highScore)
        {
            PlayerPrefs.SetInt("HighScore", points);
            highScoreText.text = "High Score: " + points;
        }

        UpdateHighScoreText();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        pauseScreen.SetActive(isPaused);
        UpdateHighScoreText();
        currentScoreText.text = "Current Score: " + points;
    }

    void RestartLevel()
    {
        isGameOver = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToMainMenu()
    {
        isGameOver = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    void ContinueGame()
    {
        TogglePause();
    }



}
