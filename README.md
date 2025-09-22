# Symulator Placu Budowy
# Zadanie testowe dla FlintSystems
## Witajcie, szanowni specjaliści FlintSystems!
Przedstawiam wam: Projekt Unity poświęcony realistycznemu symulatorowi żurawia. Projekt z modułową, skalowalną architekturą (choć główny nacisk nie na niej). Projekt demonstruje moje umiejętności w pracy z fizyką i tworzeniu projektów symulacyjnych, doświadczenie w tworzeniu i dekompozycji encji, wykorzystanie DI, programowania reaktywnego i asynchronicznego.
> **Uwaga**: Podstawową wersję projektu ukończyłem i oddałem po jednym dniu pracy, następnie spędziłem kolejne dwa dni na skalowaniu architektury i dodawaniu nowych funkcji. W sumie na rozwój projektu poświęcono trzy dni. Niektóre systemy projektu (z wyjątkiem podstawowych, wskazanych w zadaniu technicznym) wciąż są w trakcie dopracowywania; pewne aspekty nie zdążyłem w pełni zrealizować i zoptymalizować. Z przyjemnością podzielę się moimi pomysłami na dalsze ulepszenie projektu podczas rozmowy technicznej!

## 🏗️ Architektura Projektu
Projekt posiada modułową architekturę, gdzie każdy moduł jest niezależny i izolowany. Szczegółowo z tym rozwiązaniem można zapoznać się w moim repozytorium TechnicalSample, więc nie będę na tym akcentować uwagi. Ważne jest zrozumienie, że dla wygody testowania projekt można uruchomić i przetestować ze sceny dowolnego modułu (Bootstrap, MainMenu lub ConstructionSite).
### Kluczowe Zasady Architektoniczne
- **Projekt Modułowy**: Każdy moduł jest samowystarczalny i może być rozwijany niezależnie
- **Wzorzec MVP**: Wyraźne oddzielenie między logiką biznesową (Model), interfejsem użytkownika (View) i koordynacją (Presenter)
- **Dependency Injection**: Używanie VContainer do czystego zarządzania zależnościami
- **Programowanie Reaktywne**: Biblioteka R3 do programowania opartego na zdarzeniach i przepływie danych
 
## 🏗️ System Żurawia - Zadanie Główne
### Hierarchia Żurawia i Komponenty
Żuraw składa się z trzech głównych komponentów, z których każdy ma określone obowiązki:
#### 1. **Turntable**
- **Cel**: Kontroluje obrót żurawia wokół osi pionowej
- **Kluczowe Funkcje**:
  - Realistyczny obrót z krzywymi przyspieszenia/hamowania
  - Redukcja prędkości zależna od obciążenia (cięższy ładunek = wolniejszy obrót)
  - Płynna interpolacja między stanami obrotu
  - Śledzenie bieżącego kąta obrotu (znormalizowanego do 0-360°)
#### 2. **Trolley**
- **Cel**: Zarządza poziomym ruchem wzdłuż wysięgnika żurawia
- **Kluczowe Funkcje**:
  - Płynny ruch do przodu/tyłu z ograniczeniami
  - Kontrola głębokości haka (długość pionowego kabla)
  - Śledzenie pozycji (zakres znormalizowany 0-1)
#### 3. **Hak**
- **Cel**: Obsługuje podłączanie ładunku, symulację fizyki i zarządzanie obciążeniem
- **Kluczowe Funkcje**:
  - Symulacja fizyki liny za pomocą ConfigurableJoint
  - Realistyczne zachowanie kołysania ładunku
  - Obliczanie i monitorowanie wagi obciążenia
  - Wykrywanie i podłączanie ładunku
   
### 🔧 Konfiguracja ConfigurableJoint
Komponent Hak używa `ConfigurableJoint` Unity do realistycznej symulacji fizyki liny. Konfiguracja w prefabie `CraneHook` jest starannie dostrojona do optymalnego zachowania:
#### Ograniczenia Ruchu
- **Ruch Liniowy**: Tylko oś Y (ruch pionowy)
- **Ruch Kątowy**: Ograniczony obrót na wszystkich osiach dla realistycznego kołysania
- **Ograniczenia Kątowe**: ±10° na osiach X i Z, ±10° na osi Y
#### Napędy Stawu
- **Napęd Y** (Główny):
  - Sprężyna: 10,000 (silne pozycjonowanie pionowe)
  - Tłumik: 20000 (płynny ruch bez oscylacji)
  - Maks. Siła: 500,000 niutonów
