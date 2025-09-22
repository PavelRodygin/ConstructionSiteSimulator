# Symulator Placu Budowy
# Zadanie testowe dla FlintSystems
## Witajcie, szanowni specjaliści FlintSystems!
Przedstawiam wam: Projekt Unity poświęcony realistycznemu symulatorowi żurawia. Projekt z modułową, skalowalną architekturą (choć nie był na nią główny nacisk). Projekt demonstruje moje umiejętności w pracy z fizyką i tworzeniu projektów symulacyjnych, doświadczenie w tworzeniu i dekompozycji encji, wykorzystanie DI, programowania reaktywnego i asynchronicznego.
> **Uwaga**: Podstawową wersję projektu oddałem po dobie pracy, następnie spędziłem jeszcze dwie doby na skalowaniu projektu i dodawaniu nowych funkcji. W sumie na projekt poświęcono 3 dni.
## 🏗️ Architektura Projektu
Projekt posiada modułową architekturę, gdzie każdy moduł jest niezależny i izolowany. Szczegółowo z tym rozwiązaniem można zapoznać się w moim repozytorium TechnicalSample, więc nie będę na tym akcentować uwagi. Ważne jest zrozumienie, że dla wygody testowania projekt można uruchomić i przetestować ze sceny dowolnego modułu (Bootstrap, MainMenu lub ConstructionSite).
### Kluczowe Zasady Architektoniczne
- **Projekt Modułowy**: Każdy moduł jest samowystarczalny i może być rozwijany niezależnie
- **Wzorzec MVP**: Wyraźne oddzielenie między logiką biznesową (Model), interfejsem użytkownika (View) i koordynacją (Presenter)
- **Wstrzykiwanie Zależności**: Używanie VContainer do czystego zarządzania zależnościami
- **Programowanie Reaktywne**: Biblioteka R3 do programowania opartego na zdarzeniach i przepływie danych
## 🏗️ System Żurawia - Zadanie Główne
### Hierarchia Żurawia i Komponenty
Żuraw składa się z trzech głównych komponentów, z których każdy ma określone obowiązki:
#### 1. **Podstawa Obrotowa**
- **Cel**: Kontroluje obrót żurawia wokół osi pionowej
- **Kluczowe Funkcje**:
  - Realistyczny obrót z krzywymi przyspieszenia/hamowania
  - Redukcja prędkości zależna od obciążenia (cięższy ładunek = wolniejszy obrót)
  - Płynna interpolacja między stanami obrotu
  - Śledzenie bieżącego kąta obrotu (znormalizowanego do 0-360°)
#### 2. **Wózek** - Poziomy Ruch Haka
- **Cel**: Zarządza poziomym ruchem wzdłuż wysięgnika żurawia
- **Kluczowe Funkcje**:
  - Płynny ruch do przodu/tyłu z ograniczeniami
  - Kontrola głębokości haka (długość pionowego kabla)
  - Śledzenie pozycji (zakres znormalizowany 0-1)
  - Monitorowanie obciążenia w czasie rzeczywistym z podłączonego ładunku
#### 3. **Hak** - Obsługa Ładunku
- **Cel**: Obsługuje podłączanie ładunku, symulację fizyki i zarządzanie obciążeniem
- **Kluczowe Funkcje**:
  - Symulacja fizyki oparta na ConfigurableJoint
  - Realistyczne zachowanie kołysania ładunku
  - Obliczanie i monitorowanie wagi obciążenia
  - Automatyczne wykrywanie i podłączanie ładunku
### 🔧 Konfiguracja ConfigurableJoint
Komponent Hak używa `ConfigurableJoint` Unity do realistycznej symulacji fizyki. Konfiguracja w prefabie `CraneHook` jest starannie dostrojona do optymalnego zachowania:
#### Ograniczenia Ruchu
- **Ruch Liniowy**: Tylko oś Y (ruch pionowy)
- **Ruch Kątowy**: Ograniczony obrót na wszystkich osiach dla realistycznego kołysania
- **Ograniczenia Kątowe**: ±10° na osiach X i Z, ±10° na osi Y
#### Napędy Stawu
- **Napęd Y** (Główny):
  - Sprężyna: 20,000 (silne pozycjonowanie pionowe)
  - Tłumik: 7,500 (płynny ruch)
  - Maks. Siła: 500,000 (obsługuje ciężkie ładunki)
- **Napędy X/Z** (Stabilizacja):
  - Sprężyna: 10,000 (zapobiega nadmiernemu kołysaniu)
  - Tłumik: 500 (naturalne tłumienie)
  - Maks. Siła: 50,000 (kontrolowany ruch)
#### Napędy Kątowe
- **Sprężyna**: 2,500 (naturalny ruch kołysania)
- **Tłumik**: 1,000 (realistyczne tłumienie)
- **Maks. Siła**: Nieograniczona (pozwala na naturalną fizykę)
### 🎯 Izolacja Komponentów i Rozszerzalność
Każdy komponent żurawia jest zaprojektowany tak, aby był całkowicie izolowany i skupiony na swoim konkretnym zadaniu:
- **RotatingBase**: Obsługuje tylko logikę obrotu i stan
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
// Czujnik obrotu żurawia (stopnie względem podstawy)
public ReadOnlyReactiveProperty<float> RotationSensor =>
    _rotationAngle.ToReadOnlyReactiveProperty();
