using UnityEngine;
using UnityEngine.SceneManagement;

class ReturnToMainMenu : MonoBehaviour {
    public void ToMenu() {
        var currentName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene("Menu");
        SceneManager.UnloadScene(currentName);
    }
}
