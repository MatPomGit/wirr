"""Offline regressions for student-visible artifact validator."""
import tempfile
import unittest
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "scripts"))
from validate_student_public import validate_grades, validate_report

class PublicArtifactsTests(unittest.TestCase):
    def test_report_rejects_names(self):
        with tempfile.TemporaryDirectory() as d:
            path = Path(d) / "report.json"
            path.write_text('{"labNumber":1,"student":{"nazwisko":"X"}}', encoding="utf-8")
            self.assertTrue(validate_report(path))

    def test_report_allows_index(self):
        with tempfile.TemporaryDirectory() as d:
            path = Path(d) / "report.json"
            path.write_text('{"labNumber":1,"studentIndices":["123456"]}', encoding="utf-8")
            self.assertEqual(validate_report(path), [])

    def test_grades_reject_name_column(self):
        with tempfile.TemporaryDirectory() as d:
            path = Path(d) / "grades.csv"
            path.write_text("student_index,grade,name\n123456,4.0,Jan\n", encoding="utf-8")
            self.assertTrue(validate_grades(path))

    def test_grades_reject_duplicate_and_extra_column(self):
        with tempfile.TemporaryDirectory() as d:
            path = Path(d) / "grades.csv"
            path.write_text("student_index,grade\n123456,4.0\n123456,5.0,abc\n", encoding="utf-8")
            self.assertGreaterEqual(len(validate_grades(path)), 2)

    def test_grades_allow_index_only(self):
        with tempfile.TemporaryDirectory() as d:
            path = Path(d) / "grades.csv"
            path.write_text("student_index,grade\n123456,4.0\n123457,3.5\n", encoding="utf-8")
            self.assertEqual(validate_grades(path), [])

if __name__ == "__main__":
    unittest.main()
