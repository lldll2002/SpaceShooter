using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 버튼을 할당 할 변수 선언
    public Button startButton;

    void OnEnable()
    {
        // 이벤트 연결
        startButton.onClick.AddListener(() => OnStartButtonClick());
    }

    public void OnStartButtonClick()
    {
        // 씬 로딩 (Logic 씬)
        SceneManager.LoadScene("Level01"); // 가장 처음 메뉴를 불러옴
        SceneManager.LoadScene("Logic", LoadSceneMode.Additive); // 이전에 불러온 Level01를 불러놓은 상태로 로직씬을 불러옴 
    }
}
