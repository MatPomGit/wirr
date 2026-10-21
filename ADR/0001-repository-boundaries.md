# ADR-0001: osobne repozytoria development i produkcji

Status: przyjęta zasada, 2026-10-21.

Rozwój Unity i testy prowadzimy w `MatPomGit/wirr`, a zatwierdzone materiały dla studentów w `KIA-students/wirr`. PRZ odpowiada za materiały dydaktyczne i chronione dane. Autopilot koordynuje testy i promocję wersji, ale nie zatwierdza ocen. Skutek: studenci otrzymują tylko linki produkcyjne, a migracja raportów musi zachować idempotencję i ślady odbioru.
