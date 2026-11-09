using UnityEngine;
using UnityEngine.InputSystem;

// 카메라에 붙인다. 캐릭터(PlayerMove)를 찾아서 처음 거리를 유지하며 따라간다.
// 마우스를 좌우로 움직이면(버튼 안 눌러도) 캐릭터를 중심으로 시점이 좌우로 돈다.
public class Cameramove : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 0.15f; // 마우스 1픽셀당 회전 각도

    private Transform target;
    private Vector3 offset;           // 시작할 때 캐릭터 → 카메라 거리
    private Quaternion startRotation; // 시작할 때 카메라 방향
    private float yaw;                // 지금까지 좌우로 돈 각도

    private void Start()
    {
        PlayerMove player = FindAnyObjectByType<PlayerMove>();
        if (player == null) return;

        target = player.transform;
        offset = transform.position - target.position;
        startRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 마우스 좌우 이동량만큼 회전 각도를 더한다
        if (Mouse.current != null)
            yaw += Mouse.current.delta.ReadValue().x * mouseSensitivity;

        // 캐릭터를 중심으로 offset을 yaw만큼 돌려서 카메라 위치/방향 결정
        Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
        transform.position = target.position + turn * offset;
        transform.rotation = turn * startRotation;
    }
}
