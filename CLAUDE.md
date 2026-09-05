# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## 5. No Closing Colons (Korean Output)

**End Korean sentences with a period, not a colon.**

When the user writes in Korean, your output is also Korean:
- Don't end sentences with `:` even if the next line is a list or example.
- LLMs trained on English docs leak the colon habit into Korean. Catch it.
- The test: every Korean sentence terminator should be `.`, `?`, or `!` — not `:`.
- Colons are fine inside code, key-value pairs, or labels. Not as sentence enders.

## 6. File Header Comments in Korean

**First line of every new source file: a one-line Korean comment stating its role.**

When creating a new file:
- C#: `// 레이더 사용 중 좀비 미니맵 표시 시간을 관리하는 컴포넌트`
- TypeScript/JavaScript: `// 사용자 인증 상태를 관리하는 Context Provider`
- Python: `# KIS API 호출을 비동기로 래핑하는 클라이언트`
- SQL: `-- 일별 집계 결과를 저장하는 머티리얼라이즈드 뷰`
- Place it directly under required directives (`'use client'`, `'use server'`, shebang).
- Skip config files (`*.config.ts`, `package.json`, etc.).

Why: agents read files selectively, not whole codebases. A one-line Korean header gives instant context so the next session (human or agent) can navigate without re-reading the entire file.

## 7. Plan + Checklist + Context Notes

**Before any non-trivial task, produce three artifacts. Don't start coding without them.**

- **Plan** — what we're building and why.
- **Checklist** (`checklist.md`) — concrete tasks as checkboxes. Tick as you go.
- **Context Notes** (`context-notes.md`) — decisions made during the work and the reasoning behind them. Append continuously.

If the user gives only a plan and asks you to start coding, stop and ask: "Should I create the checklist and context notes first?" The next session — yours or someone else's — needs the notes to pick up where you left off without re-deriving every decision.

## 8. Run Tests Before Marking Complete

**If you touched code, run the tests before saying "done".**

- `npm test`, `pytest`, `cargo test`, whatever the project uses — run it.
- If tests pass, report results. If they fail, fix and re-run.
- No test setup? At minimum, verify the project builds/compiles.
- For Unity work, wait for script compilation, inspect the Unity Console, then run relevant EditMode/PlayMode tests or manual Play Mode acceptance checks.
- Run tests proactively, before the user signals "끝", "완료", "다 됐어" — not after.

This is the step LLMs skip most often. Treat it as non-negotiable.

## 9. Semantic Commits

**Commit when one logical change is complete. Don't wait for the user to ask.**

- The test: "Can I describe this commit in one sentence?" If yes, commit. If no, the changes are still mixed — split them.
- Good: "auth 미들웨어 추가". Bad: "auth 추가하고 UI도 고치고 버그도 수정" (split into 3).
- Don't accumulate 20 unrelated edits and lose the ability to roll back individually.
- Don't commit just to commit — meaningful units only.

Note: For solo prototypes or throwaway scripts, group commits loosely if it slows you down. The point is reversibility, not ceremony.

## 10. Read Errors, Don't Guess

**Read the actual error/log line. Don't pattern-match from memory.**

When something fails:
- Read the full error message and stack trace.
- Check the actual log output, not what you assume it should say.
- Don't apply a "common fix" before confirming the cause.
- If unclear, add a print/log to verify state — then fix.

This is the step LLMs skip most often after "run tests". They guess from error keywords and apply the most-recent-pattern fix. That's how a one-line bug becomes a three-file refactor.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

## 11. Unity Project Context

This repository extends the completed **PART 06 top-down shooter / Zombie Survivor** project into a larger single-player survival game.

### Engine and project constraints
- Use **Unity 6.3 LTS**.
- Keep the project **single-player** for now.
- Do **not** add Netcode, Mirror, Photon, Steamworks multiplayer, relay, lobby, dedicated server, or replication code unless the user explicitly requests multiplayer work.
- Preserve the project's current render pipeline, input system, scene structure, naming style, and existing gameplay scripts unless a requested feature cannot be added safely without changing them.
- Do not migrate the entire project to DOTS/ECS, Addressables, a different input system, a different render pipeline, or a new architecture just because the new system could use one.
- Never hand-edit `.unity`, `.prefab`, `.asset`, or other Unity YAML files unless there is no safer option. Prefer Unity Editor operations or narrowly scoped Editor tooling.
- Existing Zombie Survivor behavior is the baseline. Extensions must not silently break player movement, shooting, zombie AI, health, spawning, HUD, or game flow.

