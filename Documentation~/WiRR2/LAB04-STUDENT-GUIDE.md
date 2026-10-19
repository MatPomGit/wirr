# Laboratorium 04. Okluzja i poprawność widoczności obiektów w symulowanej rzeczywistości rozszerzonej

**WiRR 2.0, blok 02, część 2. Czas: 90 minut. Środowisko: Unity 6, gotowa scena `Lab04_DepthRoom`, tryb SIM_AR.**

## Od pobrania materiałów do pierwszego uruchomienia

Poniższa procedura dotyczy laboratorium 03 i 04 w wersji komputerowej SIM_AR. Na stanowisku uczelnianym zacznij od kroku 4, jeśli prowadzący przygotował już Unity i projekt. Nie pobieraj ponownie plików bez potrzeby.

1. Otwórz stronę kursu: https://github.com/KIA-students/wirr . W zakładce README znajdziesz odnośniki do instrukcji oraz zasobów.
2. Jeżeli nie masz Unity, wejdź na https://unity.com/download , pobierz Unity Hub dla swojego systemu i zainstaluj go. W Unity Hub zaloguj się na własne konto Unity ID, aktywuj dostępną bezpłatną licencję, przejdź do **Installs → Install Editor** i wybierz zgodną wersję **Unity 6000.6.x**, wskazaną przez prowadzącego. Nie instaluj modułów Android, Quest ani ARCore dla ćwiczeń SIM_AR na PC.
3. Jeżeli przygotowujesz stanowisko samodzielnie, otwórz Unity Hub, wybierz **Projects → New project**, szablon **Universal 3D (URP)** i naciśnij **Create project**. W uruchomionym edytorze wejdź w **Window → Package Manager**, naciśnij **+ → Install package from git URL…**, wklej `https://github.com/KIA-students/wirr.git#upm` i zatwierdź **Install**. Pakiet instalowany tą metodą wymaga dostępnego narzędzia Git; jeżeli Unity zgłosi jego brak, skorzystaj z instrukcji [konfiguracji stanowiska](../Reference/OS_SETUP.md) albo zgłoś problem prowadzącemu. Nie pobieraj repozytorium jako ZIP, jeżeli dysponujesz gotowym projektem.
4. Otwórz **Unity Hub → Projects → Add** (jeżeli projekt nie jest jeszcze na liście), wskaż przygotowany katalog projektu Unity i uruchom go przyciskiem **Open**. Odczekaj na zakończenie importu pakietów. Jeżeli na stanowisku nie ma wskazanego projektu, zapytaj prowadzącego o jego lokalizację; samo pobranie PDF-u nie tworzy działającej sceny.
5. W górnym menu edytora wybierz **WiRR → Nowe instrukcje → Materiały studenta**. W panelu **Doświadczenia WiRR** kliknij przycisk otwierający scenę dla odpowiedniego laboratorium. Jeżeli przycisk jest nieaktywny, prowadzący może użyć **WiRR → Nowe instrukcje → Przygotuj osiem scen (prowadzący)**. Nie nadpisuj samodzielnie istniejących scen.
6. W panelu **Project** odszukaj w razie potrzeby scenę w `Assets/WiRR/WiRR2/`. Po jej otwarciu naciśnij przycisk **Play** (trójkąt w górnym pasku Unity). Interakcje wykonuj w widoku **Game**; widok **Scene** służy do edycji, a nie do rejestrowania próby.
7. Po lewej stronie widoku **Game** wpisz indeks (przy pracy w parze oba indeksy), wybierz **G1/G2** i kliknij **Rozpocznij / wznów sesję**. Dopiero teraz można nacisnąć **Rozpocznij próbę**. Ustawienia wybieraj przed rozpoczęciem każdej próby.

Ścieżka startowa na uczelnianym komputerze jest krótsza: **Unity Hub → Open przygotowanego projektu → WiRR → Nowe instrukcje → Materiały studenta → Otwórz odpowiednią scenę → Play → Rozpocznij / wznów sesję**.

