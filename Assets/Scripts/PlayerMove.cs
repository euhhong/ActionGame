using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 위치(좌표)만 앞/뒤/좌/우로 이동시킨다.
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private static readonly int WalkGo = Animator.StringToHash("Walkgo"); // Animator 파라미터 이름과 대소문자까지 같아야 함
    private Animator animator;

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

        transform.position += move.normalized * moveSpeed * Time.deltaTime;

        // 움직이는 중이면 Walkgo = 3 (Walking), 멈추면 0 (Idle)
        if (animator != null)
            animator.SetInteger(WalkGo, move != Vector3.zero ? 3 : 0);
    }
}
