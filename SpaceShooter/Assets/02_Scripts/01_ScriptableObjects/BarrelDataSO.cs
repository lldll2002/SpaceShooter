using UnityEngine;

[CreateAssetMenu(fileName = "BarrelDataSO", menuName = "Scriptable Objects/BarrelDataSO")]
public class BarrelDataSO : ScriptableObject
{
    public GameObject expEffect;
    public Texture[] textures; // 기존에 사용했던 것으로 쓰면 수정할 때 편하다
}
