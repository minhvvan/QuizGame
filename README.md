# QuizGame

![Icon-256](https://github.com/user-attachments/assets/a74ac8e0-df18-45a7-9af1-4a62f9cc45ea)

<br>

## 📝 프로젝트 소개

이 프로젝트는 Unity로 개발된 모바일 퀴즈 게임입니다. 사용자들이 다양한 카테고리의 퀴즈를 풀며 지식을 테스트하고 점수를 얻을 수 있는 게임입니다.

<img src="https://github.com/user-attachments/assets/7b74fcd6-378f-4a7e-a885-2b11e326845c" width="300" alt="게임 인트로 화면">

<br>

## 🛠️ 주요 기능 및 구현 사항


### 1. DoTween을 이용한 UI 애니메이션 및 관리


#### 상태 패턴 기반 퀴즈 카드 시스템

- 단 3개의 퀴즈 카드만을 생성하여 순환하는 최적화된 시스템 구현
- 상태 패턴(State Pattern)을 활용한 카드 위치 및 애니메이션 관리
- 사용자가 퀴즈를 풀면 다음 퀴즈가 자동으로 표시되며, 카드가 자연스럽게 순환하는 방식

```csharp
// 퀴즈 카드 상태 패턴 구현 예시
public interface IQuizCardPositionState
{
    void Trasition(bool withAnimation, Action onComplete = null);
}

public class QuizCardPositionStateContext
{
    private IQuizCardPositionState _currentState;

    public void SetState(IQuizCardPositionState state, bool withAnimation, Action onComplete = null)
    {
        if (_currentState == state) return;
        
        _currentState = state;
        _currentState.Trasition(withAnimation, onComplete);
    }
}
```

<br>

#### 카드 위치 관리 및 순환 시스템

- First, Second, Remove 세 가지 위치 상태를 통한 카드 순환
- 완료된 퀴즈 카드는 화면 밖으로 밀어내고 새 퀴즈 데이터로 업데이트하여 재사용
- 메모리 사용량 최적화 및 게임 퍼포먼스 대폭 향상

```csharp
// 퀴즈 카드 순환 관리 코드 예시
private void ShowNextQuiz(int currentIdx)
{
    int currentCardIdx = currentIdx % 3;
    _cardControllers[currentCardIdx].SetQuizCardPosition(QuizCardController.QuizCardPositionType.Remove, true, () =>
    {
        if (_quizIdx < _quizDataList.Count)
        {
            _cardControllers[currentCardIdx].SetQuiz(_quizDataList[_quizIdx], OnCompletedQuiz, OnQuizResult, _quizIdx);
        }
        
        _quizIdx++;

        int first = (currentIdx + 1) % 3;
        int second = (currentIdx + 2) % 3;

        // 다음 카드를 첫 번째 위치로 이동
        _cardControllers[first].gameObject.SetActive(true);
        _cardControllers[first].SetQuizCardPosition(QuizCardController.QuizCardPositionType.First, true);
        _cardControllers[first].StartQuiz();

        // 그 다음 카드를 두 번째 위치로 이동
        _cardControllers[second].gameObject.SetActive(true);
        _cardControllers[second].SetQuizCardPosition(QuizCardController.QuizCardPositionType.Second, true);
    });
}
```

<img src="https://github.com/user-attachments/assets/41540d77-6259-4336-898a-2abaf86c853a" width="300" alt="카드 순환 스크린샷">

<br>
<br>

#### 커스텀 UI 컴포넌트

##### 1. 애니메이션 스위치 컴포넌트

- 부드러운 전환 애니메이션을 갖춘 직관적인 토글 스위치 구현
- DoTween을 활용한 핸들 이동 및 배경색 변화 애니메이션
- 상태 변경 이벤트 콜백 지원으로 다른 UI 요소와 쉽게 연동 가능

```csharp
// 커스텀 스위치 구현 예시
public class Switch : MonoBehaviour
{
    [SerializeField] private RectTransform _handle;
    [SerializeField] private Image _background;
    [SerializeField] List<Color> _colors = new List<Color>();
    
    private bool _bActive;
    public Action<bool> OnSwitchChanged;

    void OnClickSwitch()
    {
        if (_bActive) SetOff();
        else SetOn();
        OnSwitchChanged?.Invoke(_bActive);
    }

    void SetOn()
    {
        _background.color = _colors[1];
        _handle.DOAnchorPosX(_rectTransform.sizeDelta.x - _handle.sizeDelta.x, .2f).SetEase(Ease.OutQuad);
        _bActive = true;
    }

    void SetOff()
    {
        _background.color = _colors[0];
        _handle.DOAnchorPosX(0, 0.2f).SetEase(Ease.OutQuad);
        _bActive = false;
    }
}
```

<img src="https://github.com/user-attachments/assets/62a60c35-c188-4dc0-bb99-d67b4a690601" width="300" alt="스위치.png">

<br>
<br>

##### 2. 재사용 기반 스크롤 뷰 시스템

- 오브젝트 풀링을 활용한 고성능 스크롤 뷰 구현
- 화면에 보이는 셀만 생성하고 재활용하는 방식으로 메모리 사용량 최적화
- 그리드 레이아웃 지원 및, 스크롤 방향에 따른 셀 동적 생성/제거 로직

```csharp
// 스크롤 뷰 컨트롤러 핵심 로직
public class ScrollViewController : MonoBehaviour
{
    // 스크롤 위치에 따라 보이는 셀만 생성하고 재활용
    private void OnScroll(Vector2 value)
    {
        if (_lastYValue < value.y)
        {
            // 위로 스크롤 시 상단에 새 셀 추가, 하단의 안 보이는 셀 제거
            ProcessScrollUp();
        }
        else
        {
            // 아래로 스크롤 시 하단에 새 셀 추가, 상단의 안 보이는 셀 제거
            ProcessScrollDown();
        }

        _lastYValue = value.y;
    }
}
```

<img src="https://github.com/user-attachments/assets/d72cc944-3a03-4cac-a11f-2cd3f7862e16" width="300" alt="스크롤뷰.png">

<br>
<br>

##### 3. 둥근 타이머 컴포넌트

- 원형 진행 게이지로 남은 시간을 시각적으로 표현하는 타이머 구현
- 애니메이션 캡(cap) 요소를 통한 부드러운 시각적 피드백
- 시간 경과에 따른 자동 색상 변경 및 이벤트 콜백 지원

```csharp
// 둥근 타이머 구현 핵심 코드
public class RoundTimer : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float totalTime;
    [SerializeField] private Image headCapImage;
    [SerializeField] private Image tailCapImage;
    [SerializeField] private TMP_Text timeText;
    
    private IEnumerator ProgressTimer()
    {
        while (!_isPaused)
        {
            CurrentTime += Time.deltaTime;
            fillImage.fillAmount = (totalTime - CurrentTime) / totalTime;

            // 타이머 만료 처리
            if (CurrentTime >= totalTime)
            {
                headCapImage.gameObject.SetActive(false);
                tailCapImage.gameObject.SetActive(false);
                _isPaused = false;
                onTimerExpired?.Invoke();
                yield break;
            }
            // 타이머 진행 시각적 업데이트
            else
            {
                headCapImage.gameObject.SetActive(true);
                tailCapImage.gameObject.SetActive(true);
                fillImage.gameObject.SetActive(true);
                headCapImage.transform.localRotation = 
                    Quaternion.Euler(new Vector3(0, 0, fillImage.fillAmount * 360));

                var time = totalTime - CurrentTime;
                timeText.text = time.ToString("F0");
            }
            
            yield return null;
        }
    }
}
```

<img src="https://github.com/user-attachments/assets/205f9b4a-04f3-473f-a9d2-c2948748e8b5" width="300" alt="타이머.gif">

<br>
<br>

### 2. 구글 AdMob을 이용한 광고 시스템

#### 통합 광고 관리 시스템

- 배너, 전면, 보상형 광고를 체계적으로 관리하는 통합 시스템 구현
- 플랫폼별(Android/iOS) 광고 ID 관리 및 자동 전환
- 광고 이벤트 핸들링을 통한 로드 상태 관리 및 오류 처리

```csharp
// AdMob 초기화 및 관리 시스템
public class AdmobAdsManager : Singleton<AdmobAdsManager>
{
    // 플랫폼별 광고 ID 관리
#if UNITY_ANDROID
    private string _bannerAdUnitId = "ca-app-pub-xxx/xxx";
    private string _interstitialAdUnitId = "ca-app-pub-xxx/xxx";
    private string _rewardedAdUnitId = "ca-app-pub-xxx/xxx";
#elif UNITY_IOS
    private string _bannerAdUnitId = "ca-app-pub-xxx/xxx";
    private string _interstitialAdUnitId = "ca-app-pub-xxx/xxx";
#endif
    
    // 광고 초기화 및 로드
    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }
}
```

<br>

#### 다양한 광고 형태 지원

- 배너 광고: 화면 하단에 표시되는 비침입적 광고
- 전면 광고: 특정 전환 지점에서 표시되는 전체 화면 광고
- 보상형 광고: 하트 획득 등 보상을 제공하는 사용자 선택형 광고

```csharp
// 보상형 광고 표시 구현
public void ShowRewardedAd(Action<Reward> callback) 
{
    const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

    if (_rewardedAd != null && _rewardedAd.CanShowAd())
    {
        _rewardedAd.Show((Reward reward) =>
        {
            callback?.Invoke(reward);
            Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
        });
    }
}

// 샵에서 광고 통합 사용 예시
public void OnClickShopItemButton(int index)
{
    switch (index)
    {
        case 1: // 광고 시청으로 하트 획득
        {
            AdmobAdsManager.Instance.ShowRewardedAd((Reward reward) =>
            {
                GameManager.Instance.HeartCount += (int)reward.Amount;
            });
            break;
        }
        // 다른 아이템 케이스...
    }
}
```

<img src="https://github.com/user-attachments/assets/26f76e13-45e9-49b7-a796-daa104e2753f" width="300" alt="광고 시스템 스크린샷">

<br>
<br>

### 3. CSV 기반 퀴즈 데이터 관리 시스템

- 퀴즈 데이터를 외부 CSV 파일로 분리하여 유지보수 및 업데이트 용이성 향상
- 스테이지별 퀴즈 데이터 파일 분리를 통한 체계적 관리
- 정규식을 활용한 CSV 파싱으로 견고한 데이터 처리 구현

<br>

#### CSV 데이터 구조 및 로딩 시스템

```csharp
// CSV 파일 파싱 및 퀴즈 데이터 로드 시스템
public static class QuizDataController
{
    static string ROW_SEPARATOR = @"\r\n|\n\r|\n|\r";
    static string COL_SEPARATOR = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    private static char[] TRIM_CHARS = { '\"' };
    
    public static List<QuizData> LoadQuizData(int stageIndex)
    {
        // 스테이지 인덱스에 따른 CSV 파일 로드
        var fileName = "QuizData-" + stageIndex;
        TextAsset quizDataAsset = Resources.Load(fileName) as TextAsset;
        
        // 정규식을 사용한 CSV 행 분리
        var lines = Regex.Split(quizDataAsset.text, ROW_SEPARATOR);
        var quizDataList = new List<QuizData>();
        
        // 헤더를 제외한 각 행(퀴즈 데이터)을 파싱
        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], COL_SEPARATOR);
            if (values.Length < 4) continue; // 최소 필요 데이터 검증
            
            QuizData quizData = new QuizData();

            // CSV 열 데이터 파싱 및 QuizData 객체 생성
            for (var j = 0; j < values.Length; j++)
            {
                var value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                
                switch (j)
                {
                    case 0: quizData.question = value; break;
                    case 1: quizData.description = value; break;
                    case 2: quizData.type = int.Parse(value); break;
                    case 3: quizData.answer = int.Parse(value); break;
                    case 4: quizData.firstOption = value; break;
                    case 5: quizData.secondOption = value; break;
                    case 6: quizData.thirdOption = value; break;
                }
            }
            
            quizDataList.Add(quizData);
        }
        
        return quizDataList;
    }
}
```

<br>

#### CSV 데이터 파일 예시

```
Question,Description,Type,Answer,option-1,option-2,option-3
지구에서 가장 큰 대양은 무엇인가요?,지구의 표면적 중 약 30%를 차지하는 가장 큰 대양입니다.,0,0,태평양,대서양,인도양
사과는 채소입니까?,사과는 채소인지 과일인지에 대한 퀴즈입니다.,1,1,,,
```

<br>

## 👥 개발자 정보

<br>

- 개발: [이민환]
- 디자인: [Mocapot]

<br>

## 🙏 감사의 말

<br>

- DoTween - http://dotween.demigiant.com/
- AdMob - https://admob.google.com/
- Mocapot
- 멋쟁이 사자처럼
