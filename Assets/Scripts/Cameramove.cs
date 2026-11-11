using UnityEngine;
using UnityEngine.InputSystem;

// 카메라에 붙인다. 캐릭터(PlayerMove)를 찾아서 뒤쪽 위에서 일정 거리를 두고 따라간다.
// 마우스를 좌우로 움직이면(버튼 안 눌러도) 캐릭터를 중심으로 시점이 좌우로 돈다.
// 왼쪽 클릭하면 시점 회전이 멈추고, 다시 클릭하면 다시 돈다.
public class Cameramove : MonoBehaviour
{
    [SerializeField] private float distance = 7f;            // 캐릭터 뒤로 떨어진 거리
    [SerializeField] private float height = 3.5f;            // 캐릭터보다 위로 올라간 높이
    [SerializeField] private float lookHeight = 1.2f;        // 캐릭터의 어느 높이를 바라볼지 (가슴 정도)
    [SerializeField] private float rotateSensitivity = 0.04f; // 마우스 1픽셀당 회전 각도 (작을수록 천천히 돈다)

    private Transform target;
    private float yaw;            // 지금까지 좌우로 돈 각도
    private bool rotateLocked;    // 클릭하면 true → 시점 회전 멈춤, 다시 클릭하면 false → 다시 회전

    private void Start()
    {
        PlayerMove player = FindAnyObjectByType<PlayerMove>();
        if (player != null) target = player.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            // 왼쪽 클릭할 때마다 시점 회전 멈춤 / 다시 회전 전환
            if (mouse.leftButton.wasPressedThisFrame)
                rotateLocked = !rotateLocked;

            // 멈춘 상태가 아니면 마우스 좌우 이동량만큼 회전 각도를 더한다
            if (!rotateLocked)
                yaw += mouse.delta.ReadValue().x * rotateSensitivity;
        }

        // 캐릭터 뒤쪽 위 (yaw만큼 캐릭터 둘레를 돈 위치)에 카메라를 두고 캐릭터를 바라본다
        Vector3 offset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, height, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