### Source baseline from the book
The attached book's PART 06 table of contents organizes Zombie Survivor around these existing areas.
- Chapter 14. Level art and player preparation.
- Chapter 15. Guns and shooter gameplay.
- Chapter 16. Life and zombie AI.
- Chapter 17. Final completion and post processing.

Treat those systems as the existing foundation, not as systems to rewrite from scratch.

## 12. Product Goal

Build a larger single-player survival game inspired by the broad exploration loop of Project Zomboid while keeping the current Zombie Survivor combat foundation.

The requested extension scope is limited to these systems.
- A wider explorable map.
- Drivable vehicles with enter, drive, and exit behavior.
- An inventory that can hold and use items.
- A player status UI and status data.
- A circular minimap in the top-right corner.
- Zombies are **not visible** on the minimap by default.
- A radar item can be acquired and used from inventory.
- Using the radar reveals zombie positions on the minimap for a limited duration.
- The project remains single-player during this phase.

Do not automatically add base building, crafting, hunger, thirst, sleep, weather, day/night, fuel, vehicle repair, procedural generation, quests, multiplayer, Steam integration, or save-cloud systems. Add them only after the user requests them.

## 13. Core Design Decisions

These decisions are the default unless the current project structure makes them unsafe or the user changes them.

### 13.1 Large map
- Start with the simplest scalable solution that fits the existing project.
- Keep one gameplay world scene initially.
- Divide the world into logical `WorldChunk` or `WorldZone` parents so distant decorative objects can be activated or deactivated by distance if profiling shows a need.
- Do not introduce additive-scene streaming or Addressables in the first implementation unless actual profiling proves the single-scene approach is insufficient.
- Reuse existing zombie spawning and navigation where possible.
- Expand NavMesh or the project's current navigation solution rather than replacing the AI stack.

### 13.2 Vehicles
Initial vehicle scope is intentionally small.
- Player can approach a vehicle.
- Player can enter it.
- Player can drive it.
- Player can exit it at a valid nearby position.
- Main camera and minimap follow the controlled actor correctly when entering and exiting.
- Zombies continue to exist and behave while the player is in a vehicle.

Do not add fuel, vehicle inventory, part damage, repair, keys, hot-wiring, towing, or multiple seats until requested.

### 13.3 Inventory
Use a data-driven inventory without over-engineering it.
- Reuse an existing item abstraction if the project already has one.
- Otherwise use a small `ItemData` ScriptableObject for immutable item data.
- Store runtime quantities in inventory slots, not inside shared ScriptableObject assets.
- Required operations are add, remove, inspect, select, and use.
- The radar is a normal usable inventory item, not a special hard-coded UI button.
- Avoid drag-and-drop until the basic inventory works unless the user explicitly asks for it.

### 13.4 Player status
For the first slice, preserve existing health and add only the minimum extra status needed by the current design.
- `Health` remains authoritative if it already exists.
- `Stamina` may be added if movement or vehicle design needs it.
- Hunger, thirst, fatigue, temperature, panic, infection, and similar survival stats are deferred unless requested.
- UI must read status through a stable gameplay-facing API rather than duplicating the values in UI scripts.

### 13.5 Circular minimap
Use a dedicated orthographic minimap camera rendered to a `RenderTexture` and displayed through a circular UI mask unless the project already has an equivalent minimap system.

Default behavior.
- Position the minimap in the top-right corner.
- Use a circular mask.
- The minimap is north-up for the first implementation.
- The player's marker is always visible.
- Environment/map geometry visible to the minimap is controlled by a dedicated culling mask.
- Zombie markers are never included in the normal minimap view.
- The main gameplay camera must never render minimap-only markers.

### 13.6 Radar reveal
The radar item is the only requested mechanism that reveals zombies on the minimap.

Recommended implementation.
- Every zombie prefab gets a lightweight child marker or marker component intended only for the minimap.
- Zombie minimap markers use a dedicated layer such as `MinimapZombie`.
- The minimap camera excludes `MinimapZombie` by default.
- A `MinimapRadarController` temporarily includes `MinimapZombie` in the minimap camera culling mask.
- Using a radar item starts a configurable reveal duration.
- If a second radar is used while one is active, restart the timer from the configured full duration unless the user asks for stacking behavior.
- When the timer expires, zombie markers disappear from the minimap immediately.
- Radar affects only minimap visibility. It must not alter zombie AI, aggro range, world rendering, damage, or pathfinding.

