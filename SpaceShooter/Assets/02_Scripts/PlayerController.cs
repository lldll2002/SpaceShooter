// 캐릭터를 컨트롤 하기 위해서는 어떤게 필요할까?
// 키보드랑 마우스로 입력할거니까, 그 값을 받아와서 원하는 방향으로 움직이기
// 키보드 값을 받아오려면 뭐가 필요할까?
// 


#pragma warning disable CS0108 // 다음 규약에 따른 경고를 띄우지 않음 CS0108


using System;
using System.Collections;
// using System.Numerics; 이미 Vector값이 추가 되어있다. Vector 빨간 줄 뜨면, 이거 지움.
// using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour // 베이스 클래스 C# 은 베이스클래스가 여러개일 때 상속이 불가능함. MonoBehaviour 만 상속 가능 그래서 추가로 상속 더해줌
{
    // 전역변수(Global Variable) 선언(인풋매니져와 관련된 것)
    // 프로그램 전체에서 접근할 수 있는 변수.
    // C# 에서는 직접적으로 지원하진 않아서 Class 내에서 'static', 'public' 변수를 사용해서 사용 가능
    private float v; // 초기값을 선언하지 않으면 자동으로 0 입.
    private float h; // v 버티컬 h 호라이즌
    private float r; // 마우스가 X축으로 욺직일 때의 델타값을 저장하는 함수를 선언
    private float mouseY; // 마우스 Y축 하고싶다....이렇게 하면 안된다는데...?
    // private float mouseX; 로 쓰면 좀 더 명확하다.

    [SerializeField] // attribute 적용. 바로 다음 한 줄만 속성이 부여된다. 
    // private로 설정된 정보를 유니티 안에서 Inspector에 표현해준다
    private float moveSpeed = 8.0f; // 코딩 줄 안에 직접 숫자를 적어넣는게 아닌 moveSpeed로 치환해서 쓰기 위해 선언
    [SerializeField] private float turnSpeed = 200.0f; // 분당 회전하는 속도
    // 유니티 안에서 inspector 에 있는 Move Speed 가 더 우선 된다. 
    // 만약 유니티에서 속도가 6이면 코드가 8이어도 6으로 적용됨.
    [SerializeField] private Transform firePos;
    // [SerializeField]
    // private GameObject bulletPrefab;
    [SerializeField] private AudioClip fireSfx;
    [SerializeField] private Image hpBar;

    // 총알 효과 추가=========================
    public MeshRenderer muzzleFlash;
    // ======================================

    // Animator 컴포넌트를 저장할 변수 설정
    // [HideInInspector] , [NonSerialized] 하면 퍼블릭이지만 Inspector에서 안보이게 설정 가능함.
    public Animator animator; // Animator 클래스타입의 데이터를, animator 라는 변수로 저장한다.
    private new AudioSource audio;

    //이벤트 선언
    private CharacterController cc;
    // Animator Hash 추출
    // forward 등을 사용하는거보다 해쉬값을 바로 사용하는게 빠르기 때문에 추출함.
    private readonly int hashForward = Animator.StringToHash("forward");
    private readonly int hashStrafe = Animator.StringToHash("strafe");


    // // 모든 스크립트의 Start 함수보다 먼저 호출, 1번 호출
    // void Awake()
    // {

    // }
    // // 한 번 호출, 해당 스크립트가 활성화 될 때마다 매번 호출됨.
    // void OnEnable()
    // {

    // }

    // User Define Event 사용자 정의 이벤트
    // Delegate 델리게이트
    // 이벤트 역시 일종의 함수이기 때문에 미리 정의를 해 두고 발생을 시켜야함.
    // public delegate 이벤트의 형식 .... Handler가 붙었다? 델리게이트인가? 의심해봐야함.
    public delegate void PlayerDieHandler();
    // 이벤트 선언 어디서든 호출 할 수 있어야 하기 때문에 static 으로 선언한다.
    // 메모리의 최 상단에 올라가 있어서 다른 클래스에서 쉽게 접근할 수 있는 특징을 가진다.
    public static event PlayerDieHandler OnPlayerDie; // on~뭐뭐 할 때 마다. 변수. 플레이어가 죽을 때 마다~

    private float initHp = 100.0f;
    private float currHp = 100.0f;

    // Lamda Expression 람다식
    private bool isFire => Input.GetMouseButton(0);
    // isFire 변수를 읽으면, =>(goes to~) 를 실행한다. 

    // 연사 속도 조절
    [SerializeField] private float fireRate = 0.3f;
    // 다음 발사 시간
    private float nextFireTime = 0.0f;

    // 한 번 호출, 해당 클래스 안에서 제일 먼저 호출되는 함수
    void Start()
    {
        // Cursor Lock
        //Cursor.lockState = CursorLockMode.Locked;
        // Cursor.SetCursor(); 유저 커스텀 커서를 사용할 수 있게 해줌

        //Application.targetFrameRate = 60; // 모든 장비에서 동일하게 프레임이 나오도록 조정하는 것
        cc = GetComponent<CharacterController>();
        animator = this.gameObject.GetComponent<Animator>();
        // 애니메이션의 링크를 연결하는 함수.
        // 플레이어 컨트롤로 들어가 있는 오브젝트(player)의 컴포넌트 중에 Animator 타입의 컴포넌트를 get하겠다.
        // this.gameObject는 생략할 수 있다.
        audio = GetComponent<AudioSource>();

        // ==========총알 효과 ==========
        muzzleFlash = firePos.GetComponentInChildren<MeshRenderer>();
        muzzleFlash.enabled = false; // 처음엔 꺼져있어야 하니까

    }
    // // 규칙적인 간격으로 호출됨. 기본 값은 0.02sec 간격으로 호출
    // // 유니티 물리엔진이 백그라운드로 연산하는 물리연산 주기.
    // void FixedUpdate()
    // {

    // }
    // 매 프레임 마다 호출되는 함수 , if 60FPS면 초당 60번씩 호출되는 함수. 
    // 이론적으로는 화면을 렌더링하는 횟수와 동일함. 그러나, 리소스에 따라서 불규칙한 주기.
    void Update()
    {
        // 매번 불러오니까 연산이 긴거를 넣어버리면 프레임저하의 원인이 될 수 있다.
        // 키보드 입력하면 ~해라 같은것도 따로 빼서 메서드화 시키면 좋다.
        // 계산 결과값
        // ex 캐릭터의 이동

        // Debug.Log(Time.deltaTime); // 몇 번이나 재생되는지 확인하는 계산값

        InputAxisMethod();
        LocomotionMethod();
        AnimationMethod();
        Fire();
        // 그동안 작성했던 함수들을 전체 지정해서 노란전구->함수추출 해서 새 이름을 지정했다.

    }

    private void Fire()
    {
        Debug.DrawRay(firePos.position, firePos.forward * 10.0f, Color.green);

        // if (Input.GetMouseButtonDown(0)) 이거를 람다문법으로 변경.
        if (isFire && Time.time >= nextFireTime)
        // 총을 발사하고, 다음 총 쏘는 시간보다 흘러간 시간이 더 크면,
        {

            // 총알 프리팹을 이용해서 런타임에서 동적을 생성
            // Instantiate(bulletPrefab, firePos.position, firePos.rotation); //처음에는 이거 썼다가 나중에는 싱글톤으로 PoolManager 사용
            // 너무 많은 Instantitate 사용하면 램 많이 먹음 ..

            // // PoolManager 총알을 가져오는 로직
            Bullet bullet = PoolManager.Instance.bulletPool.Get(); // 처음에 총알이 안만들어져 있을 경우, 위의 createbullet 이 실행됨. 20개가 다 만들어진 후엔 이미 만들어져있는 pool 에서 리턴을 함.
            // Test bullet = TestPool.Instance.bulletPool.Get();
            bullet.transform.SetPositionAndRotation(firePos.position, firePos.rotation);
            // //Debug.Log(firePos.position + ", " + firePos.rotation);

            // // // 총알 발사하기
            bullet.Shoot();

            if (Physics.Raycast(firePos.position, firePos.forward, out RaycastHit hit, 10.0f, 1 << 8 | 1 << 10))
            // 비트연산 1<<8 옆으로 8칸 가라... 2의 8승... 256 왜 썼나? 여러가지 조합을 사용할 수 있기 때문에. 예를들어 1<<8 | 1<<9 8,9번 레이어 모두 검출하겠다~
            // 발사 원점과 총구가 발사되는 방향에서 10m 간 raycast 를 발사, 뭔가에 닿으면 hit 라는 true 값을 리턴해줌.
            {
                Debug.Log($"Hit = {hit.collider.name}");
                hit.collider.GetComponent<IDamagable>().OnDamaged(); // 닿아서 터지는게 여러개가 되면 if 절로 하나하나 구분하면 처리량이 많아지기 때문에
                // IDamagable 스크립트로 하나 추가 만든다.

                // Raycast 는 반드시!!! 콜라이더가 있는 것만 검출 가능 

                // 
            }
            // Raycast 광선 발사되고 첫 번째 몹에 닿으면 멈춤,
            // ~All 뚫고 다 지나감
            // none 검출된 갯수가 확실치 않을 때 (fix안되어있을 때)



            // 충돌감지
            // OnCollisionEnter 충돌감지
            // OnCollisionStay 벽에 그대로
            // OnCollisionExit 사라짐

            // OnTrigger v 체크 되어있으면, OnTriggerEnter, Stay, Exit 으로 나옴
            // 1. 둘 다 충돌을 발생기키기 위한 Collider Component 들어있어야 함
            // 2. 하나는 이동하는 객체는 반드시 rigidbody 들어가있어야 함.

            // 용어
            // Call Back Method, Function -> Event 라고도 함.
            // 뒤에서 호출을 준다~ 언제 만족할 지 모르는 함수를 백그라운드에서 대기했다가
            // 만족하는 순간 호출을 해준다.
            // 매 프레임마다 충돌감지를 할 수 없으니까, 조건에 따라서 호출을 해줌.

            // 충돌 할 때 총알 

            // 총 소리 발생
            audio.PlayOneShot(fireSfx, 0.8f); // 한 번 샷에 지금 소리설정의 80% 정도로 소리를 발생시킨다.

            // 총구 화염 효과 깜빡
            StartCoroutine(ShowMuzzleFlash()); // 반드시 StartCoroutine 함수로 시작해야함.

            // nextFireTime 발사 시각 = 현재시간 + 연사 속도
            nextFireTime = Time.time + fireRate;
        }
    }

    // 총 쏠 때 마다 깜빡거리게 만드는 함수 ===================
    // 코루틴 - (Co-routine)
    IEnumerator ShowMuzzleFlash()
    {
        // 비활성화 시켜놨던 muzzleFlash를 활성화
        muzzleFlash.enabled = true;

        // Texture Offset 변경 (0, 0) (0.5, 0) (0.5, 0.5) (0, 0,5)
        // Random.Range(0,2) (0, 1) * 0.5 0~2까지의 난수를 발생시킨 후 0.5를 곱한다.
        // 그러면 0, 0.5, 1 의 값을 Return 할 수 있음
        Vector2 offset = new Vector2(Random.Range(0, 2), Random.Range(0, 2)) * 0.5f;
        muzzleFlash.material.mainTextureOffset = offset;

        // Scale 변경
        muzzleFlash.transform.localScale = Vector3.one * Random.Range(1.0f, 3.0f);

        // Z 축으로 랜덤하게 회전하는 코드
        muzzleFlash.transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(0, 360));

        // 켜진 상태에서 약간의 대기가 필요함 (바로꺼지면 안보임)
        // waitting ... 좀 더 쉽게 사용하기 위해서 IEnumerator 사용함
        // 코루틴 함수에서는 반드시 waiting(yield) 함수가 이써야함.
        yield return new WaitForSeconds(0.2f);
        // 0.2초간 지날 때 까지 메인프로세스에 양보하고, 안지났으면 양보
        // 전부 지났으면 다음 루틴을 실행한다.

        // 다시 비활성화
        muzzleFlash.enabled = false;

    }

    private void AnimationMethod()
    {
        // // 애니메이션 파라메터 전달
        // animator.SetFloat("forward", v); //animator 변수에 저장된 Float타입의 변수를 Set 한다.
        // // forward라는 이름의 변수를, v 값 만큼.
        // animator.SetFloat("strafe", h);
        // // 위 처럼 변수 안에 forward 등 으로 문자를 쓰면 내부적으로 해쉬값 전환 등 작업이 추가되서 느려짐
        // // 해쉬값을 바로! 적용하면 빨라짐.
        animator.SetFloat(hashForward, v);
        animator.SetFloat(hashStrafe, h);
    }

    private void LocomotionMethod()
    {
        // Vector의 덧셈 연산 여러개 쓴 벡터를 하나로 합치기 !!!!
        // 기본적으로 벡터의 덧셈은 피타고라스 정의에 의해서 루트2로 증가하기 때문에,
        // 인게임에서는 설정한 값 보다 빨라지는 문제가 생긴다.
        // 이것을 다시 1로 바꾸기 위해서 정규화벡터를 찾아야한다.
        // Vector3 moveDir = (전진후진방향벡터) + (좌우방향벡터);
        // ~~~~~~~~~(이동 처리 로직들)~~~~~~~~~~~
        // Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        Vector3 moveDir = (transform.forward * v) + (transform.right * h);
        // Debug.Log("비정규화" + moveDir.magnitude);
        // Debug.Log("정규화" + moveDir.normalized.magnitude);


        // // transform.position += new Vector3(0, 0, 0.1f); // vector3의 형태로 새로운 xyz의 위치값을 지정하겠다..
        // // transform.position += Vector3.forward * 0.1f; // 앞으로~ 이동한다.
        // transform.Translate(Vector3.forward * v * 0.1f); // 메소드를 사용하는 방법. , 
        // // ~~~ v 값을 곱한다!! 위에서 지정한 v 는 키보드 인풋 버티컬이니까 , 키보드 안누르면 안움직임 
        // // transform.Translate(-Vector3.forward * 0.1f); // 반대로 가는 표시
        // transform.Translate(Vector3.right * h * 0.1f);

        // transform.Translate(moveDir.normalized * Time.deltaTime * moveSpeed);
        // 플레이어 컨트롤러로 캐릭터 컨트롤하기
        cc.Move(moveDir.normalized * Time.deltaTime * moveSpeed);
        /*
            60FPS => 0.016666 0.016666*60
            30FPS => 0.033333 0.033333*30
        */
        // ~~~~~~~~~~여기까지 이동처리 로직~~~~~~~~~~

        // 회전 처리 로직
        transform.Rotate(Vector3.up * Time.deltaTime * r * turnSpeed); //Y축(up)을 기준으로 한다, 시간은 고정, 변위값(r), 속도
    }

    private void InputAxisMethod()
    {
        // 매 프레임마다 키보드가 입력 되었는지 확인해야하니까 update에 적는다
        v = Input.GetAxis("Vertical"); // ~~여기서 리턴하는 값을 v 라는 변수에 집어넣겠다~~  
        //Input은 외부로부터 들어오는 것들을 관리하는 클래스
        // GetAxis 축 값을 받아옴. -1.0f ~ 0.0 ~ +1.0f 까지, 연속적인 값이 들어온다.
        // Debug.Log(v); // C#에서 썼던 Console.WriteLine 과 비슷한거
        // 콘솔뷰에 메세지를 출력하는 메소드.
        // Debug.log("메세지");
        h = Input.GetAxis("Horizontal");
        // Debug.Log($"h={h} , v={v}");
        r = Input.GetAxis("Mouse X"); // 띄어쓰기 주의!! 인풋메니저에 있는 글자랑 똑같이 맞추기
                                      // v, h 값이 양수이면 앞으로/위로 가고 있다~ 를 알 수 있다. 이 것을 통해 애니메이션을 적용할 수 있음
                                      // mouseY = Input.GetAxis("Mouse Y");
    }

    // // Update가 호출 된 횟수만큼 호출됨
    // void LateUpdate()
    // {
    //     // Update의 계산결과를 후처리작업
    //     // 카메라 이동, (플레이어가 움직이고 나면 카메라가 따라가니까)
    // }

    // 충돌 콜백 함수(몬스터에 피격을 당했을 때)
    void OnTriggerEnter(Collider coll) // 몬스터 양 손에 있는 스피어콜라이더가 넘어온다
    {
        if (currHp > 0.0f && coll.CompareTag("PUNCH"))
        {
            currHp -= 10.0f;

            hpBar.fillAmount = currHp / initHp;

            if (currHp <= 0.0f)
            {
                // 이벤트를 작동시키겠다 (Raise)
                OnPlayerDie();

                // 몬스터 생성 중지
                GameManager.Instance.IsGameOver = true;

                /* 
                    instance 함수가 없다고 하면....
                    GameObject.Find("GameManager").GetComponent<GameManager>().IsGameOver = true;
                    굉장히 길고, 효율이 떨어진다.
                */
                // PlayerDie();
            }
            // Debug.Log(coll.gameObject.name);
        }
    }

    private void PlayerDie()
    {
        // MONSTER Tag 달고있는 모든 몬스터를 추출한다.
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        foreach (var monster in monsters) // 몬스터스에 들어가있는 것 중 첫 번째 몬스터를 끄집어 낸다~~~쭉 돌아가면서 끝까지
        {
            // monster.SendMessage("YouWin", SendMessageOptions.DontRequireReceiver); // 이 함수를 호출해 주세요~, 리시버가 없어도 됩니다.
            monster.GetComponent<MonsterController>().YouWin(); // 두 번째 방법
        }
    }


}


