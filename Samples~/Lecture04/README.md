# WiRR | Wykład 04: demonstrator Unity

**Typ:** opcjonalny przykład dydaktyczny, bez oceniania i raportowania.

**Wymagania:** Unity 6000.6.5f1, projekt WiRR URP z pakietami Input System i uGUI. Nie wymaga gogli, kamery, AR Foundation, sieci ani konta. Wszystkie wartości pomiarowe są syntetyczne.

## Import przez Unity Package Manager

1. Otwórz projekt WiRR (pakiet `pl.prz.kia.wirr`).
2. W Unity wybierz **Window → Package Manager**, pakiet **WiRR Course Toolkit** i zakładkę **Samples**.
3. Wybierz próbkę **Wykład 04: Śledzenie, stereo i okluzja** i naciśnij **Import**.
4. W folderze zaimportowanej próbki odnajdź `WiRRW04UnityDemo.cs`. Utwórz pustą scenę Unity, dodaj **GameObject → Create Empty**, nazwij `W04_Demo`, a następnie dołącz komponent `WiRRW04UnityDemo`.
5. Otwórz zakładkę **Game** w proporcjach **16:9**, kliknij **Play**. Jeżeli scena zawiera już kamerę, skieruj ją z pozycji około `(0,1.3,-3)` na obiekt około `(0,1.3,2)`. Skrypt tworzy kamerę i oświetlenie, gdy ich nie ma.

## Trzy tryby

### 1. Kotwica AR

Przyciski A–F prowadzą przez poprawną pozycję, utratę i odzyskanie śledzenia, błędną relokalizację **35 cm**, niezależny test referencji z przykładową tolerancją **5 cm** i korektę. Czerwona bryła to wizualizacja debugowa, zielona pojawia się po zaakceptowaniu stanu.

Ten pokaz demonstruje **logikę dopuszczenia nakładki**, nie odczyt SDK śledzenia ani rzeczywisty estymator SLAM. Śledzenie i poprawna rejestracja względem celu są odrębne.

### 2. Stereo

Dwie schematyczne kamery, promienie i ruchomy punkt 3D to wizualizacja **poza skalą**; dokładne obliczenia pochodzą z `Z = fB/d` dla `f = 800 px` i `B = 0,10 m`. Przełączanie `d: 40 → 20 px` zmienia `Z: 2 → 4 m` oraz modelową niepewność `σZ: 2,5 → 10 cm` dla `σd = 0,5 px`. Wyniki te nie są pomiarami żadnego urządzenia.

### 3. Okluzja

Niebieski prostopadłościan to **proxy geometrii rzeczywistej**, żółty to obiekt wirtualny. Zmieniaj `Z real` oraz `Z virtual` i wyłączaj dostępność lub synchronizację głębi. **Jasny kontur przeszkody pozostaje widoczny nawet przy braku danych głębi**. Wariant domyślny nie renderuje wtedy obiektu zależnego od depth; dodatkowy przełącznik celowo pokazuje błędne wyświetlanie hologramu w obszarze przeszkody. Kontur oznacza referencję geometryczną, a nie zrekonstruowaną głębię.

To pokaz klasycznego Z-bufora Unity, **nie** rzeczywista integracja z AR Depth API ani algorytm segmentacji.

## Ograniczenia i kontrola wydania

- Przykład jest tylko do prowadzenia wykładu. Nie zapisuje raportów studentów i nie przydziela ocen.
- W tej wersji nie ma rzeczywistej kamery stereo, sensorów IMU ani czujnika głębi.
- [ ] Zbudować i sprawdzić kompilację w docelowym Unity 6000.6.5f1.
- [ ] W Play Mode potwierdzić działanie kliknięć, suwaków, przejść A–F i trzech wizualizacji.
- [ ] Sprawdzić import przez Package Manager i pracę z projektem ustawionym na Input System-only.
- [ ] Nie synchronizować z `KIA-students/wirr` ani nie publikować jako gotowego sample bez wykonania powyższych testów.

Przykład stanowi pomoc dydaktyczną do wykładu **W04: Śledzenie i rozumienie sceny**.