Do not scan all zombies every frame just to decide whether to draw them on the minimap. Prefer layer-based rendering or event-driven marker registration.

## 14. Harness Engineering Execution Protocol

Use a repeatable harness for every non-trivial feature. The harness exists to make Claude inspect, change, verify, and recover safely instead of making large speculative edits.

### Phase A. Inspect before editing
Before writing code for a feature.
1. Find the existing scene, scripts, prefabs, ScriptableObjects, layers, tags, and UI that the feature touches.
2. Identify the current player controller, camera, zombie prefab, zombie spawning path, health system, game manager, and input path.
3. Record exact dependencies in `context-notes.md`.
4. Compile the untouched baseline once. If the baseline already fails, report the existing error before changing code.
5. Do not rename or move existing files during inspection.

### Phase B. Plan the smallest vertical slice
Create or update `checklist.md` before coding.

Each slice must be playable by itself and have explicit acceptance checks.

Example.
```text
[ ] Add minimap camera and circular UI.
    Verify: player marker follows the player and no zombie marker is visible.
[ ] Add zombie minimap marker layer.
    Verify: marker exists but remains hidden from the minimap by default.
[ ] Add radar inventory effect.
    Verify: using radar shows zombie markers for N seconds and hides them after timeout.
```

### Phase C. Implement surgically
- Touch only files required by the current checklist item.
- Prefer serialized references over repeated scene searches.
- Avoid new global singletons unless the project already uses the same pattern and the feature truly belongs there.
- Do not refactor an existing working system merely to make the new feature look cleaner.
- If a current script is too tightly coupled, add the smallest adapter or interface needed by the requested feature.
- Match existing naming, namespace, folder, and coding conventions.

### Phase D. Verify immediately
After each logical change.
1. Let Unity recompile.
2. Read the entire Console error, including the first meaningful stack frame.
3. Fix compile errors before continuing to the next feature.
4. Run relevant EditMode or PlayMode tests when available.
5. Perform the feature's manual acceptance check in Play Mode.
6. Update `checklist.md` only after the check passes.
7. Append important discoveries and decisions to `context-notes.md`.

### Phase E. Commit one logical unit
After a slice passes verification, make one semantic commit if Git is available.

Recommended commit progression.
- `feat: 기본 원형 미니맵 추가`
- `feat: 레이더 좀비 표시 기능 추가`
- `feat: 인벤토리 아이템 사용 흐름 추가`
- `feat: 차량 탑승 및 주행 추가`
- `feat: 월드 청크 활성화 최적화 추가`

Do not combine all systems into one commit.

## 15. Required Project Artifacts

For this Unity project, non-trivial implementation work must keep these files current.

### `plan.md`
Contains the current milestone, why it exists, dependencies, risks, and acceptance criteria.

### `checklist.md`
Contains concrete checkboxes for the current vertical slice. Mark an item complete only after verification.

### `context-notes.md`
Append-only working notes for facts discovered in the project.

Record things such as.
- Actual player script path.
- Actual zombie prefab path.
- Existing health and weapon classes.
- Current input system.
- Current scene name.
- Layer and tag assignments created for minimap use.
- Any project-specific setup needed in the Inspector.
- Errors encountered and their actual cause.
- Decisions that a later Claude session must not rediscover.

Do not use `context-notes.md` as a dump of guesses. Record only verified project facts and explicit design decisions.

## 16. Recommended Vertical-Slice Order

Build in this order unless the current project structure gives a strong reason to change it.

### Slice 0. Baseline lock
Goal.
- Project opens in Unity 6.3 LTS.
- Existing Zombie Survivor scene compiles and plays.
- Existing movement, shooting, zombies, health, spawning, and HUD still work.

Do not start extensions until this baseline is verified.

### Slice 1. Circular minimap without zombie reveal
Goal.
- Circular minimap appears top-right.
- Player is represented correctly.
- Map follows player.
- No zombie appears on the minimap.
- Main camera does not show minimap markers.

This slice must work before radar code exists.

