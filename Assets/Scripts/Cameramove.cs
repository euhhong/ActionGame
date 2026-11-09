using UnityEngine;

// 카메라에 붙인다. 캐릭터(PlayerMove)를 찾아서 처음 거리를 유지하며 따라간다.
public class Cameramove : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    private void Start()
    {
        PlayerMove player = FindAnyObjectByType<PlayerMove>();
        if (player == null) return;

        target = player.transform;
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        // 캐릭터가 이동한 뒤에 카메라를 옮긴다
        if (target != null)
            transform.position = target.position + offset;
    }
}
