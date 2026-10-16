# WiRR: potwierdzenie przyjęcia raportu, 16.10.2026

## Błąd i poprawka

`Editor/WiRRPublicReportSubmission.cs` zawierał parser `IsAcceptedResponse`,
ale `SubmitFile` zamiast niego szukał dokładnego fragmentu `"accepted":true`.
Odpowiedź `{ "accepted": true }` była odrzucana. Odpowiedź z niezgodnym
`submission_id` mogła zostać uznana za potwierdzenie bieżącej wysyłki.

Ścieżka HTTP wywołuje teraz istniejący parser. Nadal wymaga sukcesu HTTP,
a potwierdzenie jest zapisywane dopiero po zaakceptowaniu odpowiedzi.
Zachowano zgodność z odpowiedzią bez `submission_id`; jeśli serwer podaje
identyfikator, musi on odpowiadać wysyłanemu zgłoszeniu.

## Walidacja

- Uruchomiono `python -m unittest discover -s scripts~ -p 'test_wirr*.py' -v`:
  **14 testów PASS**, w tym 40 podprzypadków lab/próg. To testy walidatora
  raportów Python, nie parsera Unity ani transportu HTTP.
- Dodano **12 przypadków EditMode** w `Tests/Editor/WiRRSubmissionReceiptTests.cs`:
  JSON zwarty i sformatowany, kolejność pól, wielkość liter identyfikatora,
  brak potwierdzenia, odmowa, zagnieżdżone pole, obcy identyfikator i błędny JSON.
- Testy korzystają z rzeczywistego `JsonUtility` i parsera produkcyjnego.
  **Nie uruchomiono ich tutaj: środowisko nie zawiera Unity.**
- Projekt `Project~` deklaruje pakiet w `testables`, aby Test Runner
  mógł znaleźć jego testy. Zależność Test Framework już była w projekcie.

## Następny odbiór

W przygotowanym projekcie Unity uruchomić EditMode dla
`KIA.WiRR.Editor.Tests.WiRRSubmissionReceiptTests`. Następnie wykonać próbę
dwóch raportów z jednego bloku: zapis, przyjęcie przez serwer, ponowienie po
awarii i potwierdzony PR/commit w KIA-students/wirr. Sam plik `.receipt.json`
nie jest dowodem przekazania do GitHub ani zatwierdzenia oceny.

Nie zamykać odbioru WiRR 2.0 na podstawie tej poprawki. Nie wykonano wysyłki
testowej na serwer ani publikacji raportów studentów.
