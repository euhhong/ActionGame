using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 위치(좌표)만 앞/뒤/좌/우로 이동시킨다.
// 이동 중 Shift를 누르면 달리기, 달리는 중 다시 Shift를 누르면 걷기로 바뀐다.
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    private static readonly int WalkGo = Animator.StringToHash("Walkgo"); // Animator 파라미터 이름과 대소문자까지 같아야 함
    private const int IdleValue = 0;
    private const int WalkValue = 3;
    private const int RunValue = 5;

    private Animator animator;
    private bool isRunning;

    private void Awake()
    {
        // 캐릭터 모델이 자식 오브젝트에 있어도 찾을 수 있게 자식까지 검색
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        Vector3 move = Vector3.zero;
        if (kb.wKey.isPressed) move += Vector3.forward;
        if (kb.sKey.isPressed) move += Vector3.back;
        if (kb.aKey.isPressed) move += Vector3.left;
        if (kb.dKey.isPressed) move += Vector3.right;

        bool isMoving = move != Vector3.zero;

        // 이동 중 Shift를 누를 때마다 걷기/달리기 전환, 멈추면 다시 걷기 상태로
        if (!isMoving)
            isRunning = false;
        else if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
            isRunning = !isRunning;

        float speed = isRunning ? runSpeed : moveSpeed;
        transform.position += move.normalized * speed * Time.deltaTime;

        // Idle = 0, Walk = 3, Run = 5
        if (animator != null)
            animator.SetInteger(WalkGo, !isMoving ? IdleValue : isRunning ? RunValue : WalkValue);
    }
}
