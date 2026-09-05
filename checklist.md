# Checklist — 맵 확장 3차: Blender로 농장 소품 재제작 (사용자 "엉성해 보인다" 피드백)

## CP9. Blender 파이프라인 확인
- [x] `C:\Program Files\Blender Foundation\Blender 5.2\blender.exe` 설치 확인, `--background --python` 헤드리스 실행 가능 확인.
- [x] 스크래치패드에 `build_rural_props.py` 작성 — bpy/bmesh로 헛간(진짜 박공지붕 프리즘)/사일로(원뿔 지붕)/나무(불규칙 아이코스피어 수관)/건초더미(베벨 처리)/목재 울타리(기둥+가로대 2단)를 절차적으로 생성 후 각각 FBX로 내보냄.
      Verify: 헤드리스 실행 로그에서 5개 FBX 전부 "export finished" 확인, 에러 없음.

## CP10. Unity 임포트 및 기존 프리미티브 교체
- [x] 5개 FBX를 `Assets/Models/Rural/`로 복사 후 `refresh_unity`로 임포트, 콘솔 에러 0건 확인.
- [x] 기존 프리미티브 기반 오브젝트 25개(Barn Body/Roof, Silo Body/Roof, Pasture Fence 1~7(철제 묘지 울타리 재활용본), Tree 01~11, Hay Bale 1~3) 전부 삭제.
- [x] 새 FBX 모델을 동일한 위치/스케일/회전으로 재배치(나무는 기존 변주값 그대로 재사용, 목재 울타리는 세그먼트 폭이 달라져(2.4 vs 2) 6개로 재계산해 비슷한 길이를 커버).
- [x] 신규 재질(`Fence Wood.mat` 1개 추가) 포함 총 7종 재질을 각 서브파츠에 재할당(53건).
      Verify: 45도/탑뷰 스크린샷으로 실제 박공지붕 실루엣, 원뿔형 사일로 지붕, 불규칙한 나무 수관, 목재(철제 아님) 울타리를 육안 확인 — 이전 프리미티브 버전 대비 확연히 자연스러움.

## CP11. 충돌/NavMesh 재반영
- [x] 헛간(BoxCollider)·사일로(CapsuleCollider)·나무 11그루(각 Trunk에 CapsuleCollider)에 콜라이더 + `NavMeshModifier`(Not Walkable) 추가(건초더미/울타리는 기존 묘지 시각용 울타리와 동일하게 콜라이더 생략 — 프로젝트 기존 관례).
- [x] `NavMeshSurface.BuildNavMesh()` 재실행 + 에셋 재저장(지난 두 차례와 동일한 함정 반복 방지 위해 매번 `AssetDatabase.DeleteAsset`+`CreateAsset`+`SaveAssets`로 명시적 저장).
      Verify: Play Mode에서 헛간 중심(18,0,18)과 나무 트렁크(8,0,22) 둘 다 반경 내 NavMesh 샘플링 실패(=차단됨) 확인. 플레이어를 헛간 옆으로 이동해도 정상 서 있음. 콘솔 에러 0건.

## CP12. 마무리
- [x] `manage_scene save`, `git status`로 변경 파일이 `Main.unity`/`NavMesh-Navigation.asset`/`Fence Wood.mat`/`Assets/Models/Rural/*`로만 국한됨을 확인.
