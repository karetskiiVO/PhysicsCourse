using UnityEngine;
using UnityEngine.SceneManagement;

class ReturnToMainMenu : MonoBehaviour {
    [SerializeField]
    string menuSceneName;

    public void ToMenu() {
        var currSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(menuSceneName);
        SceneManager.UnloadScene(currSceneName);
    }
}
