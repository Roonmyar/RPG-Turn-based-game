using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUIController : MonoBehaviour
{
    public void OnContinueButton()
    {
        // Закрываем UI (убираем панель)
        Destroy(gameObject);

        // Возвращаемся в основную локацию (замените "MainScene" на имя вашей сцены)
        string baseScene = SceneManager.GetActiveScene().name.Replace("_Battle", "");
        LocationManager.Instance.GoToLocation(baseScene);
    }
}