Jeśli pracujesz na własnym komputerze, sprawdź najpierw [instrukcję Unity dla Windows i Ubuntu](../Reference/OS_SETUP.md). Wersja SIM_AR nie wymaga instalacji aplikacji na telefonie ani użycia gogli.

## Cel i przedmiot doświadczenia

Celem ćwiczenia jest sprawdzenie, w jaki sposób geometria sceny i informacja o głębi decydują o widoczności obiektów w rzeczywistości rozszerzonej. Student korzysta z przygotowanej sceny komputerowej zawierającej przeszkodę i trzy obiekty. W kolejnych próbach rozpoznaje błędne przesłanianie, porównuje dwa warianty renderowania, reguluje tolerancję testu głębi i sprawdza rezultat z innego punktu obserwacji.

Okluzja polega na tym, że powierzchnia znajdująca się bliżej obserwatora zasłania część lub całość obiektu położonego dalej. Test głębi rozstrzyga, która z porównywanych powierzchni jest bliższa kamerze. Operacja `raycast` wykrywa natomiast przecięcia promienia z geometrią sceny. Jest to inne zadanie niż określanie widoczności każdego fragmentu obrazu. Zmiana oświetlenia wpływa na wygląd obiektów, ale nie powinna zastępować poprawnej analizy głębi.

W demonstratorze głębia pochodzi ze znanej geometrii syntetycznej. Ocena referencyjna dotyczy promienia prowadzącego do środka obiektu, nie wszystkich jego pikseli. Nie jest to pomiar wykonany czujnikiem głębi telefonu.

## Przygotowanie stanowiska, około 10 minut

1. Otwórz udostępniony przez prowadzącego projekt Unity. Nie twórz własnego projektu ani nie instaluj SDK.
2. W panelu `WiRR → Nowe instrukcje → Materiały studenta` wybierz laboratorium 04 lub otwórz przygotowaną scenę `Lab04_DepthRoom`.
3. Włącz **Play**. Po prawej powinna być widoczna przeszkoda oraz trzy obiekty. Po lewej znajduje się panel sterowania i odpowiedzi.
4. Wprowadź ten sam indeks lub te same dwa indeksy i grupę G1/G2 co w laboratorium 03. Naciśnij `Rozpocznij / wznów sesję`.
5. Znajdź trzy obiekty: czerwony numer 1 znajduje się za przeszkodą, zielony numer 2 przed nią, a turkusowy numer 3 z boku. Wyświetlane etykiety mogą być celowo mylące. Rozpoznawaj obiekty również po barwie i geometrii.

## Ćwiczenie wstępne: poprawna kolejność przesłaniania, około 10 minut

Wybierz `Warunek bazowy`, `A: zła okluzja` i `3.0`. Ustaw `Tolerancja testu głębi` na wartość wyjściową, a `Przyciemnione oświetlenie` pozostaw wyłączone. Zaznacz `Trening (bez zapisu do porównania)` i naciśnij `Rozpocznij próbę`.

Dla każdego numeru aplikacja pyta `czy powinien być widoczny?`. Odpowiedz `Tak` albo `Nie` na podstawie geometrii, a nie tylko obrazu lub etykiety. Pomocniczy `Komunikat promienia SIM` może być celowo błędny. Zwróć uwagę na trzy odrębne źródła informacji: położenie obiektu, wygląd jego renderowania i komunikat promienia. Po ćwiczeniu wyłącz trening.

## Doświadczenie 1. Rozpoznanie błędnej widoczności, około 15 minut

