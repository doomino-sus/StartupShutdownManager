# Menedżers skryptów

Aplikacja do zarządzania skryptami uruchamianymi podczas startu i zamykania systemu Windows.

## Opis
Aplikacja umożliwia zarządzanie skryptami, które są wykonywane podczas różnych zdarzeń systemowych:
- Uruchomienie systemu
- Logowanie użytkownika
- Przed wyłączeniem systemu
- Przed wylogowaniem użytkownika

## Funkcje
- Dodawanie, edycja i usuwanie skryptów
- Obsługa plików .exe, .bat i skryptów PowerShell (.ps1)
- Uruchamianie skryptów z podwyższonymi uprawnieniami
- Włączanie/wyłączanie skryptów bez ich usuwania
- Testowanie skryptów bezpośrednio z aplikacji
- Trwałe zapisywanie konfiguracji
- Przyjazny interfejs użytkownika

## Wymagania
- System Windows
- .NET Framework 4.7.2 lub nowszy
- Uprawnienia administratora

## Instalacja
1. Pobierz najnowszą wersję
2. Uruchom aplikację jako Administrator
3. Dodaj swoje skrypty przez interfejs użytkownika

## Kompilacja ze źródeł
1. Sklonuj repozytorium
2. Otwórz rozwiązanie w Visual Studio
3. Zainstaluj wymagany pakiet NuGet:4. Skompiluj rozwiązanie

## Użytkowanie
1. Uruchom aplikację jako Administrator
2. Kliknij "Dodaj skrypt"
3. Wybierz plik skryptu i skonfiguruj ustawienia wykonania
4. Używaj menu kontekstowego lub przycisków do zarządzania skryptami
5. Testuj skrypty używając przycisku "Testuj"

## Zrzuty ekranu
[Tutaj dodaj zrzuty ekranu aplikacji]

## Struktura projektu

```
StartupShutdownManager/
├── .gitignore
├── README.md
├── LICENSE
├── StartupShutdownManager.sln
├── StartupShutdownManager/
│   ├── Program.cs
│   ├── App.config
│   ├── packages.config
│   ├── mojaikona.ico
│   └── Properties/


```

## Opis struktury

- `StartupShutdownManager.sln` - Główny plik rozwiązania Visual Studio
- `StartupShutdownManager/` - Główny katalog projektu
  - `Program.cs` - Punkt wejścia aplikacji
  - `App.config` - Plik konfiguracyjny aplikacji
  - `packages.config` - Konfiguracja pakietów NuGet
  - `mojaikona.ico` - Ikona aplikacji

# Opis trybów wykonania skryptu

## 1. Przy starcie systemu (SystemStartup)
Skrypt uruchamia się zaraz po starcie systemu Windows:
- Wykonywany przed zalogowaniem jakiegokolwiek użytkownika.
- Uruchamia się z uprawnieniami SYSTEM.

### Przykładowe zastosowania:
- Inicjalizacja sprzętu
- Konfiguracja sieci
- Montowanie dysków
- Uruchamianie krytycznych usług

## 2. Przy logowaniu użytkownika (UserLogon)
Skrypt uruchamia się po pomyślnym zalogowaniu użytkownika:
- Wykonywany dla każdego użytkownika, który się loguje.
- Uruchamia się w kontekście zalogowanego użytkownika.

### Przykładowe zastosowania:
- Mapowanie dysków sieciowych
- Konfiguracja środowiska użytkownika
- Uruchamianie aplikacji użytkownika
- Synchronizacja danych użytkownika

## 3. Przed wyłączeniem systemu (BeforeShutdown)
Skrypt uruchamia się, gdy system otrzyma polecenie wyłączenia:
- Wykonywany przed rozpoczęciem procesu wyłączania systemu.
- Uruchamia się zanim system zacznie zamykać usługi i aplikacje.

### Przykładowe zastosowania:
- Backup danych
- Zamykanie aplikacji w kontrolowany sposób
- Zapisywanie stanu systemu
- Czyszczenie plików tymczasowych

## 4. Przed wylogowaniem użytkownika (BeforeLogoff)
Skrypt uruchamia się, gdy użytkownik inicjuje wylogowanie:
- Wykonywany przed zamknięciem sesji użytkownika.
- Uruchamia się, kiedy użytkownik jest jeszcze zalogowany.

### Przykładowe zastosowania:
- Zapisywanie ustawień aplikacji
- Synchronizacja danych użytkownika
- Zamykanie aplikacji użytkownika
- Czyszczenie tymczasowych plików użytkownika

## Przykłady praktycznego wykorzystania

### Przy starcie systemu:
```powershell
# Montowanie dysku sieciowego dla wszystkich użytkowników
Net Use Z: \\server\share /PERSISTENT:YES
# Uruchomienie specyficznej usługi
Start-Service "MyCustomService"
```

### Przy logowaniu użytkownika:
```powershell
# Mapowanie osobistego dysku sieciowego
Net Use H: \\server\users\$env:USERNAME
# Uruchomienie programów użytkownika
Start-Process "outlook.exe"
```

### Przed wyłączeniem systemu:
```powershell
# Backup ważnych danych
Compress-Archive -Path "C:\WażneDane" -DestinationPath "D:\Backup\$(Get-Date -Format 'yyyy-MM-dd')"
# Zatrzymanie usług w odpowiedniej kolejności
Stop-Service "CustomService1"
Stop-Service "CustomService2"
```

### Przed wylogowaniem użytkownika:
```powershell
# Zapisanie stanu aplikacji
Export-AppState -Path "$env:APPDATA\MyApp\state.json"
# Zamknięcie połączeń sieciowych
Net Use * /DELETE /Y
```

## Ważne uwagi:
- Skrypty uruchamiane przy starcie systemu i przed wyłączeniem wykonują się w kontekście systemowym.
- Skrypty logowania/wylogowania wykonują się w kontekście użytkownika.
- Wszystkie skrypty mają limit czasu wykonania (domyślnie 5 minut).
- Skrypty powinny być idempotentne (bezpieczne przy wielokrotnym wykonaniu).
- Należy uwzględnić obsługę błędów i logowanie.
- Skrypty powinny być testowane w środowisku testowym przed wdrożeniem.

## Zalecenia:
- Używaj logowania w skryptach aby śledzić ich wykonanie.
- Dodaj obsługę błędów i powiadomienia.
- Unikaj długotrwałych operacji.
- Testuj skrypty przed wdrożeniem.
- Dokumentuj cel i działanie każdego skryptu.
- Regularnie przeglądaj i aktualizuj skrypty.
