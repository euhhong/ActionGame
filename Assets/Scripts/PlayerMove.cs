using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 위치(좌표)만 앞/뒤/좌/우로 이동시킨다.
// 이동키만 누르면 걷기, Shift를 누르고 있는 동안은 달리기, 이동키를 떼면 멈춘다.
// 스페이스바를 누르면 점프한다. 점프 모션이 끝나는 순간 땅에 닿도록 높이를 계산한다.
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    // 이동키를 바꿔 누를 때 생기는 아주 짧은 빈틈 동안은 멈춘 걸로 치지 않는다
    [SerializeField] private float keySwitchGrace = 0.15f;
    [SerializeField] private float jumpHeight = 1.5f; // 점프 최고 높이

    private static readonly int WalkGo = Animator.StringToHash("Walkgo"); // Animator 파라미터 이름과 대소문자까지 같아야 함
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private const string JumpStateName = "Jump";
    private const int IdleValue = 0;
    private const int WalkValue = 3;
    private const int RunValue = 5;

    private Animator animator;
    private float lastMoveTime = float.NegativeInfinity;

    private bool isJumping;
    private float jumpTime;          // 점프 시작 후 지난 시간
    private float jumpDuration = 1f; // 점프 모션 길이 (Jump 상태에 들어가면 실제 길이로 바뀜)
    private float groundY;           // 점프 시작할 때 높이

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

        if (move != Vector3.zero) lastMoveTime = Time.time;

        // 이동키를 떼도 잠깐(keySwitchGrace)은 이동 중으로 본다 → 키 바꿀 때 Idle로 끊기지 않음
        bool isMoving = Time.time - lastMoveTime <= keySwitchGrace;
        bool isRunning = kb.shiftKey.isPressed; // Shift를 누르고 있는 동안만 달리기

        float speed = isRunning ? runSpeed : moveSpeed;
        transform.position += move.normalized * speed * Time.deltaTime;

        // 스페이스바: 땅에 있을 때만 점프 시작
        if (kb.spaceKey.wasPressedThisFrame && !isJumping)
            StartJump();

        if (isJumping)
            UpdateJump();

        // Idle = 0, Walk = 3, Run = 5
        if (animator != null)
            animator.SetInteger(WalkGo, !isMoving ? IdleValue : isRunning ? RunValue : WalkValue);
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTime = 0f;
        groundY = transform.position.y;
        if (animator != null) animator.SetTrigger(JumpTrigger);
    }

    private void UpdateJump()
    {
        // Jump 상태로 들어가면 실제 모션 길이를 읽어서 체공 시간으로 쓴다
        if (animator != null)
        {
            AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            if (next.IsName(JumpStateName)) jumpDuration = next.length;
            else if (current.IsName(JumpStateName)) jumpDuration = current.length;
        }

        jumpTime += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTime / jumpDuration);

        // 포물선: 시작(t=0)과 끝(t=1)에서 높이 0, 중간(t=0.5)에서 jumpHeight
        float height = 4f * jumpHeight * t * (1f - t);

        Vector3 pos = transform.position;
        pos.y = groundY + height;
        transform.position = pos;

        // 모션이 끝나는 순간 착지
        if (t >= 1f) isJumping = false;
    }
}
