# Laboratorium 03. Ocena dokładności i stabilności kotwic w symulowanej rzeczywistości rozszerzonej

**WiRR 2.0, blok 02, część 1. Czas: 90 minut. Środowisko: Unity 6, gotowa scena `Lab03_AnchorGallery`, tryb SIM_AR.**

## Cel i przedmiot doświadczenia

Celem ćwiczenia jest zbadanie, czy stabilność położenia obiektu w rzeczywistości rozszerzonej oznacza jednocześnie jego dokładne umiejscowienie. Student pracuje na komputerze z przygotowanym projektem Unity i sceną symulacyjną. Nie jest potrzebny telefon, zestaw XR ani własny kod. W trakcie ćwiczenia student umieszcza obiekt przy znanym punkcie odniesienia, porównuje dwa modele jakości śledzenia i analizuje powrót obiektu po utracie śledzenia.

Kotwica przestrzenna (ang. *spatial anchor*) jest mechanizmem służącym do utrzymywania położenia obiektu w wybranym układzie odniesienia. Nie należy jej utożsamiać z wykryciem płaszczyzny ani z wyznaczeniem punktu przecięcia promienia z geometrią. Dokładność położenia odnosi się do zgodności z niezależnie określoną pozycją referencyjną. Stabilność opisuje natomiast zmienność tego położenia w czasie. Niewielki rozrzut odczytów może więc współistnieć z dużym błędem systematycznym.

W tej scenie trzy znaczniki są rozmieszczone co 0,30 m. Środkowy stanowi punkt odniesienia. Żółta kula reprezentuje model umieszczony przy kotwicy, a linia wskazuje różnicę względem referencji. Błąd jest wyznaczany na podstawie danych syntetycznych SIM_AR. Nie wolno przedstawiać go jako pomiaru dokładności urządzenia rzeczywistego.

## Przygotowanie stanowiska, około 10 minut

1. Uruchom przygotowany przez prowadzącego projekt Unity. Jeżeli pojawia się okno wyboru projektu, poproś o wskazanie właściwego katalogu; nie twórz nowego projektu.
2. Otwórz panel `WiRR → Nowe instrukcje → Materiały studenta` lub scenę `Lab03_AnchorGallery` udostępnioną w projekcie. Wybierz laboratorium 03, jeżeli panel otworzy inne ćwiczenie.
3. Uruchom scenę przyciskiem **Play** w edytorze Unity. W panelu po lewej powinny być widoczne polecenia, a w widoku po prawej trzy znaczniki i obiekt symulacyjny.
4. Wprowadź numer indeksu (od 5 do 12 cyfr). Podczas pracy dwuosobowej wpisz także drugi indeks. Wybierz G1 lub G2 i naciśnij `Rozpocznij / wznów sesję`. Profil obejmuje laboratoria 03 i 04.
5. Sprawdź napis `SIM_AR`. Jeżeli brakuje sceny, interfejsu lub materiałów, zgłoś problem prowadzącemu. Nie instaluj pakietów ani SDK.

## Ćwiczenie wstępne: zrozumienie obrazu, około 10 minut

Wskaż środkowy znacznik referencyjny, dwa sąsiednie znaczniki oddalone o 0,30 m oraz żółty obiekt. Zwróć uwagę na to, że zmiana obserwowanej pozycji obiektu nie musi oznaczać zmiany pozycji referencji. Wybierz w panelu etap `3.0` i zaznacz `Trening (bez zapisu do porównania)`. Wybierz profil `A: wiele cech`, naciśnij `Rozpocznij próbę`, a następnie `Umieść model przy referencji`. Obserwuj odczyty w centymetrach. Po treningu usuń zaznaczenie trybu treningowego.

## Doświadczenie 1. Umieszczenie i odczyt błędu, około 15 minut

1. Wybierz `A: wiele cech`. Pozostaw opcję utraty śledzenia wyłączoną.
2. Wybierz etap `3.0`, a następnie naciśnij `Rozpocznij próbę` oraz `Umieść model przy referencji`.
3. Poczekaj na zakończenie trzech automatycznych odczytów, wykonywanych w odstępach jednej sekundy na przygotowanej trasie. Nie zmieniaj konfiguracji w trakcie pomiaru.
4. Odszukaj wynik próby w sekcji `Wyniki tego laboratorium`. Zanotuj status, trzy wartości błędu (jeżeli są dostępne), jednostkę i oznaczenie warunku.
5. Sprawdź, czy wynik dotyczy pozycji względem referencji. Brak pomiaru lub utrata śledzenia oznacza brak ważnej wartości, a nie błąd równy 0 cm.

