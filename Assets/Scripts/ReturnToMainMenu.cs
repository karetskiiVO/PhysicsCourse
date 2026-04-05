using UnityEngine;
using UnityEngine.SceneManagement;

class ReturnToMainMenu : MonoBehaviour {
    [SerializeField]
    string menuSceneName;

    public void ToMenu() {

        SceneManager.LoadScene(menuSceneName);
        SceneManager.UnloadScene(SceneManager.GetActiveScene().name);
    }
}