### Slice 2. Radar reveal vertical slice
Goal.
- One radar item can exist in inventory or a temporary test inventory.
- Using it consumes or triggers the radar according to current inventory conventions.
- Zombies appear on minimap for a configurable duration.
- They disappear when the duration ends.
- Reusing radar while active restarts the timer.
- Radar never changes zombie world behavior.

### Slice 3. Inventory foundation
Goal.
- Add, remove, list, select, and use items.
- Radar uses the same generic item-use path as other usable items.
- UI reflects quantity changes.
- No duplicated runtime count inside `ItemData` assets.

### Slice 4. Player status UI
Goal.
- Existing health is shown from its authoritative source.
- Add only requested extra status values.
- UI is read-only presentation of gameplay state.

### Slice 5. Vehicle vertical slice
Goal.
- One vehicle prefab can be entered, driven, and exited.
- Player control is disabled while driving and restored after exit.
- Camera and minimap follow the active controlled actor.
- Exiting does not place the player inside obstacles.

### Slice 6. Wider world
Goal.
- Expand exploration area after core systems work in the original scene.
- Keep content divided into logical world zones or chunks.
- Measure performance before adding more complex streaming.
- Do not optimize based on guesses.

This order intentionally proves the minimap/radar behavior early, before the larger map makes debugging harder.

## 17. Suggested Runtime Boundaries

Use these as responsibilities, not as mandatory class names. Reuse current equivalents when they already exist.

### Gameplay state
- Existing player controller owns movement behavior.
- Existing health component owns current health.
- `Inventory` owns slot state and quantities.
- `ItemData` owns immutable item metadata.
- `UsableItem` or the existing item-use abstraction applies an item's effect.
- `RadarItemEffect` requests a timed minimap reveal.
- `MinimapRadarController` owns only the radar reveal timer and minimap zombie-layer visibility.
- `MinimapFollow` owns minimap camera following.
- `ZombieMinimapMarker` identifies a zombie's minimap marker.
- `VehicleController` owns vehicle motion.
- `VehicleSeat` or equivalent owns enter/exit interaction.

### UI
UI classes may subscribe to gameplay changes or read exposed state, but UI must not own authoritative gameplay data.

Avoid this.
- Inventory count stored only in UI Text.
- Radar timer implemented only in a UI animation.
- Health changed directly by a HUD script.

Prefer this.
- Gameplay state changes first.
- UI observes and renders it.

## 18. Unity-Specific Safety Rules

- Add a one-line Korean role comment to the first line of every new `.cs` file, for example `// 레이더 사용 중 좀비 미니맵 표시 시간을 관리하는 컴포넌트`.
- Avoid `GameObject.Find`, `FindObjectOfType`, `FindFirstObjectByType`, and tag searches inside `Update`.
- Do not call `GetComponent` repeatedly in `Update` when the reference can be cached.
- Use `Time.deltaTime` for frame-rate independent timers.
- Keep radar duration serialized or data-driven instead of hard-coding it in multiple scripts.
- Prefer events for inventory/status UI refresh instead of rebuilding the entire UI every frame.
- Keep pooled or spawned zombie marker setup compatible with the existing zombie spawn path.
- If zombies are instantiated at runtime, verify the minimap marker exists on spawned instances, not just on a scene sample.
- Ensure minimap-only marker layers are excluded from the main camera.
- Ensure zombie markers are excluded from the minimap camera when radar is inactive.
- Avoid physics layers for purely visual marker behavior unless they are also required for physics. A render-only layer should not accidentally change collision behavior.
- Never change global Time Scale for radar duration.
- Do not use coroutines from destroyed objects as the sole source of authoritative radar state if object lifetime can end during scene transitions. Keep the timer on a stable gameplay object.

## 19. Minimap and Radar Acceptance Tests

The minimap/radar feature is complete only when all checks pass.

### Normal minimap
- Circular shape is visually correct.
- It stays in the top-right at supported game resolutions.
- Player marker is visible.
- Zombie marker is not visible before radar use.
- Main camera never shows player/zombie minimap icons floating in the world.

### Radar active
- Radar can be used through the inventory use flow.
- Zombie markers become visible immediately after use.
- New zombies spawned while radar is active also appear.
- Destroyed zombies disappear from the minimap without stale icons.
- Reveal lasts for the configured duration.
- Using another radar while active restarts the duration cleanly.

### Radar expired
- All zombie markers become hidden again.
- No zombie remains visible because of a stale renderer, layer, duplicate camera, or orphaned UI icon.
- Zombie AI and world rendering are unchanged.

