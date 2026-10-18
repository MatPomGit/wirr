"""Offline regression for Docker-visible Unity UPM package reference."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

SCRIPT = Path(__file__).resolve().parents[1] / "tools" / "unity-ci" / "prepare_project.py"
spec = importlib.util.spec_from_file_location("wirr_prepare_project", SCRIPT)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)

class DockerPackagePathTests(unittest.TestCase):
    def test_docker_package_reference_resolves_to_mount(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp) / "package"
            project = Path(temp) / "project"
            root.mkdir()
            (root / "package.json").write_text(
                json.dumps({"name": "pl.prz.kia.wirr", "dependencies": {}}),
                encoding="utf-8",
            )
            module.prepare(root, project, "6000.3.24f1", "/workspace/package")
            manifest = json.loads((project / "Packages" / "manifest.json").read_text())
            value = manifest["dependencies"]["pl.prz.kia.wirr"]
            self.assertEqual(value, "file:../../package")
            resolved = (Path("/workspace/project/Packages") / value[5:]).resolve()
            self.assertEqual(resolved, Path("/workspace/package"))

    def test_unexpected_container_mount_is_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp) / "package"
            root.mkdir()
            (root / "package.json").write_text(
                json.dumps({"name": "pl.prz.kia.wirr", "dependencies": {}}),
                encoding="utf-8",
            )
            with self.assertRaises(ValueError):
                module.prepare(root, Path(temp) / "project", "6000.3.24f1", "/tmp/package")

if __name__ == "__main__":
    unittest.main()
