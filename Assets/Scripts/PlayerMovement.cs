using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float rotateSpeed = 180f; // 좌우 회전 속도

    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터
    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디

    private void Start() {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate() {
        // 마우스 조준 방향으로 회전 (이동 방향 계산보다 먼저 실행되어야 함)
        // 방금 계산한 정면 방향을 반환값으로 바로 재사용 — Rigidbody 회전 직후
        // Transform과의 동기화 시점에 의존하지 않기 위함
        Vector3 facingDirection = RotateTowardsMouse();
        // WASD 입력에 따른 이동 실행
        Vector3 moveDirection = CalculateMoveDirection();
        Move(moveDirection);

        // 이동 방향이 캐릭터가 바라보는 방향과 얼마나 일치하는지(전진 +, 후진 -)를
        // 애니메이터의 Move 파라미터에 반영. 순수 좌우 이동(스트레이프)일 때는
        // 내적이 0에 가까워져 제자리 걸음처럼 보이는 어색함을 줄여준다.
        float facingRelativeSpeed = Vector3.Dot(moveDirection, facingDirection);
        playerAnimator.SetFloat("Move", facingRelativeSpeed);
    }

    // 카메라의 수평 방향(높이 성분 제거) 기준으로 WASD 입력의 이동 방향을 계산
    private Vector3 CalculateMoveDirection() {
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDirection =
            camForward * playerInput.move + camRight * playerInput.strafe;
        // 대각선 이동 시 속도가 더 빨라지지 않도록 보정
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
        return moveDirection;
    }

    // 계산된 방향으로 캐릭터를 이동
    private void Move(Vector3 moveDirection) {
        // 상대적으로 이동할 거리 계산
        Vector3 moveDistance = moveDirection * moveSpeed * Time.deltaTime;
        // 리지드바디를 통해 게임 오브젝트 위치 변경
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance);
    }

    // 마우스가 가리키는 지점을 바라보도록 캐릭터를 회전하고, 갱신된 정면 방향을 반환
    private Vector3 RotateTowardsMouse() {
        Vector3 lookDirection = playerInput.mouseWorldPosition - playerRigidbody.position;
        lookDirection.y = 0f;

        // 마우스가 캐릭터 위치와 거의 겹치면 회전하지 않고 현재 정면 방향을 그대로 반환
        if (lookDirection.sqrMagnitude < 0.0001f) return playerRigidbody.rotation * Vector3.forward;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        Quaternion newRotation = Quaternion.RotateTowards(
            playerRigidbody.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        // 리지드바디를 통해 게임 오브젝트 회전 변경
        playerRigidbody.rotation = newRotation;
        return newRotation * Vector3.forward;
    }
}