- **Napędy X/Z** (Stabilizacja):
  - Sprężyna: 10,000 (zapobiega nadmiernemu kołysaniu)
  - Tłumik: 5000
  - Maks. Siła: 50,000 (kontrolowany ruch)
#### Napędy Kątowe
- **Sprężyna**: 2,500 (naturalny ruch kołysania)
- **Tłumik**: 1,000 (realistyczne tłumienie)
- **Maks. Siła**: Nieograniczona
### 🎯 Izolacja Komponentów i Rozszerzalność
Każdy komponent żurawia jest zaprojektowany tak, aby był całkowicie izolowany i skupiony na swoim konkretnym zadaniu:
- **Turntable**: Obsługuje tylko logikę obrotu i stan
- **Trolley**: Zarządza poziomym ruchem i pozycjonowaniem haka
- **Hook**: Zajmuje się fizyką ładunku i podłączaniem
Ta izolacja czyni system wysoce rozszerzalnym:
#### Łatwa Integracja Czujników
```csharp
// Każdy komponent może łatwo eksponować dane reaktywne dla czujników i instrumentów
public ReadOnlyReactiveProperty<float> CurrentRotationAngle =>
    _rotationAngle.ToReadOnlyReactiveProperty();
public ReadOnlyReactiveProperty<float> CurrentLoad =>
    _currentLoad.ToReadOnlyReactiveProperty();
public ReadOnlyReactiveProperty<float> HookHeight =>
    _hookHeight.ToReadOnlyReactiveProperty();
public ReadOnlyReactiveProperty<float> TrolleyDistance =>
    _trolleyDistance.ToReadOnlyReactiveProperty();
// Reaktywne polecenia dla aktualizacji czujników
private readonly ReactiveCommand<SensorData> _sensorUpdateCommand = new();
// Czujniki mogą subskrybować reaktywne zdarzenia z filtrowaniem
_rotationAngle
    .Where(angle => Mathf.Abs(angle) > 0.1f)
    .Subscribe(angle => UpdateRotationSensor(angle));
```
#### Przesyłanie Danych Czujników w Czasie Rzeczywistym
Architektura programowania reaktywnego (R3) ułatwia tworzenie różnych czujników i instrumentów dla panelu operatora:
```csharp
// Śledzenie bieżącego kąta obrotu żurawia
public ReactiveProperty<float> CurrentRotationAngle { get; } = new(0f);
 turntable.CurrentRotationAngle
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnAngleChanged)
                .AddTo(this);
// Śledzenie masy bieżącego ładunku
 hook.CurrentCargoMass
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnMassChanged)
                .AddTo(this);
// Śledzenie pozycji Trolley
 trolley.RelativeZPosition
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnPositionChanged)
                .AddTo(this);
```
### 🚀 Integracja Programowania Reaktywnego
Projekt wykorzystuje R3 (Reactive Extensions for Unity) do czystego, opartego na zdarzeniach programowania z naciskiem na **reaktywne zdarzenia i polecenia**. Ta potężna kombinacja z modułową architekturą czyni tworzenie różnych czujników i instrumentów pomiarowych niesamowicie łatwym:
#### Łatwe Tworzenie Czujników
Dzięki dekompozycji komponentów i R3, można łatwo stworzyć:
- **🔄 Czujnik Obrotu**: Pokazuje kąt obrotu żurawia w stopniach względem podstawy
- **⚖️ Czujnik Obciążenia**: Wyświetla bieżącą wagę ładunku i status obciążenia
- **📏 Czujnik Wysokości**: Mierzy pionową pozycję haka i głębokość
- **📐 Czujnik Odległości**: Pokazuje poziomą pozycję wózka od podstawy żurawia
-
#### Reaktywne Zdarzenia i Polecenia
- **ReactiveCommand**: Używane do aktualizacji danych czujników i interakcji UI
- **ReactiveProperty**: Dane czujników w czasie rzeczywistym z automatycznymi powiadomieniami o zmianach
- **Subject/Observable**: Wzorce publikowania i subskrypcji zdarzeń dla strumieni czujników
- **CompositeDisposable**: Właściwe zarządzanie zasobami dla subskrypcji reaktywnych
#### Przesyłanie Danych Czujników
- **Aktualizacje w Czasie Rzeczywistym**: Czujniki automatycznie aktualizują się przy zmianie danych komponentów
- **Filtrowanie Danych**: Używaj operatorów reaktywnych do filtrowania i przetwarzania danych czujników
- **Ograniczanie**: Zapobiegaj nadmiernym aktualizacjom czujników dla wydajności
- **Czyste Usuwanie**: Wszystkie subskrypcje czujników są właściwie zarządzane
-
### 🎮 System Sterowania
Żuraw posiada zaawansowany system sterowania z:
- **Płynną Obsługą Wejścia**: Wejście oparte na stanach z właściwym przyspieszeniem/hamowaniem
- **Zachowanie Zależne od Obciążenia**: Cięższe ładunki redukują maksymalną prędkość
- **Ograniczenia Bezpieczeństwa**: Limity ruchu i sprawdzenia pojemności obciążenia
- **Natychmiastowa Informacja Zwrotna**: Natychmiastowa reakcja na wejście operatora
-
### 🔧 System Konfiguracji (nieukończony)
Zachowanie żurawia jest kontrolowane przez ScriptableObjects `CraneSpecificationSO`:
- **Ustawienia Obrotu**: Prędkość, przyspieszenie, krzywe hamowania
- **Pojemność Obciążenia**: Nominalna i maksymalna waga ładunku
- **Wydajność**: Redukcja prędkości na podstawie obciążenia
- **Ruch**: Ustawienia prędkości wózka i haka
-
### 🎯 Przyszła Rozszerzalność
Modułowy projekt ułatwia dodanie:
1. **Systemy Czujników**: Różne instrumenty pomiarowe (czujniki obrotu, obciążenia, wysokości, odległości)
2. **Systemy Dźwiękowe**: Źródła audio można łatwo umieścić na wózku, mechanizmie obrotu lub haku
3. **Efekty Wizualne**: Systemy cząsteczek, animacje i wizualna informacja zwrotna
4. **Integracja Panelu**: Przesyłanie danych czujników w czasie rzeczywistym do interfejsów operatora
5. **Telemetria**: Systemy logowania i analizy danych
6. **Integracja AI**: Automatyczna operacja żurawia na podstawie danych czujników
  