1. Wybierz `Warunek bazowy`, `A: zła okluzja`, etap `3.0` oraz wyjściową tolerancję głębi.
2. Zostaw włączone przygotowane scenariusze mylących etykiet i błędnego komunikatu promienia. Upewnij się, że oświetlenie nie zostało zmienione.
3. Naciśnij `Rozpocznij próbę`. Dla trzech kolejnych obiektów odpowiedz, czy powinny być widoczne według geometrii.
4. Po trzech odpowiedziach odczytaj zapisany wynik: liczbę poprawnych odpowiedzi, fałszywych alarmów i pominiętych obiektów widocznych, o ile są dostępne w raporcie.
5. Opisz konkretną niezgodność między widocznością na ekranie, etykietą, komunikatem promienia i położeniem względem przeszkody. Nie traktuj kliknięcia jako dowodu poprawności.

## Doświadczenie 2. Porównanie wariantów okluzji, około 20 minut

1. Wykonaj zapisaną próbę dla `A: zła okluzja`.
2. Bez zmiany punktu widzenia, oświetlenia, tolerancji i ustawień błędnych etykiet przełącz na `B: głębia zgodna`.
3. Wykonaj identyczny zestaw trzech odpowiedzi. Porównaj liczby odpowiedzi poprawnych, fałszywych alarmów oraz pominięć.
4. W tabeli podaj wynik każdej serii i własną interpretację. Fałszywy alarm oznacza uznanie obiektu za widoczny mimo braku widoczności według referencji; pominięcie oznacza odrzucenie obiektu, który według referencji powinien być widoczny.
5. Na poziomie `4.0` wyjaśnij własnymi słowami, dlaczego wykrycie przecięcia promienia z obiektem nie jest równoznaczne z poprawnym zasłanianiem jego obrazu.

| Wskaźnik | A: zła okluzja | B: głębia zgodna |
|---|---|---|
| Poprawne odpowiedzi | | |
| Fałszywe alarmy | | |
| Pominięte obiekty widoczne | | |
| Widoczna niezgodność z geometrią | | |

## Doświadczenie 3. Tolerancja głębi i kontrpróba, około 20 minut

1. Wróć do konfiguracji, w której wykryto problem. Zapisz początkową `Tolerancję testu głębi` w metrach.
2. Zmień **wyłącznie** suwak tolerancji (dostępny zakres 0–2 m). Wybierz nową wartość, dla której spodziewasz się poprawy. Wykonaj próbę i porównaj wynik z bazowym. Nie zakładaj, że większa tolerancja jest zawsze lepsza.
3. Osobno włącz `Przyciemnione oświetlenie`, nie zmieniając innych parametrów, i zaobserwuj wygląd sceny. Następnie przywróć pierwotne oświetlenie. Wyjaśnij, dlaczego samo przyciemnienie nie stanowi naprawy geometrii.
4. Ustaw `Drugi widok / dystans`. Wykonaj nową próbę z wybraną tolerancją. Zestaw poprawność odpowiedzi w obu widokach.
5. Na poziomie `5.0` oceń, czy rozwiązanie dobrane dla pierwszego widoku zachowuje przydatność w drugim. Podaj ograniczenie metody wynikające z punktu obserwacji i sposobu wyznaczania referencji.

**Ważne:** Zmian parametrów dokonuj między próbami. Aplikacja unieważnia wynik, jeżeli konfiguracja zmieni się podczas aktywnego pomiaru. Porównania prowadź przy jednakowym zestawie obiektów i tej samej geometrii.

## Gdzie i jak przekazać raport z laboratoriów 03 i 04

Dla bloku 02 przygotuj **dwa odrębne wyniki**, po jednym z każdego laboratorium. W aktualnej scenie pilotażowej procedura jest następująca:

