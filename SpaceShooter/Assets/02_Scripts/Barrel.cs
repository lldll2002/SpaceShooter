using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Barrel : MonoBehaviour, IDamagable
{
    [SerializeField] private BarrelDataSO barrelDataSO;
    // 터트릴 거 변수 선언,

    private int hitCount = 0;
    // public GameObject expEffect;

    // [SerializeField] private Texture[] textures;

    private new MeshRenderer renderer;
    // MeshRenderer 추가

    void Start()
    {
        // 차일드에 있는 MeshRenderer 컴포넌트를 추출
        renderer = GetComponentInChildren<MeshRenderer>();
        // 텍스처를 선택하기 위한 난수 발생
        int index = Random.Range(0, barrelDataSO.textures.Length);

        renderer.material.mainTexture = barrelDataSO.textures[index];
    }


    // void OnCollisionEnter(Collision coll)
    // {
    //     if (coll.gameObject.CompareTag("BULLET"))
    //     {
    // ++hitCount; // hitCount += 1; 1씩 더해지면서 히트가 카운트 된다.
    // if (hitCount >= 3)
    // {
    //     // 폭발효과 여기에 길게 쓰지 않고 밑에 별도 함수 생성
    //     ExpBarrel(); // Ctrl + . 해서 별도 함수를 생성한다.
    // }
    //     }
    // }

    /*
        유니티에서 난수를 발생시키는 방법
        Random.Range(min, max)

        # 정수 Integer
            Random.Range(0, 10) => 0, 1, 2, 3, .... , 9 까지. max 값은 포함하지 않음

        # 실수 Float
            Random.Range(0.0f , 10.0f) => 0.0f ~ 10.0f max 값도 포함함.
    */

    private void ExpBarrel()
    {

        // 폭발하는 시점에 rigidbody component 넣고 물리엔진으로 터트리기
        var rb = this.gameObject.AddComponent<Rigidbody>();
        Vector3 pos = Random.insideUnitSphere;
        rb.AddExplosionForce(1500.0f, transform.position + pos, 10.0f, 1800.0f);
        // 폭발 했을 때 위로 올라가면서 효과 넣기
        Destroy(this.gameObject, 3.0f); // 터지고 나서 3초 후에 없앤다.

        var obj = Instantiate(barrelDataSO.expEffect, transform.position, Quaternion.identity);
        Destroy(obj, 5.0f);
    }

    public void OnDamaged()
    {
        ++hitCount; // hitCount += 1; 1씩 더해지면서 히트가 카운트 된다.
        if (hitCount >= 3)
        {
            // 폭발효과 여기에 길게 쓰지 않고 밑에 별도 함수 생성
            ExpBarrel(); // Ctrl + . 해서 별도 함수를 생성한다.
        }
    }
}
