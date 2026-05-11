using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Task3 {
    [RequireComponent(typeof(Button))]
    public abstract class Loader : MonoBehaviour {
        abstract protected System.Type sceneGenerator { get; }
        abstract protected string Name { get; }

        const string TargetSceneName = "MainTask3";

        void Start() {
            var button = GetComponent<Button>();
            var text = GetComponentInChildren<Text>();

            button.onClick.AddListener(Load);
            text.text = Name;
        }

        void Load() {
            var currSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadSceneAsync(TargetSceneName).completed +=
                (_ => GameObject.Find("World").AddComponent(sceneGenerator));
            SceneManager.UnloadScene(currSceneName);
        }
    }
}
