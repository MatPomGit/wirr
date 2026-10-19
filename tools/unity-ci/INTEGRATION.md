# Jedna wersja WiRR: plan integracji i odbioru

Data: 2026-10-19. Edytor referencyjny: Unity 6000.6.5f1.

## Jedno źródło zmian

Kod, szczegółowe instrukcje i harness są integrowane najpierw w repozytorium źródłowym, w PR #99. Zawiera on historie PR #88 (harness), #96 (kompilacja i assemblies), #97 (instrukcje), #98 (istniejący Supabase) i własne poprawki wersji oraz nawigacji. Poszczególnych gałęzi nie należy scalać ponownie po przyjęciu integracji.

Po odbiorze źródła publikujemy jeden sanitizowany snapshot przez https://github.com/KIA-students/wirr/pull/80 . Snapshot zawiera również poprawki studenckiego PR #84. Zachowujemy dokładnie stan `student-work/` i `competition/` z repozytorium kursu. Wykluczamy prywatne workflow, narzędzie eksportu kolejki i `wirr-modified-files.txt`. Nie kopiujemy do studentów historii gałęzi źródłowych. Dalsze zmiany studenckie należy przenosić z powrotem do źródła przed kolejnym eksportem.

Kolejność: odbiór zintegrowanego źródła → scalenie PR #99 → ponowne porównanie publicznego snapshotu z aktualnym repozytorium kursu → odbiór PR #80 → publikacja studenckiej gałęzi `upm` i Pages. Pozostałe PR-y zastąpione integracją można zamknąć z odnośnikiem do jej SHA. Nie publikujemy WiRR 2.0 na podstawie samej dostępności plików.

## Szybki test dokładnej wersji na Andromedzie

Uruchom w PowerShell. Każde polecenie Git jest sprawdzane; SHA pobieramy w repozytorium, w którym wykonano fetch, i przekazujemy jawnie do świeżego worktree. Nie używamy FETCH_HEAD odczytywanego z innego worktree.

```powershell
$ErrorActionPreference = "Stop"
$source = "C:\Users\matpo\repo\wirr-matpom"
git -C $source fetch origin fix/unity-6000-6-5-student-materials
if ($LASTEXITCODE -ne 0) { throw "Nie udało się pobrać gałęzi integracyjnej" }
$sha = (git -C $source rev-parse refs/remotes/origin/fix/unity-6000-6-5-student-materials).Trim()
if ($LASTEXITCODE -ne 0 -or $sha -notmatch '^[a-f0-9]{40}$') { throw "Brak poprawnego SHA" }
$verify = Join-Path "D:\Unity\cli" ("wirr-integrated-" + [guid]::NewGuid().ToString("N"))
git -C $source worktree add --detach $verify $sha
if ($LASTEXITCODE -ne 0) { throw "Nie udało się utworzyć worktree" }
if ((git -C $verify rev-parse HEAD).Trim() -ne $sha) { throw "Niezgodny HEAD" }
if (git -C $verify status --porcelain) { throw "Worktree zawiera zmiany" }
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$verify\tools\unity-ci\run-windows.ps1"
if ($LASTEXITCODE -ne 0) { throw "Testy nie przeszły; zachowano logi i projekt" }
Get-Content "$verify\artifacts\unity-native\summary.json"
```

Jeżeli edytor nie jest w standardowym katalogu, dodaj do wywołania runnera `-Editor 'PELNA_SCIEZKA_DO_UNITY_6000.6.5f1\Editor\Unity.exe'`. Nie wskazuj innej wersji edytora. Runner sprawdza wymagane testy potwierdzeń, eksportu, ochrony właściciela i osiem testów PlayMode. Brak wyników oznacza FAIL, nie oczekiwanie na ocenę.

## Kryteria dopuszczenia do scalenia i publikacji

- Dokładny SHA z czystego worktree, Unity 6000.6.5f1, EditMode i PlayMode: PASS z aktualnymi XML i logami. Brak pominiętych testów oraz nierozwiązanych błędów kompilacji/importu.
- Windows i Ubuntu: czysty projekt URP, instalacja pakietu, osiem scen, otwarcie istniejącego projektu bez nadpisania własnych scen.
- Blok 02: rzeczywiste próby, przeniesienie obu laboratoriów do istniejącego formularza `wirr-report/1.0`, dwie osobne wysyłki i dwa potwierdzenia Supabase. Sprawdzenie ponowienia po braku sieci i dalszego eksportu kolejki. Bez wysyłania fikcyjnych danych studenckich.
- Snapshot publiczny: brak odwołań do repozytorium roboczego, brak prywatnych automatyzacji, zachowane prace i ranking, zgodność Editor/Runtime/Tests/Samples z zaakceptowanym źródłem.

## Wykonane kontrole w tej sesji

Testy offline: scripts~ 48, tools/wirr2 12, tests 9, competition 5 oraz WebSim 12: PASS. Nawigacja 36 instrukcji: PASS. Składnia wszystkich plików C# w Editor/Runtime/Tests i YAML workflow: PASS. Unikalność GUID oraz zgodność bezpośrednich zależności manifest/lockfile: PASS. Są to kontrole offline. Nie wykonano Unity EditMode/PlayMode, Windows/Ubuntu smoke ani rzeczywistego E2E Supabase.

Dotychczasowe awarie ILPP/Burst i wyniki ze starszego Unity 6000.6.0f1 nie potwierdzają odbioru tej wersji. Nie usuwaj ani nie regeneruj istniejących GUID, aby ukryć problem instalacji edytora. Jeśli nowy przebieg ulegnie awarii natywnej, zachowaj aktualne logi, XML, SHA i dziennik błędów hosta; nie wykorzystuj starych wyników PlayMode.
