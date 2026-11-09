using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 위치(좌표)만 앞/뒤/좌/우로 이동시킨다.
// 이동키만 누르면 걷기, Shift를 누르고 있는 동안은 달리기, 이동키를 떼면 멈춘다.
// 스페이스바를 누르면 점프 모션 시작. 무릎 굽히는 동안은 땅에 있다가 takeOff 지점부터 떠오르고,
// landing 지점에 땅에 닿는다. 올라갈 때는 일정한 속도, 꼭대기를 찍자마자 멈칫 없이 빠른 일정 속도로 떨어진다.
// 떨어지는 구간과 착지 후 동작은 모션 자체도 빠르게 재생한다.
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    // 이동키를 바꿔 누를 때 생기는 아주 짧은 빈틈 동안은 멈춘 걸로 치지 않는다
    [SerializeField] private float keySwitchGrace = 0.15f;
    [SerializeField] private float jumpHeight = 1.5f; // 점프 최고 높이
    // 점프 모션 전체를 0~1로 봤을 때, 발이 땅에서 떨어지는 지점과 다시 닿는 지점
    // (0~takeOff: 무릎 굽히는 준비 동작, 땅에 그대로 / landing~1: 착지 후 동작, 땅에 그대로)
    [SerializeField, Range(0f, 1f)] private float takeOff = 0.3f;
    [SerializeField, Range(0f, 1f)] private float landing = 0.8f;
    // 모션의 공중 구간 중 꼭대기 지점 비율 (앞은 올라가는 모션, 뒤는 떨어지는 모션)
    [SerializeField, Range(0.3f, 0.8f)] private float riseShare = 0.5f;
    // 떨어지는 구간 배속 (모션과 낙하 둘 다, 클수록 빨리 떨어짐)
    [SerializeField] private float fallSpeedMultiplier = 2f;
    // 착지 후 동작 재생 속도 배율 (1 = 원래 속도)
    [SerializeField] private float landingAnimSpeed = 1.6f;

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
    private bool jumpDurationFound;  // 실제 길이를 한 번 읽었으면 점프 끝까지 고정
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
        jumpDurationFound = false;
        groundY = transform.position.y;
        if (animator != null) animator.SetTrigger(JumpTrigger);
    }

    private void UpdateJump()
    {
        // Jump 상태로 들어가면 실제 모션 길이를 딱 한 번 읽어서 체공 시간으로 고정한다
        // (점프 도중에 길이가 바뀌면 속도가 달라지므로)
        if (!jumpDurationFound && animator != null)
        {
            AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            if (next.IsName(JumpStateName)) { jumpDuration = next.length; jumpDurationFound = true; }
            else if (current.IsName(JumpStateName)) { jumpDuration = current.length; jumpDurationFound = true; }
        }

        // 구간별 배속: 떨어지는 구간과 착지 후 구간은 모션을 빠르게 재생하고, 시간도 같은 배율로 흐르게 해서 맞춘다
        float peak = takeOff + (landing - takeOff) * riseShare; // 꼭대기 지점
        float before = jumpTime / jumpDuration;
        bool inLandingPhase = before >= landing;
        bool inFallPhase = before >= peak && !inLandingPhase;
        float animSpeed = inLandingPhase ? landingAnimSpeed : inFallPhase ? fallSpeedMultiplier : 1f;
        jumpTime += Time.deltaTime * animSpeed;
        float t = Mathf.Clamp01(jumpTime / jumpDuration);

        // 공중에 떠 있는 구간(takeOff ~ landing)에서만 높이를 준다
        float height = 0f;
        if (t > takeOff && t < landing && landing > takeOff)
        {
            if (t < peak)
            {
                // 올라갈 때: 일정한 속도
                height = jumpHeight * (t - takeOff) / (peak - takeOff);
            }
            else
            {
                // 떨어질 때: 꼭대기에서 멈칫 없이 바로 빠른 일정 속도로 땅까지 (완충/감속 없음)
                height = jumpHeight * (landing - t) / (landing - peak);
            }
        }

        Vector3 pos = transform.position;
        pos.y = groundY + height;
        transform.position = pos;

        // 떨어지는 구간·착지 후 동작은 빠르게 재생. Jump 상태를 빠져나가기 시작하면 원래 속도로
        bool playingJump = true;
        if (animator != null)
        {
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            bool nextIsJump = animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(JumpStateName);
            bool leaving = animator.IsInTransition(0) && !nextIsJump;
            playingJump = nextIsJump || (current.IsName(JumpStateName) && !leaving);
            animator.speed = playingJump ? animSpeed : 1f; // 떨어지는/착지 구간은 모션도 같은 배속
        }

        // 모션이 끝났거나, 착지 후 Animator가 이미 다음 동작으로 넘어갔으면 점프 종료
        if (t >= 1f || (inLandingPhase && !playingJump))
        {
            isJumping = false;
            if (animator != null) animator.speed = 1f;
        }
    }
}
