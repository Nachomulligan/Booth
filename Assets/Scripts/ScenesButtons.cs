using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesButtons : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    public void PausarJuego()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }
    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.name);
    }
}