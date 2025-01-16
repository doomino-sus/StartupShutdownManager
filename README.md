# StartupShutdownManager
Aplikacja do zarządzania skryptami podczas startu i wyłączenia systemu Windows

# Menedżer Skryptów Startowych i Wyłączeniowych

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