## 20. Vehicle Acceptance Tests

A vehicle slice is complete only when.
- Player can enter only within the intended interaction range.
- Input transfers to the vehicle exactly once.
- Player character does not keep walking while driving.
- Camera follows the driven vehicle.
- Minimap follows the controlled actor.
- Exit restores player input.
- Exit places the player at a valid exit point.
- Repeated enter/exit does not duplicate listeners or references.
- Destroying or disabling the vehicle cannot leave the player permanently stuck in vehicle-control mode.

## 21. Inventory Acceptance Tests

Inventory is complete for this scope only when.
- Adding an item changes authoritative quantity once.
- Removing an item cannot produce a negative quantity.
- Using a consumable updates quantity once.
- Using radar activates the minimap effect once per use.
- UI refreshes after actual inventory changes, not every frame.
- Shared `ItemData` assets remain immutable at runtime for per-player quantities.

## 22. Large-Map Performance Harness

Do not assume a wide map requires a complex streaming solution. Measure first.

For each major map expansion, test.
- Average and worst frame time in a representative area.
- Active zombie count.
- Active GameObject count if a spike is observed.
- Physics cost if vehicles or many colliders are present.
- Navigation/pathfinding cost with representative zombie density.
- Minimap RenderTexture cost.

If performance remains acceptable, keep the simpler architecture.

If performance becomes unacceptable, optimize the measured bottleneck first. Possible later actions include chunk activation, object pooling, reduced update frequency, additive scenes, or Addressables, but only after evidence identifies the need.

## 23. Future Multiplayer Compatibility Without Building Multiplayer

The project is single-player now. Do not write network code.

However, avoid choices that make later multiplayer conversion unnecessarily difficult when the simpler alternative is equally easy.
- Keep player state in player-owned components instead of only static globals.
- Keep inventory operations behind methods instead of public list mutation from UI.
- Keep vehicle enter/exit as explicit state transitions.
- Keep radar as a player gameplay effect that controls that player's minimap, not as a rule that globally changes every zombie.
- Do not introduce multiplayer abstractions, network IDs, RPCs, ownership checks, or prediction code yet.

This is a boundary rule, not permission to build speculative networking infrastructure.

## 24. Definition of Done for This Expansion

A requested feature is not done because code exists. It is done only when.
- Unity compiles with no new errors.
- The relevant Play Mode scenario works.
- Existing Zombie Survivor gameplay still works.
- Acceptance checks for that slice pass.
- `checklist.md` is updated.
- `context-notes.md` contains important verified decisions.
- No unrelated files were changed.
- A logical Git commit is created and pushed to the configured GitHub remote when Git is available and the checkpoint has passed verification.
- Any Inspector or scene setup the user must perform is documented clearly.

## 25. Do Not Guess About the Existing Project

When the user later provides the actual Unity project, inspect it before naming concrete existing scripts or paths.

Do not assume the project contains classes with names such as `PlayerController`, `Zombie`, `GameManager`, `Inventory`, or `LivingEntity` merely because the book uses or may use similar terminology. Search the repository and use the names that actually exist.

If the actual project conflicts with this design, preserve the working project and adapt the smallest part of this plan rather than forcing the project to match this document.

## 26. GitHub Progress Checkpoint Commit + Push Harness

**Every verified progress checkpoint must be committed and pushed to GitHub before starting the next logical checkpoint.**

A progress checkpoint means one coherent unit of work that can be described in one sentence and whose acceptance checks have passed. Examples include completing one checklist item, finishing one vertical slice, or fixing one independently verifiable bug. Do not create a commit for every file save or every tiny edit.

### 26.1 Pre-commit gate
Before committing a checkpoint, run this gate in order.
1. Finish the current checklist item or vertical slice.
2. Let Unity finish script compilation.
3. Confirm there are no new Console compile errors.
4. Run the relevant EditMode, PlayMode, build, or manual acceptance checks.
5. Update `checklist.md` with the verified result.
6. Append verified decisions, paths, setup, and important errors to `context-notes.md`.
7. Inspect `git status --short` and confirm every changed file belongs to the current checkpoint.
8. Inspect the diff before staging. Do not silently include unrelated edits, secrets, generated caches, or local machine files.
9. Run `git diff --check` and fix whitespace/conflict-marker problems before committing.

