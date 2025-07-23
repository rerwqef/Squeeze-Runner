using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public bool gameActive;
    public GameObject slider;
    public Button resetButton;
    private void OnEnable()
    {
        resetButton.onClick.AddListener(Reset);
    }
    private void OnDisable()
    {
        resetButton.onClick.RemoveListener(Reset);
    }
    public void Reset()
    {
        SceneManager.LoadScene(0);
    }
    public void StartGame()
    {
        slider.SetActive(true);
        resetButton.gameObject.SetActive(true);
        PlatformController.Instance.StartTheGame();
        gameActive = true;
    }
    
    private void Update()
    {
        if (!gameActive && Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartGame();
        }
    }
}
