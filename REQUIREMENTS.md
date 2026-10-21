# Wymagania development WiRR

Unity **6000.6.5f1** jako docelowa wersja; UPM, OpenXR i XRI zgodnie z manifestem projektu. Przed wydaniem wymagaj poprawnego importu, EditMode i PlayMode, weryfikacji idempotencji eksportu i podpisanego SHA. Tryb wsadowy Unity: `Unity -batchmode -nographics -quit -projectPath <project> -runTests -testPlatform EditMode -testResults <results.xml> -logFile <editor.log>`. Rzeczywisty test na runnerze jest wymagany; sama instrukcja nie potwierdza działania.