If the checkpoint has not passed its verification gate, do not commit it as completed work. Keep fixing or clearly report the blocker.

### 26.2 Staging rule
- Stage only files that belong to the current logical checkpoint.
- Prefer explicit paths with `git add <path>` when unrelated working-tree changes exist.
- `git add -A` is allowed only after verifying that all current changes belong to the same checkpoint.
- Never add Unity-generated folders or machine-local files that should be ignored, such as `Library/`, `Temp/`, `Logs/`, `obj/`, IDE caches, or build output, unless the repository intentionally tracks a specific file there.
- Never commit credentials, API keys, Steam secrets, access tokens, `.env` secrets, private certificates, or local authentication files.

### 26.3 Commit rule
After the pre-commit gate passes, create one semantic commit for the checkpoint.

Recommended forms.
- `feat: 기본 원형 미니맵 추가`
- `feat: 레이더 좀비 표시 기능 추가`
- `feat: 인벤토리 아이템 사용 흐름 추가`
- `feat: 차량 탑승 및 주행 추가`
- `fix: 레이더 종료 후 좀비 아이콘 잔존 수정`
- `docs: 체크리스트와 컨텍스트 노트 갱신`

Rules.
- The commit subject must describe only the completed checkpoint.
- Do not mix unrelated features in one commit.
- Do not amend or rewrite an earlier pushed commit unless the user explicitly asks.
- Do not use `--no-verify` to bypass repository hooks unless the user explicitly requests it and the reason is documented.

### 26.4 Push rule
Immediately after a successful checkpoint commit, push the current checked-out branch to the configured GitHub remote before starting the next checkpoint.

Before the first push in a session, inspect.
```bash
git branch --show-current
git remote -v
```

Default behavior.
- Keep working on the currently checked-out branch.
- Do not switch branches, merge into `main`, create a pull request, or change repository remotes unless requested.
- If the current branch already has an upstream, use `git push`.
- If it has no upstream and `origin` is the intended GitHub remote, use `git push -u origin <current-branch>`.
- Never use `git push --force` or `git push --force-with-lease` automatically.
- Never push another local branch just because it exists.
- Never push tags unless the user explicitly requests a tag or release.

### 26.5 Push failure recovery
If push fails, read the exact Git output before taking action.

Handle failures conservatively.
- Authentication or permission failure → keep the local commit, stop automatic Git operations, and report the exact failure and the current commit hash.
- Remote or upstream is missing → keep the local commit and report what is missing. Do not invent a remote URL.
- Non-fast-forward rejection → do not force-push and do not rewrite history automatically. Fetch the remote, inspect the divergence, and report or resolve only when the safe resolution is clear.
- Merge/rebase conflict → do not discard either side. Record the conflict in `context-notes.md`, show the conflicted files, and resolve only with verified project intent.
- Repository hook failure → read the full hook output, fix the actual issue, re-run verification, then commit/push again. Do not bypass the hook by default.

A failed push does not erase a verified local commit. Preserve the commit and make the blocker explicit.

### 26.6 Progress report after every checkpoint
After a successful push, report a compact checkpoint summary containing.
- Completed checklist item or slice.
- Verification performed and result.
- Commit subject.
- Short commit hash from `git rev-parse --short HEAD`.
- Current branch.
- Push result.
- Next checklist item.

Example.
```text
Checkpoint complete.
- Completed: 원형 미니맵 기본 표시.
- Verified: Unity compile OK, Play Mode player-follow OK, zombies hidden.
- Commit: feat: 기본 원형 미니맵 추가.
- Commit: a1b2c3d.
- Branch: feature/survival-expansion.
- Push: origin에 push 완료.
- Next: 레이더 사용 시 좀비 마커 임시 표시.
```

### 26.7 Session-end Git gate
Before ending a work session.
1. Run `git status --short`.
2. Confirm no completed verified checkpoint is left only in the working tree.
3. Commit and push any completed checkpoint that passed verification.
4. Do not create a fake "WIP complete" commit for broken or unverified code just to make the tree clean.
5. If unfinished work must remain local, record its exact state and blocker in `context-notes.md` and report that it was intentionally not pushed as completed work.

The target rhythm is.
```text
inspect → plan → implement → compile/test → acceptance check → update notes → review diff → commit → push → report → next checkpoint
```

This commit-and-push loop is part of the harness, not optional cleanup at the end of the project.

