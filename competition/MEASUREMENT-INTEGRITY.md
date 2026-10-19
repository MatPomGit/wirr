# WiRR: kontrola wiarygodności pomiarów studentów

Status: procedura oceny, a nie mechanizm automatycznego stwierdzania nieuczciwości.

## Zakres
1. Sprawdź źródło zgłoszenia (GitHub, archiwum PRz, kolejka), numer laboratorium, wersję raportu i datę `updatedAtUtc`. Kolejny eksport tych samych autorów nie tworzy nowej pracy.
2. Zidentyfikuj autorów według prywatnego rejestru PRz; raport może mieć 1–2 autorów. Rozbieżny identyfikator wyjaśnij przed przypisaniem oceny, nie edytuj oryginalnego JSON.
3. Przelicz mediany z prób surowych, wartości procentowe, FPS i milisekundy. `1000/FPS` jest przybliżeniem dla median czasu klatki i FPS obliczanych niezależnie: niewielka różnica nie świadczy o fałszerstwie.
4. Sprawdź warunki eksperymentu i konfigurację sceny; nie porównuj bezpośrednio wydajności różnych urządzeń, rozdzielczości lub wersji Unity.
5. Sprawdź powtarzalność przebiegów oraz podobieństwa między **niezależnymi** raportami. Wspólne stanowisko, szablony formularza i praca w parze mogą tłumaczyć podobieństwa. Czas PlayMode i liczba zdarzeń nie są samodzielną miarą jakości.
6. Dla anomalii zażądaj adekwatnego dowodu (surowe dane, Profiler, log zdarzeń, krótka demonstracja, ponowny pomiar); zapewnij możliwość wyjaśnienia błędu bez publicznych zarzutów.

## Wynik kontroli
Stosuj określenia: **potwierdzone źródłowo**, **spójne, lecz niezweryfikowane zewnętrznie**, **wymaga wyjaśnienia**. Rozbieżność lub brak logów nie dowodzi sfabrykowania wartości. Ocenę zatwierdza prowadzący, nie skrypt.

## Runda I, stan 19.10.2026
- Publiczne `student-work/`: 8 raportów JSON, przy czym 2 są kolejnymi eksportami jednej pracy Lab01.
- Prywatne archiwum PRz: cztery ZIP, razem 7 raportów (4 Lab01 i 3 Lab02).
- W sprawdzonych wypełnionych parach FPS/ms i medianach Lab02 z publicznych JSON nie znaleziono dużych niespójności arytmetycznych w zastosowanym screeningu; to nie dowodzi autentyczności.
- W prywatnym archiwum wykryto rozbieżność identyfikatora w dwóch raportach tej samej paczki. Powiązane zadanie: `MatPomGit/prz#198`.
- Pełne ustalenia dowodowe i działania prowadzącego: `MatPomGit/wirr#107`.
- Oficjalny ranking `KIA-students/wirr/competition/scoreboard.json` pozostaje z `null` dla niezatwierdzonych wyników. Nie zamieniać `null` na zero ani nie przyznawać punktów za liczbę zgłoszeń lub sam fakt pracy w parze.

## Ochrona danych
Raporty identyfikujące studentów, nazwiska, numery indeksów, informacje z prywatnych archiwów oraz oceny indywidualne pozostają w repozytorium PRz. Do publicznego repozytorium kursu trafiają wyłącznie anonimowe reguły i zagregowany postęp.
