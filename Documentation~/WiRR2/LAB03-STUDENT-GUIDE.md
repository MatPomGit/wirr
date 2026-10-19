# Laboratorium 03. Ocena dokładności i stabilności kotwic w symulowanej rzeczywistości rozszerzonej

**WiRR 2.0, blok 02, część 1. Czas: 90 minut. Środowisko: Unity 6, gotowa scena `Lab03_AnchorGallery`, tryb SIM_AR.**

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

Przed zapisaniem odpowiedzi upewnij się, że potrafisz rozróżnić dokładność, stabilność i ponowną lokalizację. W polu `Twój wniosek (jeden na laboratorium)` napisz kilka pełnych zdań: przedstaw porównane warunki, wskaż zauważoną różnicę błędu i rozrzutu, opisz zachowanie po utracie śledzenia oraz ograniczenie symulacji. Nie zastępuj wniosku samymi hasłami.

W panelu wybierz `Eksportuj dwa wyniki bloku`. W aktualnej wersji demonstratora eksport jest **lokalny** i zapisuje dwa pliki wyników oraz indeks bloku w katalogu wskazanym przez aplikację. Eksport nie potwierdza wysyłki ani zaliczenia. Po laboratorium 04 przekaż pliki sposobem wskazanym przez prowadzącego. Jeśli inne wydanie stanowiskowe wyświetla odrębne potwierdzenie odbioru na serwerze, kieruj się jego rzeczywistym statusem; nie zakładaj, że taki mechanizm działa w zestawie pilotażowym.

**Osiąganie poziomów:** 3.0 wymaga poprawnego umieszczenia obiektu; 3.5 porównania warunków; 4.0 wyjaśnienia błędu i rozrzutu; 4.5 uzasadnionego doboru profilu i powtórzenia; 5.0 próby utraty śledzenia, analizy powrotu i wskazania niepewności. Wybrany w panelu poziom jest wskazówką dydaktyczną, nie automatyczną oceną.

Materiały kursowe: https://github.com/KIA-students/wirr . Wersja instrukcji: 2026-10-19 (uzupełnienie materiału pilotażowego).
