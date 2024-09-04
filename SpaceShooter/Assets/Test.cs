using UnityEngine;

public class Test : MonoBehaviour
{
    private Rigidbody rb; // 컴포넌트를 저장할 임시함수
    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // 파라메터 없음
    }

    // 총알이 이상한 방향으로 날아가는 것을 고치는 함수
    public void Shoot()
    {
        //rb.linearVelocity = rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.LookRotation(transform.forward); // 처음에 생성 후에 위치와 각도를 틀었으니까, 그 입력

        rb.AddRelativeForce(Vector3.forward * 800.0f);
        //Relative는 자기 좌표, 그냥은 글로벌좌표의 힘을 받음. (벡터의, 전진방향, 속도로)
    }

    // void OnCollisionEnter(Collision coll)
    // {

    //     InitItem();
    //     TestPool.Instance.bulletPool.Release(this);
    // }

    // void InitItem()
    // {
    //     this.transform.position = Vector3.zero;
    //     this.transform.rotation = Quaternion.identity;
    //     // GetComponent<TrailRenderer>().Clear();
    //     rb.linearVelocity = rb.angularVelocity = Vector3.zero;
    // }


}
