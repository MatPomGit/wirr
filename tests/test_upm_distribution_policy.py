"""Offline policy tests for student-facing Unity UPM distribution boundaries.

No Unity, network, CI or student data is required to run these checks.
"""
from __future__ import annotations

import importlib.util
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "publish_upm_channels.py"
SPEC = importlib.util.spec_from_file_location("publish_upm_channels", SCRIPT)
PUBLISH = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(PUBLISH)


class DistributionBoundaryTests(unittest.TestCase):
    def test_package_includes_required_unity_assets(self):
        included = (
            "package.json", "README.md", "LICENSE", "Runtime/WiRR2/WiRRStudentLab.cs",
            "Editor/WiRR2/WiRRStudentReportBridge.cs",
            "Tests/Editor/KIA.WiRR.Editor.Tests.asmdef",
            "Samples~/Lab01/WiRR2/Lab01_Station.unity",
            "Documentation~/Reference/QUICK_START.md",
            "WebSim~/site/index.html", "scripts~/wirr_grade_report.py",
            "Textures/Grid.meta", "Textures/Grid/checker.png",
        )
        for name in included:
            with self.subTest(name=name):
                self.assertTrue(PUBLISH.included(Path(name), "upm"))

    def test_no_internal_automation_or_student_records_in_upm(self):
        forbidden = (
            "tools/wirr2/test_sync_export_guard.py",
            "tools/wirr2/test_student_materials.py",
            "tools/unity-ci/run-windows.ps1",
            ".github/workflows/sync-kia-students-pr.yml",
            ".github/workflows/unity-windows-cli.yml",
            "scripts/export_wirr_reports.py",
            "competition/scoreboard.json", "student-work/submission.json",
            "tests/test_upm_distribution_policy.py", "AGENTS.md",
            "ARCHITECTURE.md", "STUDENT_WORKS.md",
            "Project~/Packages/manifest.json", "Textures/HDRI/sky.exr",
            "Textures/Surfaces/floor.png",
        )
        for name in forbidden:
            with self.subTest(name=name):
                self.assertFalse(PUBLISH.included(Path(name), "upm"))

    def test_optional_asset_channel_still_only_contains_heavy_textures(self):
        self.assertTrue(PUBLISH.included(Path("Textures/HDRI/sky.exr"), "assets"))
        self.assertTrue(PUBLISH.included(Path("Textures/Surfaces/floor.png"), "assets"))
        self.assertFalse(PUBLISH.included(Path("Runtime/WiRR2/WiRRStudentLab.cs"), "assets"))

    def test_public_export_keeps_sanitization_and_student_data_protection(self):
        workflow = (ROOT / ".github/workflows/sync-kia-students-pr.yml").read_text(encoding="utf-8")
        for path in (
            '"tools/wirr2"', '"tools/unity-ci"',
            '"tests/test_windows_unity_harness_policy.py"',
            '".github/workflows/unity-windows-cli.yml"',
        ):
            self.assertIn(path, workflow)
        self.assertIn('git checkout "upstream/${UPSTREAM_BRANCH}" -- student-work', workflow)
        self.assertIn('git checkout "upstream/${UPSTREAM_BRANCH}" -- competition', workflow)
        self.assertIn('if [ -n "${forbidden_matches}" ]; then', workflow)

    def test_student_release_is_manual_and_self_hosted(self):
        workflow = (ROOT / ".github/workflows/publish-student-upm.yml").read_text(encoding="utf-8")
        self.assertIn("workflow_dispatch:", workflow)
        self.assertNotIn("\n  push:", workflow)
        self.assertIn("runs-on: [self-hosted, Linux, ARM64]", workflow)
        self.assertIn("environment: student-package-publishing", workflow)


if __name__ == "__main__":
    unittest.main()
