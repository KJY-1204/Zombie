// 플레이어가 보유한 아이템 슬롯과 수량을 관리하는 인벤토리
using UnityEngine;

public class Inventory : MonoBehaviour {
    // 인벤토리 한 칸의 상태(아이템 데이터 + 보유 수량)
    [System.Serializable]
    public class Slot {
        public ItemData data; // 슬롯에 담긴 아이템 데이터, 비어있으면 null
        public int quantity; // 보유 수량
    }

    public Slot[] slots = new Slot[4]; // 고정 크기 슬롯(지금은 체력팩/레이더 2종만 사용)

    private PlayerInput playerInput; // 슬롯 사용 입력을 읽어올 컴포넌트

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();

        // 모든 슬롯을 빈 상태로 초기화
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new Slot();
        }
    }

    private void Start() {
        // 시작 시 빈 인벤토리 상태를 UI에 반영
        RefreshUI();
    }

    private void Update() {
        // 숫자키 1, 2로 각각 0번, 1번 슬롯을 즉시 사용
        if (playerInput.useSlot1)
        {
            UseSlot(0);
        }
        else if (playerInput.useSlot2)
        {
            UseSlot(1);
        }
    }

    // 아이템을 인벤토리에 추가. 이미 보유한 아이템이면 수량만 증가, 아니면 빈 슬롯에 배치
    public void Add(ItemData data, int amount) {
        // 같은 아이템을 이미 가지고 있는 슬롯 찾기
        foreach (Slot slot in slots)
        {
            if (slot.data == data)
            {
                slot.quantity += amount;
                RefreshUI();
                return;
            }
        }

        // 빈 슬롯 찾기
        foreach (Slot slot in slots)
        {
            if (slot.data == null)
            {
                slot.data = data;
                slot.quantity = amount;
                RefreshUI();
                return;
            }
        }

        // 빈 슬롯이 없으면 아이템을 담지 못함 (인벤토리 가득 참)
    }

    // 지정한 인덱스의 슬롯에 담긴 아이템을 사용
    public void UseSlot(int index) {
        if (index < 0 || index >= slots.Length) return;

        Slot slot = slots[index];
        if (slot.data == null || slot.quantity <= 0) return;

        // 아이템 데이터가 정의한 효과 실행
        slot.data.Use(gameObject);

        // 사용한 만큼 수량 차감, 0이 되면 슬롯을 비움
        slot.quantity--;
        if (slot.quantity <= 0)
        {
            slot.data = null;
            slot.quantity = 0;
        }

        RefreshUI();
    }

    // 인벤토리 상태를 UI 텍스트로 갱신
    private void RefreshUI() {
        if (UIManager.instance == null) return;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].data != null)
            {
                sb.Append(i + 1).Append(":").Append(slots[i].data.displayName)
                    .Append(" x").Append(slots[i].quantity).Append("  ");
            }
        }

        UIManager.instance.UpdateInventoryText(sb.ToString());
    }
}