/*
    암기!!!!!
    Vector3.forward = Vector3(0, 0, 1)
    Vector3.up      = Vector3(0, 1, 0)
    Vector3.right   = Vector3(1, 0, 0)

    나머지 반대방향은 마이너스로 표현한다.

    Vector3.one     = Vector3(1, 1, 1)
    Vector3.zero    = Vector3(0, 0, 0)


*/

/*
    Quaternion 쿼터니언(사(4)원수) Vector3 = x,y,z , Quaternion은 w 까지 추가한다.
    복소수 사차원 벡터
    
    오일러 회전 (오일러 Euler 0 ~ 360) 
    x -> y -> z 순서로 값

    짐벌락(Gimbal Lock) 발생

    Quaternion.Euler(30, 45, -15) 하면 자동적으로 치환을 해줌.
    Quaternion.LookRotation(벡터) 벡터가 가르키는 방향을 쿼터니언 값으로 변형시킴.
    Quaternion.identity = 0 각도다.
*/

/*
    유한 상태 머신 (Finite State Machine : FSM)
    현재 상태에 따라서 행동패턴을 지정하는 것. 칸 하나하나를 State 라고 함.

*/


/*
    UI 

    IMGUI --> 성능도 낮고, N-Screen도 사용불가
    NGUI

    UGUI --> NGUI 개발자를 데려와서 만든 것. 마우스로 작업을 해야함 눈으로 보면서 작업할 수 있어서 편리함
    UIToolkit -> CSS, HTML 과 같은 유사한 방식으로 작업하는 느낌 아직까지는 UGUI 에 비해서 사용이 까다롭다.
*/