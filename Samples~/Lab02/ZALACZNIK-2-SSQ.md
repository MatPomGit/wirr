<div style="break-before: page; page-break-before: always;"></div>

<p align="center"><img src="../../Documentation~/WiRR2/assets/prz.png" width="165" alt="Politechnika Rzeszowska"> &nbsp;&nbsp; <img src="../../Documentation~/WiRR2/assets/kia.png" width="165" alt="Katedra Informatyki i Automatyki"></p>

# Załącznik 2. Kwestionariusz objawów choroby symulatorowej (SSQ), laboratorium 02

**Cel:** ocena nasilenia objawów przed ekspozycją na środowisko immersyjne i bezpośrednio po niej. Jest to papierowa wersja istniejącego w WiRR formularza **Simulator Sickness Questionnaire (SSQ)**, zgodna co do kolejności **16 pozycji i zasad punktowania** z plikiem `WebSim~/site/ssq.mjs` w kursie.

**Ważne:** laboratorium 02 w standardowej wersji `DESKTOP_SIM` **nie wymaga gogli VR**. Jeśli nie było ekspozycji z użyciem HMD, zaznacz **N/D: bez ekspozycji immersyjnej** i nie interpretuj zmian jako skutku korzystania z VR. Kwestionariusz jest opcjonalną pomocą dydaktyczną, **nie stanowi diagnozy ani kryterium oceny laboratoryjnej**. W razie objawów przerwij ekspozycję i zgłoś sytuację prowadzącemu.

**Uczestnik (anonimowy kod, bez nazwiska i indeksu):** ______________  
**Data:** ______________  **Godzina przed:** __________ **Godzina po:** __________  
**Tryb:** ☐ HMD / VR ☐ PC bez HMD ☐ inny: __________ ☐ N/D, nie przeprowadzono ekspozycji  
**Czas rzeczywistej ekspozycji:** __________ min  **Urządzenie / warunki:** __________________

## Jednolita skala odpowiedzi SSQ, 0–3

| Kod | Nasilenie objawu |
|---|---|
| **0** | brak objawu (*none*) |
| **1** | niewielkie (*slight*) |
| **2** | umiarkowane (*moderate*) |
| **3** | silne (*severe*) |

Wypełnij **obie kolumny tylko wtedy, gdy rzeczywiście przeprowadzono pomiar przed i po ekspozycji**. Wpisz jedną liczbę 0–3 dla każdej pozycji. Puste pola nie oznaczają zera.

| Lp. | Objaw | Przed (0–3) | Po (0–3) |
|---:|---|:---:|:---:|
| 1 | Ogólny dyskomfort | ______ | ______ |
| 2 | Zmęczenie | ______ | ______ |
| 3 | Ból głowy | ______ | ______ |
| 4 | Zmęczenie oczu | ______ | ______ |
| 5 | Trudność z ustawieniem ostrości wzroku | ______ | ______ |
| 6 | Zwiększone wydzielanie śliny | ______ | ______ |
| 7 | Pocenie się | ______ | ______ |
| 8 | Nudności | ______ | ______ |
| 9 | Trudność z koncentracją | ______ | ______ |
| 10 | Uczucie pełności w głowie | ______ | ______ |
| 11 | Niewyraźne widzenie | ______ | ______ |
| 12 | Zawroty głowy przy otwartych oczach | ______ | ______ |
| 13 | Zawroty głowy przy zamkniętych oczach | ______ | ______ |
| 14 | Wrażenie wirowania | ______ | ______ |
| 15 | Dyskomfort w żołądku (odczuwanie żołądka) | ______ | ______ |
| 16 | Odbijanie | ______ | ______ |

## Punktowanie (opcjonalnie, dla kompletnego zestawu 16 odpowiedzi)

Oryginalne wagi Kennedy i wsp. (1993), stosowane także w aplikacji `ssq.mjs`:

- **N (nudności):** suma pozycji 1, 6, 7, 8, 9, 15, 16 × **9,54**.
- **O (zaburzenia okoruchowe):** suma pozycji 1, 2, 3, 4, 5, 9, 11 × **7,58**.
- **D (dezorientacja):** suma pozycji 5, 8, 10, 11, 12, 13, 14 × **13,92**.
- **TS (wynik całkowity):** suma wszystkich 16 odpowiedzi × **3,74**.

Dla kompletnych par oblicz `Δ = wynik po − wynik przed` osobno dla N, O, D i TS. Ujemna różnica oznacza zmniejszenie punktacji, nie dowód skuteczności interwencji. Brak choć jednej odpowiedzi oznacza, że danego zestawu **nie punktuje się**, zamiast uzupełniać brak zerem.

| Wskaźnik | Przed | Po | Różnica (po − przed) |
|---|---|---|---|
| N | __________ | __________ | __________ |
| O | __________ | __________ | __________ |
| D | __________ | __________ | __________ |
| TS | __________ | __________ | __________ |

## Ograniczenia, bezpieczeństwo i poufność

Jest to opis subiektywnych objawów i warunków użycia. **Nie należy** na podstawie jednego wyniku orzekać o chorobie, zdolności do korzystania z VR ani o efekcie samego interfejsu bez kontroli warunków. Kwestionariusz zawiera **dane o samopoczuciu**. Nie zapisuj w nim numeru indeksu, nazwiska ani danych kontaktowych; nie przesyłaj odpowiedzi do ogólnodostępnego repozytorium ani standardowego raportu Supabase. Jakiekolwiek badawcze przetwarzanie odpowiedzi wymaga odrębnej podstawy prawnej i procedury zgody.

**Uwagi o przerwaniu lub braku pomiaru:** _______________________________________________

______________________________________________________________________________________

**Źródło narzędzia:** Kennedy, R. S., Lane, N. E., Berbaum, K. S. i Lilienthal, M. G. (1993). *Simulator Sickness Questionnaire: An Enhanced Method for Quantifying Simulator Sickness*. *The International Journal of Aviation Psychology, 3*(3), 203–220. https://doi.org/10.1207/s15327108ijap0303_3
