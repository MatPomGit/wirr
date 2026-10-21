# Architektura WiRR development

Unity UPM w `Runtime/`, `Editor/`, `Samples~/`; testy w `Tests/`, narzędzia eksportu w `scripts/`. Formularz Unity zapisuje JSON lokalnie i przekazuje do kolejki Supabase, następnie eksporter tworzy weryfikowalny PR w `KIA-students/wirr`. Aktualna implementacja używa `student-work/`; migracja do `students-works/` jest **niezakończona**. Pozytywny HTTP nie dowodzi dostarczenia do GitHub.

Zasady danych: [STUDENT_WORKS.md](STUDENT_WORKS.md). Testy i wyniki nie mogą zawierać danych osobowych ani sekretów.
