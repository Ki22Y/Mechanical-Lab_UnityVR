using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorUIPanel : MonoBehaviour
{
    // Called from the Restart button
    public void OnRestartButtonPressed()
    {
        // Reload the current scene (restart the whole minigame)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called from the Exit button
    public void OnExitButtonPressed()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop playmode in Editor
        #else
            Application.Quit(); // Quits standalone app
        #endif
    }
}
