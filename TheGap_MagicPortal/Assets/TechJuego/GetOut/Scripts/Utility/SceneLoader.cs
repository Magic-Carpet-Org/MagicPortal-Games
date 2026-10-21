using UnityEngine.SceneManagement;
namespace TechJuego.GetOut.Utils
{
    public class SceneLoader
    {
        public static void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public static void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}