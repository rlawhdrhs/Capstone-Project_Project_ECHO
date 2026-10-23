# Project ECHO

**VR Asymmetric Stealth & Chase Game**

Unity 기반 1:1 VR 비대칭 멀티플레이 게임입니다.

Project ECHO는 잠입자와 추격자가 서로 다른 정보와 행동 수단을 활용해 대결하는 게임입니다. 소리를 통한 정보 전달과 센서 로봇의 시점 전환을 결합하여, 탐색과 추론을 중심으로 한 추격 경험을 구현했습니다.

---

## 01. Game Overview

| 항목 | 내용 |
|---|---|
| 프로젝트명 | Project ECHO |
| 장르 | VR 비대칭 잠입·추격 게임 |
| 플랫폼 | PC VR |
| 플레이 인원 | 2인 (1 vs 1) |
| 개발 기간 | 2026.03 ~ 2026.06 |
| 개발 형태 | 세종대학교 캡스톤디자인 팀 프로젝트 |
| 게임 엔진 | Unity |
| 개발 언어 | C# |
| 네트워크 | Photon Fusion 2 |

## 02. Gameplay

### Infiltrator — 잠입자

연구 시설에 잠입하여 주어진 미션을 수행하고 탈출해야 합니다.

1. 전력 복구
2. 데이터 수집
3. 출구 잠금 해제
4. 시설 탈출

이동과 상호작용 과정에서 발생한 소리는 센서 로봇에 감지될 수 있습니다. 잠입자는 은신과 이동 경로 선택을 통해 추격자의 탐색을 피해야 합니다.

### Pursuer — 추격자

시설에 배치된 센서 로봇을 이용하여 잠입자를 탐색하고 제거해야 합니다.

잠입자가 발생시킨 소리가 센서에 감지되면 위치 단서를 얻을 수 있습니다. 추격자는 해당 구역의 로봇으로 시점을 전환하고, 주변을 탐색하여 잠입자를 포착합니다.

## 03. Core Features

### Sound Detection System

플레이어의 행동에 따라 발생하는 소리를 감지하는 시스템입니다.

- 소리의 유형, 발생 위치, 강도 및 지속시간 관리
- 거리 감쇠와 장애물 차폐를 고려한 감지 판정
- 센서 로봇을 통한 소리 정보 전달

### Sensor Robot System

추격자가 시설의 여러 구역을 탐색할 수 있도록 구성한 시스템입니다.

- 센서별 소리 감지
- 감지 위치 정보 표시
- 센서 로봇 간 시점 전환
- 레이저 스캔을 통한 잠입자 탐색

### VR Interaction System

VR 환경의 공간적 특성을 활용한 상호작용 시스템입니다.

- VR 기반 이동 및 상호작용
- 엄폐 보조 시스템
- 공간음향과 햅틱 피드백
- HMD를 이용한 시선 기반 공간 탐색

### Mission System

잠입자의 진행 목표를 단계별로 관리하는 시스템입니다.

전력 복구부터 시설 탈출까지 총 4단계의 미션을 순차적으로 진행합니다.

### Multiplayer System

Photon Fusion 2를 이용하여 두 플레이어 간 게임 상태와 이벤트를 동기화합니다.

- 네트워크 기반 게임 상태 관리
- RPC를 통한 소리 이벤트 전달
- 역할별 인터페이스 및 게임 시스템 연동

## 04. Technical Architecture

소리 감지 및 추격 시스템의 주요 흐름은 다음과 같습니다.

**Sound Event → Sound Detection → Sensor Manager → Pursuer UI → Robot Viewpoint → Laser Detection**

주요 시스템 구성:

- `SoundManager` — 소리 이벤트 관리
- `SoundListener` — 거리 및 차폐 기반 감지 판정
- `SoundSensor` — 센서 감지 이벤트 처리
- `SensorManager` — 센서 정보와 제어 관리
- `SoundEventNetworkManager` — 네트워크 소리 이벤트 전달
- `LaserDetector` — 레이저 탐지 판정
- `MissionManager` — 미션 진행 상태 관리

## 05. Tech Stack

| 분야 | 기술 |
|---|---|
| Game Engine | Unity |
| Programming | C# |
| Networking | Photon Fusion 2 |
| Platform | VR HMD |
| Version Control | Git / GitHub |

## 06. Screenshots & Demo

실제 게임 플레이 화면과 영상 자료를 통해 주요 시스템의 동작을 확인할 수 있습니다.


## 07. Team & Development

**Sejong University — Capstone Design**

Team: 일단해보조

2026년 1학기 캡스톤디자인 프로젝트로 개발되었습니다.

팀 단위 협업을 통해 게임 기획, 시스템 프로그래밍, VR 인터랙션, 네트워크, UI/UX, 레벨 구성 및 테스트를 진행했습니다.
