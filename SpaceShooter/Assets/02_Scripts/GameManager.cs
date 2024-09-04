// 네임스페이스
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    // 게임 개발 디자인 패턴
    // 싱글턴 디자인 패턴 (Singleton Design Pattern)
    // 공통적으로 접근해야하는 개체를 하나만 써서 사용하게 만드는 것.
    // 어디서든지 전역적으로 접근을 허용하는 디자인 패턴
    public static GameManager Instance = null; // 자기자신을~

    public List<Transform> points = new List<Transform>();
    public GameObject monsterPrefab;

    private bool isGameOver = false;

    // 오브젝트 풀 정의(선언)
    public List<GameObject> monsterPool = new List<GameObject>();
    // 오브젝트 풀의 갯수
    public int maxPool = 10; // 몬스터를 미리 pool 에 10 마리 만들어 놓고 끌어오겠다

    // 외부에 노출 될 프로퍼티 선언
    public bool IsGameOver
    {
        get
        {
            return isGameOver;
        }
        set
        {
            isGameOver = value;


            //if (isGameOver)
            //{
            //    Debug.Log("게임오버");
            //   // 엔딩 타이틀 UI 표현
            //    CancelInvoke(nameof(CreatMonster));
            //}
        }
    }

    // 스코어에 더해져서 표시될 점수 저장 변수 선언
    private int score = 0;

    // 점수 프로퍼티 선언
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            // 점수를 저장
            score += value;
            PlayerPrefs.SetInt("SCORE", score);

            // 점수를 출력
            scoreText.text = $"SCORE : {score:0000000}";
        }
    }
    public TMP_Text scoreText;

    void Awake()
    {
        // 게임에서 제일 중요한거라 GameManager 에서 Awake 함수로 처리함
        if (Instance == null)
        {
            Instance = this; // 처음 들어오면 자기자신으로 지정
            // 다른 씬이 오픈 되어도 이 게임매니저가 사라지지 않아야 하기 때문에, 지속하도록 하는 메소드
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            // 중복해서 생성된 GameManager 를 삭제 처리 하는 로직
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        // 점수 로드 및 출력
        Score = PlayerPrefs.GetInt("SCORE", 0); // 스코어 값이 없으면 0 을 가져온다
        scoreText.text = $"SCORE : {score:0000000}";

        var spawnPointGroup = GameObject.Find("SpawnPointGroup");
        spawnPointGroup.GetComponentsInChildren<Transform>(points);

        CreatMonsterPool();

        // InvokeRepeating(nameof(CreatMonster), 2.0f, 3.0f);
        // 뒤에 오는 메소드 이름을 실행시키고, 살짝 시간을 기다렸다가 메소드를 실행시킨다. 반복적으로
        StartCoroutine(CreatMonster());
    }

    private void CreatMonsterPool()
    {
        for (int i = 0; i < maxPool; i++)
        {
            GameObject monster = Instantiate(monsterPrefab); // 어딘가에서 만들어질거다
            // 이름이 monster(clone)으로 생성되니까 바꿔주기
            monster.name = $"Monster_{i:00}";
            monster.SetActive(false);
            // Object Pool에 몬스터 하나 추가
            monsterPool.Add(monster);
        }
    }

    /*
void CreatMonster()
{
   // 1. 난수 발생 (위치를 랜덤하게)
   int index = UnityEngine.Random.Range(1, points.Count);

   Instantiate(monsterPrefab, points[index].position, Quaternion.identity);
}
*/
    IEnumerator CreatMonster()
    {
        while (!isGameOver)
        {
            // 난수 발생
            int index = UnityEngine.Random.Range(1, points.Count);

            // Object Pool 에서 비활성화 된 몬스터를 추출
            foreach (var monster in monsterPool)
            {
                if (monster.activeSelf == false) // 스스로 active 되어있지 않은 몬스터라면~
                {
                    monster.transform.position = points[index].position;
                    // 랜덤한 위치에 몬스터를 생성 시킴
                    monster.SetActive(true);
                    // 몬스터를 active 시킨다
                    break; // foreach 를 빠져나가는 구문
                }
            }
            // Instantiate(monsterPrefab, points[index].position, Quaternion.identity);

            yield return new WaitForSeconds(3.0f);
        }
    }

}