// Czujnik obciążenia (bieżąca waga ładunku)
public ReadOnlyReactiveProperty<float> LoadSensor =>
    _currentLoad.ToReadOnlyReactiveProperty();
// Czujnik wysokości haka (pozycja pionowa)
public ReadOnlyReactiveProperty<float> HookHeightSensor =>
    _hookHeight.ToReadOnlyReactiveProperty();
// Czujnik odległości wózka (pozycja pozioma od podstawy)
public ReadOnlyReactiveProperty<float> TrolleyDistanceSensor =>
    _trolleyDistance.ToReadOnlyReactiveProperty();
// Reaktywne polecenia dla aktualizacji danych czujników
private readonly ReactiveCommand<SensorData> _sensorDataCommand = new();
// Czujniki automatycznie aktualizują się przy zmianie danych
_rotationAngle
    .DistinctUntilChanged()
    .Subscribe(angle => _sensorDataCommand.Execute(new RotationSensorData(angle)));
```
### 🚀 Integracja Programowania Reaktywnego
Projekt wykorzystuje R3 (Reactive Extensions for Unity) do czystego, opartego na zdarzeniach programowania z naciskiem na **reaktywne zdarzenia i polecenia**. Ta potężna kombinacja z modułową architekturą czyni tworzenie różnych czujników i instrumentów pomiarowych niesamowicie łatwym:
#### Łatwe Tworzenie Czujników
Dzięki dekompozycji komponentów i R3, można łatwo stworzyć:
- **🔄 Czujnik Obrotu**: Pokazuje kąt obrotu żurawia w stopniach względem podstawy
- **⚖️ Czujnik Obciążenia**: Wyświetla bieżącą wagę ładunku i status obciążenia
- **📏 Czujnik Wysokości**: Mierzy pionową pozycję haka i głębokość
- **📐 Czujnik Odległości**: Pokazuje poziomą pozycję wózka od podstawy żurawia
#### Reaktywne Zdarzenia i Polecenia
- **ReactiveCommand**: Używane do aktualizacji danych czujników i interakcji UI
- **ReactiveProperty**: Dane czujników w czasie rzeczywistym z automatycznymi powiadomieniami o zmianach
- **Subject/Observable**: Wzorce publikowania i subskrypcji zdarzeń dla strumieni czujników
- **CompositeDisposable**: Właściwe zarządzanie zasobami dla subskrypcji reaktywnych
#### Kluczowe Wzorce Reaktywne dla Czujników
```csharp
// Reaktywne polecenia dla danych czujników
private readonly ReactiveCommand<SensorData> _sensorUpdateCommand = new();
private readonly ReactiveCommand<LoadAlert> _loadAlertCommand = new();
// Reaktywne właściwości dla odczytów czujników w czasie rzeczywistym
private readonly ReactiveProperty<float> _rotationAngle = new(0f);
private readonly ReactiveProperty<float> _currentLoad = new(0f);
private readonly ReactiveProperty<float> _hookHeight = new(0f);
// Publikowanie zdarzeń z Subjects
private readonly Subject<CraneTelemetry> _telemetrySubject = new();
```
#### Przesyłanie Danych Czujników
- **Aktualizacje w Czasie Rzeczywistym**: Czujniki automatycznie aktualizują się przy zmianie danych komponentów
- **Filtrowanie Danych**: Używaj operatorów reaktywnych do filtrowania i przetwarzania danych czujników
- **Ograniczanie**: Zapobiegaj nadmiernym aktualizacjom czujników dla wydajności
- **Czyste Usuwanie**: Wszystkie subskrypcje czujników są właściwie zarządzane
### 🎮 System Sterowania
Żuraw posiada zaawansowany system sterowania z:
- **Płynną Obsługą Wejścia**: Wejście oparte na stanach z właściwym przyspieszeniem/hamowaniem
- **Zachowanie Zależne od Obciążenia**: Cięższe ładunki redukują maksymalną prędkość
- **Ograniczenia Bezpieczeństwa**: Limity ruchu i sprawdzenia pojemności obciążenia
- **Natychmiastowa Informacja Zwrotna**: Natychmiastowa reakcja na wejście operatora
### 🔧 System Konfiguracji
Zachowanie żurawia jest kontrolowane przez ScriptableObjects `CraneSpecificationSO`:
- **Ustawienia Obrotu**: Prędkość, przyspieszenie, krzywe hamowania
- **Pojemność Obciążenia**: Nominalna i maksymalna waga ładunku
- **Wydajność**: Redukcja prędkości na podstawie obciążenia
- **Ruch**: Ustawienia prędkości wózka i haka
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
2. Przejdź do sceny ConstructionSite
3. Użyj sterowania żurawiem do obsługi symulacji
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
