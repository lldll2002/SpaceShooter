using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering; // 네이게이션 쓰려면 추가함.

public class MonsterController : MonoBehaviour, IDamagable
{
    public enum State // State 를 새롭게 정의하는 것
    { // data type 을 정의하는 것. 총 4가지 상태를 가진다.
        IDLE, TRACE, ATTACK, DIE
    }  // 0 , 1 , 2 , 3 순서로 감
    // 상태를 만들었으면 저장할 변수를 추가해줘야한다
    // 현재 몬스터의 상태
    public State state = State.IDLE; // 위에서 정의한 State type. 열거형 타입의 변수. 
                                     // 어떤 알고리즘에 의해서 변경을 시킬거냐? 고민 해야함.
                                     // 몬스터와 주인공 간의 거리를 기준으로 상태를 바꾸기로 함.

    // 추적 사정거리와 공격 사정거리가 필요함.

    // 추적 사정거리
    [SerializeField] private float traceDist = 10.0f;

    // 공격 사정거리
    [SerializeField] private float attackDist = 2.0f; // 이라고 지정하기로 함.

    // 거리를 알기 위해서는 각 오브젝트의 좌표값을 알아야 함. Transfrom component 에서 접근해야함.
    // 몬스터는 동일한 컴포넌트상에 있지만, 주인공은 다른 곳에 저장되어있어서 방법이 필요함.
    // Hireachy 에서 player를 찾아서, 

    private Transform playerTr; // 주인공의 위치를 저장할 함수
    private Transform monsterTr; // 몬스터의 ''
    private NavMeshAgent agnet; // 컴포넌트 추가
    private Animator animator; // 몬스터의 동작에 애니메이션을 매칭하기 위한 변수
    private readonly int hashIsTrace = Animator.StringToHash("IsTrace"); // hash 값으로 찾으면 더 빠르기 때문에 사용
    private readonly int hashIsAttack = Animator.StringToHash("IsAttack"); // 
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashDanceSpeed = Animator.StringToHash("DanceSpeed");


    public bool isDie = false; // 몬스터가 죽었는지 안죽었는지 판단하는 변수로 사용함.

    private float hp = 100.0f;

    void OnEnable()
    {
        // PlayerController.OnPlayerDie += [연결할 함수]
        // 앞에 함수가 실행되면 뒤에 연결할 함수를 더해서 실행시키겠다~
        PlayerController.OnPlayerDie += YouWin;
        // 이벤트가 언제 발생될 지 모르니까, 메모리 어딘가에 올라가있다.
        // 사용하지 않을 때에는 끊어줘야 메모리 누수를 방지 가능.

        StartCoroutine(CheckMonsterState());
        StartCoroutine(MonsterAction()); // StartCoroutine 있어야 밑의 Coroutine 함수가 실행됨.
        // 몬스터가 죽었을 때, 다시 object pool 에 들어갔다가 나오면
        // start 함수 안에 있어버리면 추적 함수가 적용되지 않는다
        // 왜? 한 번만 실행되기 때문에. 그래서 onenable 에 있어야한다.
    }

    void OnDisable()
    {
        PlayerController.OnPlayerDie -= YouWin; // 연결고리를 끊는다.
    }


    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("PLAYER");
        playerTr = playerObj?.GetComponent<Transform>();

        monsterTr = transform;
        agnet = GetComponentInParent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // void Start() 아래의 모든 함수가 가장 먼저 실행되어야하는데 Start안에 있으면
    //              onenable 보다 늦게 실행 되기 때문에 awake 함수로 변경해줌.
    // {
    //     // GameObject playerObj = GameObject.Find("Player"); // Player라고 되어있는 게임오브젝트를 찾아서 저장한다.
    //     // Find 로 찾게 되면 hireachy 에서 처음부터 찾을 때 까지 계속 검색하게 되니까,
    //     // 딱 한 번만 찾게 되는 Start 에서 진행해야 함.
    //     GameObject playerObj = GameObject.FindGameObjectWithTag("PLAYER"); // 단 1개만 가져와야 하니까 Objects안됨!!
    //     // 여러개를 배열로 가져올 때는 Objects 로 설정할 수 있음.
    //     playerTr = playerObj?.GetComponent<Transform>(); // 가져온 값을 playerTr 이라는 변수에 넣는다.

    //     monsterTr = transform; // GetComponent<Transform>();
    //     agnet = GetComponentInParent<NavMeshAgent>(); // 네브메시 에이전트 추가
    //     animator = GetComponent<Animator>(); // 애니메이터 컴포넌트 가져오기


    // }

