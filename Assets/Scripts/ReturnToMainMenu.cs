using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class ReturnToMainMenu : MonoBehaviour {
    [SerializeField]
    string menuSceneName;

    void Start() {
        var button = GetComponent<Button>();
        if (button != null) {
            button.onClick.AddListener(ToMenu);
        }
    }

    public void ToMenu() {
        var currSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(menuSceneName);
        SceneManager.UnloadScene(currSceneName);
    }
}
