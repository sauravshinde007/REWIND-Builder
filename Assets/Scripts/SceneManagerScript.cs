using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    // Reference to the Animator controlling the transition effect
    [SerializeField] private Animator transition;
    // Time for the transition animation
    [SerializeField] private float transitionTime = 1f;

    /// Coroutine to handle scene loading with a transition.
    private IEnumerator LoadLevel(string sceneName)
    {
        // Play the transition animation
        if (transition != null)
        {
            transition.SetTrigger("Start");
        }

        // Wait for the duration of the transition
        yield return new WaitForSeconds(transitionTime);

        // Load the specified scene
        SceneManager.LoadScene(sceneName);
    }


    /// Public method to load a scene with a transition.
    public void LoadScene(string sceneName)
    {
        if (transition != null)
        {
            // Start the coroutine for loading the scene with a transition
            StartCoroutine(LoadLevel(sceneName));
        }
        else
        {
            // If no transition is set, load the scene immediately
            SceneManager.LoadScene(sceneName);
        }
    }

    /// Exits the game application.
    public void ExitGame()
    {
        Application.Quit();
    }
}