1. Ukończ próby laboratorium 03. W panelu po lewej sprawdź listę **Wyniki tego laboratorium** i wpisz uzasadniony wniosek w polu **Twój wniosek (jeden na laboratorium)**.
2. W panelu **Doświadczenia WiRR** otwórz **Lab04_DepthRoom** albo wybierz **Następny lab**. Zachowaj te same indeksy i grupę, następnie wykonaj laboratorium 04 i zapisz jego wniosek.
3. Po ukończeniu obydwu ćwiczeń kliknij w widoku **Game** przycisk **Eksportuj dwa wyniki bloku**. Przycisk działa, gdy sesja jest otwarta i nie trwa aktywna próba. Poczekaj na komunikat **Zapisano dwa wyniki lokalnie** wraz ze ścieżką folderu.
4. Aby odszukać pliki, otwórz **WiRR → Nowe instrukcje → Materiały studenta → Otwórz lokalne wyniki**. Następnie przejdź do `WiRR/exports/<identyfikator_sesji>/<identyfikator_eksportu>/`. Znajdziesz tam `WiRR-lab03.json`, `WiRR-lab04.json` i `block.json`. Nie zmieniaj zawartości JSON.
5. Wersja pilotażowa zapisuje `deliveryStatus=LOCAL_ONLY`; **nie wysyła raportu automatycznie do serwera ani na GitHub**. Jeśli na danym stanowisku jest dodatkowo wdrożony osobny formularz **WiRR → Raporty → Formularz raportu laboratoryjnego**, użyj go według instrukcji prowadzącego. Za potwierdzenie złożenia raportu uznawaj wyłącznie rzeczywisty komunikat odbioru przez serwer dla obu laboratoriów, nigdy sam zapis JSON.
6. **Gdy nie ma dostępnej działającej wysyłki lub potwierdzenia**, skorzystaj z procedury awaryjnej wskazanej w instrukcji kursowej: utwórz archiwum ZIP zawierające dwa pliki wynikowe JSON i prześlij je e-mailem na **m.pomianek@prz.edu.pl**. Użyj nazwy `WiRR-blok-02-G1-INDEKS.zip`, zastępując `G1` odpowiednią grupą i `INDEKS` własnym numerem (przy pracy w parze uwzględnij oba numery). Temat wiadomości: `WiRR blok 02, grupa G1, indeks(y)`. W parze wiadomość wysyła jedna osoba.
7. Jeśli nie można nawet wyeksportować JSON, pobierz szablon [raportu awaryjnego dla bloku 02](WiRR-blok-02-raport-awaryjny.md), wypełnij części dotyczące obu laboratoriów i wyślij plik na ten sam adres. Zachowaj kopię. Nie dołączaj projektu Unity, slajdów ani plików pozbawionych danych eksperymentalnych.

**Kontrola przed zakończeniem:** sprawdź oba numery laboratoriów, wszystkie indeksy, grupę, wykaz ważnych i nieważnych prób, wnioski oraz sposób dostarczenia plików. Zapis lokalny nie jest dowodem przyjęcia raportu przez serwer ani wystawienia oceny.

## Opracowanie wyników i zakończenie, około 15 minut

W polu `Twój wniosek (jeden na laboratorium)` opisz, na czym polegał wykryty błąd widoczności, jaki efekt przyniosła zmiana sposobu okluzji lub tolerancji głębi oraz czy poprawa występowała w obu pozycjach kamery. Rozdziel błędy etykiety, odpowiedzi promienia i obrazu.

Wybierz `Eksportuj dwa wyniki bloku` i sprawdź komunikat aplikacji wskazujący katalog. Pilotażowy zestaw Unity zapisuje wyniki **lokalnie**; nie wysyła automatycznie potwierdzenia do serwera. Przekaż pliki zgodnie z rzeczywistą instrukcją prowadzącego. Zapis lokalny ani wybór etapu `5.0` nie oznaczają zaliczenia.

**Osiąganie poziomów:** 3.0 wymaga poprawnego rozpoznania niezgodności; 3.5 porównania A/B; 4.0 wyjaśnienia różnicy między testem głębi a `raycast`; 4.5 świadomego doboru tolerancji i porównania przed/po; 5.0 kontrpróby w drugim widoku i wskazania ograniczenia. Interpretację i ocenę zatwierdza prowadzący.

Materiały kursowe: https://github.com/KIA-students/wirr . Wersja instrukcji: 2026-10-19 (uzupełnienie materiału pilotażowego).
