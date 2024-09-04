using UnityEngine;
using UnityEngine.Pool;

public class TestPool : MonoBehaviour
{
    public static TestPool Instance { get; private set; }


    //Pool 변수 선언
    public IObjectPool<Test> bulletPool;
    public GameObject bulletPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        // ObjectPool 초기화
        bulletPool = new ObjectPool<Test>
        (
            // 오프젝트 풀링이 생성 될 때
            createFunc: CreateItem,
            // 하나 끄집어 낼 때 호출되는 함수
            actionOnGet: OntakeItem,
            // 오브젝트로 반환할 때
            actionOnRelease: OnReturnItem,
            actionOnDestroy: OnDestroyItem,
            defaultCapacity: 5,
            maxSize: 10,
            collectionCheck: false
        );

    }

    private Test CreateItem() // 한 줄 모두 지우기!! 컨트롤 X
    {
        Test bullet = Instantiate(bulletPrefab).GetComponent<Test>(); // 불렛을 추출해서 저장한다~
        return bullet;
    }

    private void OntakeItem(Test bullet)
    {
        bullet.gameObject.SetActive(true); // 불렛을 끄집어내와서 활성화를 시켜준다.
    }

    private void OnReturnItem(Test bullet)
    {
        bullet.gameObject.SetActive(false); // 불렛을 끄집어내와서 비활성화를 시켜준다.   
    }

    private void OnDestroyItem(Test bullet)
    {
        Debug.Log("초과 아이템 삭제");
        Destroy(bullet.gameObject); // 실제 물리적으로 삭제를 해야한다
    }

}
