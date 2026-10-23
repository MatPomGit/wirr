"""Non-network policy checks for the WiRR runner preflight workflow."""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
WORKFLOW = ROOT / ".github/workflows/wirr-selfhosted-preflight.yml"
EXPORT = ROOT / ".github/workflows/sync-kia-students-pr.yml"


class HomeServerPreflightPolicyTests(unittest.TestCase):
    def test_manual_only_and_main_bound(self):
        src = WORKFLOW.read_text(encoding="utf-8")
        self.assertIn("workflow_dispatch:", src)
        for trigger in ("pull_request:", "pull_request_target:", "schedule:", "  push:"):
            self.assertNotIn(trigger, src)
        self.assertIn("refs/heads/main", src)
        self.assertIn("github.repository == 'MatPomGit/wirr'", src)

    def test_only_arm64_selfhosted_and_no_secrets(self):
        src = WORKFLOW.read_text(encoding="utf-8")
        self.assertIn("runs-on: [self-hosted, Linux, ARM64]", src)
        self.assertIn("permissions:\n  contents: read", src)
        self.assertNotIn("ubuntu-latest", src)
        self.assertNotIn("secrets.", src)
        self.assertIn('test "$(git rev-parse HEAD)" = "${GITHUB_SHA}"', src)
        self.assertIn('for executable in git python3 gh bash', src)

    def test_preflight_evidence_has_no_token_values(self):
        src = WORKFLOW.read_text(encoding="utf-8")
        self.assertIn("actions/upload-artifact@v4", src)
        self.assertIn("retention-days: 7", src)
        self.assertNotIn("env |", src)
        self.assertNotIn("printenv", src)
        self.assertNotIn("git remote -v", src)

    def test_private_workflow_never_reaches_students(self):
        src = EXPORT.read_text(encoding="utf-8")
        self.assertIn('" .github/workflows/wirr-selfhosted-preflight.yml"'.strip(), src)
        self.assertIn('"tests/test_wirr_selfhosted_preflight_policy.py"', src)
        self.assertIn('"docs/operations"', src)
        self.assertIn('git checkout "upstream/${UPSTREAM_BRANCH}" -- student-work', src)
        self.assertIn('git checkout "upstream/${UPSTREAM_BRANCH}" -- competition', src)


if __name__ == "__main__":
    unittest.main()
