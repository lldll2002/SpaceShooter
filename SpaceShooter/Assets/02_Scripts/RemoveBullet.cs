using Unity.Mathematics;
using UnityEngine;



public class RemoveBullet : MonoBehaviour
{
    // public GameObject sparkEffect;
    [SerializeField] private EffectDataSO effectDataSO;



    void OnCollisionEnter(Collision coll) // 충돌 정보들이 넘어온다. coll 이라는 파라메터로 지정해서 받는다.
    {


        // 충돌한 물체를 파악

        // if (coll.collider.tag == "BULLET") // gameObject 로 써도 되는데 왜 collider.tag 썼나? 밑에 추가 할 코드 때문에.
        // {
        //     Destroy(coll.gameObject); // (와서 충돌한 게임오브젝트)를 Destroy한다.
        //     // 사용금지 !!! 태그가 갖고있는 값을 별도로 할당하고, 불필요한 메모리를 사용하게 됨.
        // }
        if (coll.collider.CompareTag("BULLET"))
        {
            // 충돌 정보
            ContactPoint cp = coll.GetContact(0); // 몇 번째 충돌 값을 가져올 거냐? 0번째.
            // 충돌 좌표
            Vector3 _point = cp.point; // 3차원 상의 포인트 헷갈리지 않게 _ 붙였다.
            // 법선 벡터
            // Vector3 _normal = cp.normal; // 충돌한 지점의 90도 각도 지점
            // 이대로 입력하면 벽에 부딪힌 스피어의 90도 각도의 벡터 스파크가 반대쪽으로 튀기 때문에 잘못 된 것.수정필요
            Vector3 _normal = -cp.normal; // 
                                          // 벡터의 방향을 반대로 바꿔줘야 하기 때문에 (-)를 붙여줌.

            // 법선 벡터가 가르키는 방향의 각도(quaternion)을 계산
            Quaternion rot = Quaternion.LookRotation(_normal);
            // 벡터가 바라보는 방향을 쿼터니언 타입으로 변환한다. 

            // 스파크 이펙트 생성
            GameObject obj = Instantiate(effectDataSO.sparkEffect, _point, rot);
            // 오리지널 값, 좌표값, 쿼터니언 타입
            Destroy(obj, 0.4f);

            // Destroy(coll.gameObject);
            PoolManager.Instance.bulletPool.Release(coll.gameObject.GetComponent<Bullet>());

        }
    }
    // 충돌 콜백 함수
    /*
        1. 양쪽 다 Collider Component 갖고 있어야 한다.
        2. 이동하는 GameObject에 반드시 Rigidbody Component 있어야 한다.

        # IsTrigger 언체크 된 경우
            OnCollisionEnter 
            OnCollisionStay 
            OnCollisionExit

        # IsTrigger 체크 된 경우
            OnTriggerEnter 
            OnTriggerStay 
            OnTriggerExit
        ...가 나타난다. 
    */

}
