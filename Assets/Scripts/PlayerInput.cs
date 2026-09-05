using UnityEngine;

// 플레이어 캐릭터를 조작하기 위한 사용자 입력을 감지
// 감지된 입력값을 다른 컴포넌트들이 사용할 수 있도록 제공
public class PlayerInput : MonoBehaviour {
    public string moveAxisName = "Vertical"; // 앞뒤 움직임을 위한 입력축 이름 (WASD의 W/S)
    public string strafeAxisName = "Horizontal"; // 좌우 움직임을 위한 입력축 이름 (WASD의 A/D)
    public string fireButtonName = "Fire1"; // 발사를 위한 입력 버튼 이름
    public string reloadButtonName = "Reload"; // 재장전을 위한 입력 버튼 이름
    public LayerMask groundLayer = ~0; // 마우스 조준 위치를 계산할 때 사용할 바닥 레이어

    // 값 할당은 내부에서만 가능
    public float move { get; private set; } // 감지된 앞뒤 움직임 입력값
    public float strafe { get; private set; } // 감지된 좌우 움직임 입력값
    public bool fire { get; private set; } // 감지된 발사 입력값
    public bool reload { get; private set; } // 감지된 재장전 입력값
    public bool useSlot1 { get; private set; } // 인벤토리 1번 슬롯 사용 입력값
    public bool useSlot2 { get; private set; } // 인벤토리 2번 슬롯 사용 입력값
    public bool toggleStatus { get; private set; } // 상태 창을 여닫는 입력값
    public bool toggleMap { get; private set; } // 전체 지도 창을 여닫는 입력값
    public Vector3 mouseWorldPosition { get; private set; } // 마우스 커서가 가리키는 바닥 위 월드 좌표

    // 매프레임 사용자 입력을 감지
    private void Update() {
        // 게임오버 상태에서는 사용자 입력을 감지하지 않는다
        if (GameManager.instance != null
            && GameManager.instance.isGameover)
        {
            move = 0;
            strafe = 0;
            fire = false;
            reload = false;
            useSlot1 = false;
            useSlot2 = false;
            toggleStatus = false;
            toggleMap = false;
            return;
        }

        // move/strafe에 관한 입력 감지 (WASD)
        move = Input.GetAxis(moveAxisName);
        strafe = Input.GetAxis(strafeAxisName);
        // fire에 관한 입력 감지
        fire = Input.GetButton(fireButtonName);
        // reload에 관한 입력 감지
        reload = Input.GetButtonDown(reloadButtonName);
        // 인벤토리 슬롯 사용 입력 감지 (숫자키 1, 2)
        useSlot1 = Input.GetKeyDown(KeyCode.Alpha1);
        useSlot2 = Input.GetKeyDown(KeyCode.Alpha2);
        // 상태 창 토글 입력 감지 (I 키)
        toggleStatus = Input.GetKeyDown(KeyCode.I);
        // 전체 지도 창 토글 입력 감지 (M 키)
        toggleMap = Input.GetKeyDown(KeyCode.M);

        // 마우스 조준 위치 갱신
        UpdateMouseWorldPosition();
    }

    // 마우스 커서 위치에서 바닥으로 레이캐스트하여 조준 지점을 계산
    private void UpdateMouseWorldPosition() {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            mouseWorldPosition = hit.point;
        }
        else
        {
            // 바닥에 레이가 닿지 않으면 플레이어 높이의 평면과 교차한 지점을 사용
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            if (groundPlane.Raycast(ray, out float enter))
            {
                mouseWorldPosition = ray.GetPoint(enter);
            }
        }
    }
}