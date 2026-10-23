# WiRR: weryfikacja self-hosted HomeServer (2026-10-23)

Zakres: repozytorium rozwojowe i zadania bez opłat za GitHub-hosted Actions. Samo dodanie etykiety `self-hosted` w YAML nie dowodzi, że runner jest dostępny, uprawniony do repozytorium lub działa.

## Bezpieczna kontrola

1. W Actions na gałęzi `main` uruchom **WiRR HomeServer runner preflight (manual)** przez `Run workflow`. Tylko `workflow_dispatch`, wyłącznie `self-hosted, Linux, ARM64`.
2. Jeśli zadanie pozostaje `queued`, **nie uznawaj go za PASS**. Sprawdź rejestrację i etykiety runnera, usługę systemd oraz dostęp do repozytorium. Nie zmieniaj harmonogramu eksportera raportów.
3. Dla przebiegu `success` pobierz `wirr-selfhosted-preflight-<run>` i zapisz w #125 numer runu, commit SHA, system, architekturę i wynik. Raport zawiera tylko podstawowe metadane środowiska i wersje narzędzi. Nie przechowuje tokenów.
4. Dopiero po potwierdzeniu na rzeczywistym runnerze wykonaj osobne, kontrolowane testy eksportera w odizolowanym środowisku bez danych produkcyjnych, zabezpieczone blokadą przed współbieżnym uruchomieniem.
5. Zmiana harmonogramu `wirr-reports-to-github.yml` wymaga odbioru E2E #122 i potwierdzenia, że istnieje **jedna** aktywna ścieżka przetwarzania kolejki.

## Stan i granice

- PR #124 zmienił synchronizację materiałów i publikację pakietu na self-hosted/manual. Nie potwierdzono jeszcze dostępności runnera dla tego repozytorium.
- PR #126 wprowadził warunkowe potwierdzenia Supabase; natywny test sieciowy nie został wykonany.
- Cykliczny eksporter raportów pozostaje bez zmian do czasu zweryfikowania zamiennika.
- Nie wolno uznawać HTTP 202 za potwierdzenie dostarczenia pliku do GitHub ani wykonywać testów produkcyjnymi raportami studentów.
- Działania muszą pozostać bez płatnych API lub GitHub-hosted runnerów.

Powiązania: #125 (zerowy koszt i runner), #122 (integralność raportów), #118 (Unity).
