# Laboratorium 04. Okluzja i poprawność widoczności obiektów w symulowanej rzeczywistości rozszerzonej

**WiRR 2.0, blok 02, część 2. Czas: 90 minut. Środowisko: Unity 6, gotowa scena `Lab04_DepthRoom`, tryb SIM_AR.**

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

## Opracowanie wyników i zakończenie, około 15 minut

W polu `Twój wniosek (jeden na laboratorium)` opisz, na czym polegał wykryty błąd widoczności, jaki efekt przyniosła zmiana sposobu okluzji lub tolerancji głębi oraz czy poprawa występowała w obu pozycjach kamery. Rozdziel błędy etykiety, odpowiedzi promienia i obrazu.

Wybierz `Eksportuj dwa wyniki bloku` i sprawdź komunikat aplikacji wskazujący katalog. Pilotażowy zestaw Unity zapisuje wyniki **lokalnie**; nie wysyła automatycznie potwierdzenia do serwera. Przekaż pliki zgodnie z rzeczywistą instrukcją prowadzącego. Zapis lokalny ani wybór etapu `5.0` nie oznaczają zaliczenia.

**Osiąganie poziomów:** 3.0 wymaga poprawnego rozpoznania niezgodności; 3.5 porównania A/B; 4.0 wyjaśnienia różnicy między testem głębi a `raycast`; 4.5 świadomego doboru tolerancji i porównania przed/po; 5.0 kontrpróby w drugim widoku i wskazania ograniczenia. Interpretację i ocenę zatwierdza prowadzący.

Materiały kursowe: https://github.com/KIA-students/wirr . Wersja instrukcji: 2026-10-19 (uzupełnienie materiału pilotażowego).