### 🛠️ Najważniejsze Aspekty Techniczne
- **Symulacja Oparta na Fizyce**: Realistyczne zachowanie ładunku przy użyciu systemu fizyki Unity
- **Zoptymalizowana Wydajność**: Efektywne pętle aktualizacji i zarządzanie stanami
- **Bezpieczeństwo Pamięci**: Właściwe wzorce usuwania i zarządzania zasobami
- **Testowalny**: Czyste oddzielenie obowiązków umożliwia łatwe testowanie jednostkowe
- **Skalowalny**: Architektura oparta na komponentach wspiera złożone konfiguracje żurawia
 
## 🚀 Jak Zacząć
1. Otwórz projekt w Unity 6.000.2 lub nowszym
2. Przejdź do sceny Bootstrap i uruchom ją
3. Przejdź dalej przez menu, naciskając przycisk Open Simulator
4. Eksperymentuj z różnymi wagami ładunku i obserwuj realistyczne zachowanie fizyki
## 📁 Struktura Projektu
```
Assets/
├── Modules/Base/ConstructionSite/
│ ├── Scripts/Gameplay/Crane/
│ │ ├── Crane.cs # Główny kontroler żurawia
│ │ ├── RotatingBase.cs # Komponent obrotu
│ │ ├── Trolley.cs # Ruch poziomy
│ │ ├── Hook.cs # Obsługa ładunku
│ │ └── CraneSpecificationSO.cs # Konfiguracja
│ └── Prefabs/
│ ├── Crane.prefab # Główny montaż żurawia
│ └── CraneHook.prefab # Hak z ConfigurableJoint
└── CodeBase/
    ├── Core/Infrastructure/ # System modułów
    └── Implementation/ # Konkretne implementacje
```
Ten projekt demonstruje, jak właściwa architektura, izolacja komponentów i programowanie reaktywne mogą stworzyć wysoce utrzymywalny i rozszerzalny system symulacyjny. Implementacja żurawia służy jako doskonały przykład, jak budować złożone systemy oparte na fizyce w Unity, zachowując czysty, testowalny kod.
## 🚀 Oś Czasu Rozwoju
Ten projekt został opracowany jako **24-godzinny szybki prototyp** w celu zademonstrowania:
- Izolacji komponentów i rozszerzalności
- Symulacji żurawia opartej na fizyce
- Zasad architektury modułowej
- Programowania reaktywnego z R3
Chociaż podstawowa funkcjonalność jest kompletna i działa, niektóre ulepszenia i optymalizacje są w toku. Nacisk położono na pokazanie pracy z fizyką, podejścia architektonicznego i potencjału rozszerzalności, a nie na gotowość produkcyjną.