    /*
    void Update()
    {
        // 업데이트는 매 프레임마다 호출되는 함수. 빈번하기 때문에
        // 상태값 확인하는 코드가 여기 있을 필요가 있을까??
        // 그래서 코루틴-을 사용함    
    }
    */
    // Coroutine 을 사용하기 위한 ,IEnumerator 함수를 사용한다.
    IEnumerator CheckMonsterState()
    {
        // 몬스터가 죽었을 때는 isDie 변수를 true로 변경함.
        // 살아있는 동안에는 콜백 함수 안에 있는걸 무한 반복함.
        while (isDie == false) // isDie 가 false 라면 아래를 무한반복 하겠다.
        {
            // 몬스터의 상태가 DIE 일 경우 해당 코루틴을 정지 (밑에서 죽었는데 코루틴이 순서대로 실행되지 않을 수 있기 때문에)
            if (state == State.DIE) yield break; // 죽은 상태라면 일드 브레이크~ 바로 코루틴을 정지시켜버린다.
                                                 // 일단 코루틴 안에 들어온 상태에서 반복문이 실행될지 말지를 결정해야 하기 때문에 while 안에다가 써준다

            // 제어권을 양보하는 yield 키워드가 while 안에 있어야 한다.
            // 바깥에 있으면 진짜 무한루프에 빠져서 문제생김
            // 상태값을 측정
            float distance = Vector3.Distance(monsterTr.position, playerTr.position);
            // a,b 좌표간의 거리를 계산해주는 메소드.

            if (distance <= attackDist)
            {
                state = State.ATTACK; // 거리가 공격 거리보다 가까우면- 상태를 어택으로 바꾼다.
            }
            else if (distance <= traceDist)
            {
                state = State.TRACE; // 추격 거리~~
            }
            else
            {
                state = State.IDLE; // 아니라면 아이들로 바꾼다.
            }

            yield return new WaitForSeconds(0.3f); // 0.3초 기다리는 동안 제어권을 메인루프에 양보한다.
            // 상태값을 측정하는 건 0.3초마다 반복적으로 실행되는 코드다.
        }
        // 괄호 안의 조건을 만족하면, 계속 반복함
        // 몬스터가 죽으면 isDie가 true 가 되니까, while 에서 빠져나오게 된다.
    }


    IEnumerator MonsterAction()
    {
        while (!isDie)
        {
            // if else if 쓰면 길어지니까 switch 사용.
            switch (state)
            {
                case State.IDLE:
                    // IDLE 일 경우의 로직 처리
                    agnet.isStopped = true; // 정지상태에 있냐? 그렇다. 멈춘다.
                    // Debug.Log("정지");

                    animator.SetBool(hashIsTrace, false);

                    break; // 만족하면 처리 하고~ 노랑 중괄호를 빠져나가라.
                case State.TRACE:
                    // 추적 상태일 경우 ''
                    // 네브매시에이전트 추가 후에 작성
                    agnet.SetDestination(playerTr.position); // 플레이어 위치로 목적지를 지정한다.
                    agnet.isStopped = false; // 정지상태에 있냐? 아니다. 추적 상태

                    animator.SetBool(hashIsAttack, false);
                    animator.SetBool(hashIsTrace, true);

                    // Debug.Log("추적");
                    break;
                case State.ATTACK:
                    // 공격 상태일 경우
                    // agnet.SetDestination(Vector3.Distance(.monsterTr.position, playerTr.position) <= 1); 이거 아님........
                    agnet.isStopped = true; // 일단 정지
                    animator.SetBool(hashIsAttack, true); // 공격하기
                    // Debug.Log("공격");
                    break;
                case State.DIE:
                    isDie = true;
                    agnet.isStopped = true;
                    // 죽었을 때
                    // Debug.Log("사망");
                    animator.SetTrigger(hashDie);
                    //StopCoroutine(CheckMonsterState());
                    GetComponent<CapsuleCollider>().enabled = false; // 죽고나서 콜라이더를 없애줘야 하니까 추가해줌.
                    // Object pool 로 다시 되돌리는 코드(환원)
                    Invoke(nameof(ReturnPool), 3.0f);
                    break;

            }

            yield return new WaitForSeconds(0.3f);
            // 제일 먼저 쓰고 나머지 작성 해야 무한루프에 빠지는걸 방지할 수 있다.
        }
    }

    void ReturnPool()
    {
        // object pooling 에 다시 넣어서 쓰기 위해서 리셋해야함
        // hp를 되돌림, 죽은것도 되돌림, 캡슐콜라이더도 되돌림
        hp = 100.0f;
        isDie = false;
        state = State.IDLE;
        GetComponent<CapsuleCollider>().enabled = true;
        this.gameObject.SetActive(false);
    }

    // void OnCollisionEnter(Collision coll)
    // {
    //     if (coll.gameObject.CompareTag("BULLET")) // 충돌해온 물체가 무엇인지 판별하는 코드 "불렛"이라면~
    //     {
    //         // Destroy(coll.gameObject); // 파괴한다.
    //         PoolManager.Instance.bulletPool.Release(coll.gameObject.GetComponent<Bullet>());
    //     }
    // }
    public void OnDamaged()
    {
        animator.SetTrigger(hashHit); // 한 번만 발생하기 때문에 한 번만 실행됨
        hp -= 20.0f;
        if (hp <= 0.0f)
        {
            state = State.DIE;
            // 몬스터가 죽었을 때 점수 얻기
            GameManager.Instance.Score = 50;
        }
    }

    public void YouWin()
    {
        animator.SetFloat(hashDanceSpeed, Random.Range(0.8f, 1.5f));
        animator.SetTrigger(hashPlayerDie);
        StopAllCoroutines();
        agnet.isStopped = true;
    }
}