## Doświadczenie 2. Porównanie dwóch warunków, około 20 minut

1. Wybierz `3.5` i wykonaj poprawną próbę dla `A: wiele cech`, bez utraty śledzenia.
2. Zmień wyłącznie profil na `B: mało cech`; pozostałe ustawienia pozostaw bez zmian. Wykonaj drugą próbę.
3. Sporządź poniższe zestawienie. Korzystaj z wyników zapisanych przez scenę, a nie z wartości wymyślonych lub odczytanych ze schematu.
4. Porównaj wartość typową błędu oraz jego zmienność. Jeżeli raport zawiera wektory różnicy położenia, rozróżnij ich kierunki od długości wektora. Oblicz rozstęp jako różnicę między największym i najmniejszym ważnym odczytem.
5. Na etapie `4.0` wyjaśnij, dlaczego nawet niewielki rozstęp trzech odczytów nie dowodzi małego błędu względem referencji.

| Wskaźnik | A: wiele cech | B: mało cech |
|---|---|---|
| Liczba ważnych odczytów | | |
| Błąd minimalny [cm] | | |
| Błąd maksymalny [cm] | | |
| Rozstęp [cm] | | |
| Obserwacja o stabilności | | |

Porównanie ma sens tylko przy jednakowej referencji, trasie obserwacji, liczbie próbek i wersji modelu. Symulacja wykorzystuje ustalone dane i parametry, więc zaobserwowane różnice nie są automatycznie uogólnialne na rzeczywiste urządzenia AR.

## Doświadczenie 3. Utrata śledzenia i ponowna lokalizacja, około 20 minut

1. Na podstawie poprzednich prób wskaż profil, dla którego położenie obiektu było bliższe referencji. Zapisz uzasadnienie.
2. Włącz `Kontrpróba: utrata śledzenia i ponowna lokalizacja`. Opcję `Scenariusz graniczny: brak odzyskania śledzenia` na razie pozostaw wyłączoną.
3. Wykonaj próbę dla wybranego profilu. Po początkowych trzech odczytach symulator zgłosi utratę śledzenia. Oczekuj na przywrócenie śledzenia i trzy dalsze odczyty.
4. Porównaj błąd przed utratą i po powrocie. Zapisz czas odzyskania, jeżeli wynik go udostępnia. Nie utożsamiaj ponownego pojawienia się obiektu z powrotem dokładnie do wcześniejszej pozycji.
5. Dla etapu `5.0` wykonaj osobną próbę z włączoną opcją braku odzyskania. Odnotuj, że brak odzyskania nie oznacza czasu równego zero; jest zdarzeniem nieukończonym w zadanym oknie obserwacji.
6. Uzupełnij wniosek, wskazując, które właściwości wynikają z modelu SIM_AR, a których nie można wnioskować o rzeczywistym urządzeniu bez niezależnego pomiaru.

## Opracowanie wyników i zakończenie, około 15 minut

Przed zapisaniem odpowiedzi upewnij się, że potrafisz rozróżnić dokładność, stabilność i ponowną lokalizację. W polu `Twój wniosek (jeden na laboratorium)` napisz kilka pełnych zdań: przedstaw porównane warunki, wskaż zauważoną różnicę błędu i rozrzutu, opisz zachowanie po utracie śledzenia oraz ograniczenie symulacji. Nie zastępuj wniosku samymi hasłami.

W panelu wybierz `Eksportuj dwa wyniki bloku`. W aktualnej wersji demonstratora eksport jest **lokalny** i zapisuje dwa pliki wyników oraz indeks bloku w katalogu wskazanym przez aplikację. Eksport nie potwierdza wysyłki ani zaliczenia. Po laboratorium 04 przekaż pliki sposobem wskazanym przez prowadzącego. Jeśli inne wydanie stanowiskowe wyświetla odrębne potwierdzenie odbioru na serwerze, kieruj się jego rzeczywistym statusem; nie zakładaj, że taki mechanizm działa w zestawie pilotażowym.

**Osiąganie poziomów:** 3.0 wymaga poprawnego umieszczenia obiektu; 3.5 porównania warunków; 4.0 wyjaśnienia błędu i rozrzutu; 4.5 uzasadnionego doboru profilu i powtórzenia; 5.0 próby utraty śledzenia, analizy powrotu i wskazania niepewności. Wybrany w panelu poziom jest wskazówką dydaktyczną, nie automatyczną oceną.

Materiały kursowe: https://github.com/KIA-students/wirr . Wersja instrukcji: 2026-10-19 (uzupełnienie materiału pilotażowego).
