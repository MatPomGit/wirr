"""Static regression gates for the manual Windows Unity acceptance harness.

These do not substitute for running Unity EditMode/PlayMode tests on Andromeda.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RUNNER = ROOT / "tools" / "unity-ci" / "run-windows.ps1"
WORKFLOW = ROOT / ".github" / "workflows" / "unity-windows-cli.yml"


class UnityWindowsRunnerPolicyTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.script = RUNNER.read_text(encoding="utf-8")
        cls.workflow = WORKFLOW.read_text(encoding="utf-8")

    def test_runner_resolves_repository_root_not_tools_directory(self):
        self.assertRegex(
            self.script,
            r"""Join-Path\s+\$PSScriptRoot\s+["']\.\./\.\.["']""",
        )

    def test_uses_fresh_staged_project_outside_checkout(self):
        self.assertIn("prepare_project.py", self.script)
        self.assertIn("--stage-package", self.script)
        self.assertIn("guid]::NewGuid", self.script)
        self.assertIn("Project for diagnostics:", self.script)

    def test_fail_closed_nunit_and_timeout(self):
        self.assertIn("validate_nunit.py", self.script)
        self.assertIn("--suite $mode", self.script)
        self.assertIn("WaitForExit($TimeoutMinutes * 60000)", self.script)
        self.assertIn('Remove-Item -LiteralPath $xml,$log', self.script)
        self.assertIn("if (-not $passed)", self.script)
        self.assertIn("if ($dirty)", self.script)

    def test_workflow_uses_correct_artifact_root_and_manual_trigger(self):
        self.assertIn('artifacts\\unity-native', self.script)
        self.assertIn('path: artifacts/unity-native/', self.workflow)
        self.assertIn("workflow_dispatch:", self.workflow)
        self.assertNotRegex(self.workflow, r"(?m)^\s+(?:push|pull_request):")


if __name__ == "__main__":
    unittest.main()
