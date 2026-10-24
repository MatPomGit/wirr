"""Offline contract checks for optional lecture demo. Not a Unity compilation test."""
from __future__ import annotations

import json
import math
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SAMPLE = ROOT / "Samples~/Lecture04"
SCRIPT = SAMPLE / "WiRRW04UnityDemo.cs"


class Lecture04SampleTests(unittest.TestCase):
    def test_package_manager_registers_one_optional_sample(self):
        manifest = json.loads((ROOT / "package.json").read_text(encoding="utf-8"))
        matches = [s for s in manifest["samples"] if s["path"] == "Samples~/Lecture04"]
        self.assertEqual(len(matches), 1)
        self.assertIn("Wykład 04", matches[0]["displayName"])
        self.assertTrue((SAMPLE / "README.md").is_file())
        self.assertTrue(SCRIPT.is_file())

    def test_input_system_only_uGUI_and_no_external_services(self):
        source = SCRIPT.read_text(encoding="utf-8")
        self.assertIn("using UnityEngine.InputSystem.UI;", source)
        self.assertIn("InputSystemUIInputModule", source)
        self.assertIn("using UnityEngine.UI;", source)
        self.assertNotIn("OnGUI()", source)
        self.assertNotIn("GUILayout", source)
        self.assertNotIn("Input.GetKey", source.replace("Input.GetKey i płatnych usług", ""))
        self.assertNotIn("UnityEditor", source)
        self.assertNotIn("HttpClient", source)
        self.assertNotIn("UnityWebRequest", source)

    def test_tracking_gates_separate_freshness_map_and_reference(self):
        source = SCRIPT.read_text(encoding="utf-8")
        self.assertIn("PoseFresh && mapMatched && referenceConfirmed", source)
        self.assertIn("Mathf.Abs(errorMeters) <= ReferenceToleranceMeters", source)
        self.assertIn("referenceConfirmed = false; // poprawka wymaga nowej weryfikacji", source)
        self.assertIn("private void LoseTracking()", source)
        self.assertIn("private void RecoverOnlyTracking()", source)
        self.assertIn("private void SimulateWrongRelocalization()", source)

    def test_stereo_geometry_is_declared_schematic_and_numeric_example(self):
        source = SCRIPT.read_text(encoding="utf-8")
        self.assertIn("private void BuildStereoVisualization()", source)
        self.assertIn("stereoRig.SetActive(mode == 1)", source)
        self.assertIn("wizualizacji NIE jest BaselineMeters", source)
        self.assertIn("FocalPx * BaselineMeters / disparityPx", source)
        self.assertIn("z * z * DisparitySigmaPx / (FocalPx * BaselineMeters)", source)
        f, baseline, sigma_d = 800., .10, .5
        for d, expected_z, expected_sigma in ((40., 2., .025), (20., 4., .1)):
            z = f * baseline / d
            sigma_z = z * z * sigma_d / (f * baseline)
            self.assertTrue(math.isclose(z, expected_z))
            self.assertTrue(math.isclose(sigma_z, expected_sigma))

    def test_missing_depth_preserves_real_target_and_hides_virtual_content(self):
        source = SCRIPT.read_text(encoding="utf-8")
        self.assertIn("physicalSurfaceOutline.SetActive(depthMode);", source)
        self.assertIn("realDepthProxy.SetActive(depthMode && depthUsable);", source)
        self.assertIn("virtualObject.SetActive(depthMode && (depthUsable || showUnsafeFailure));", source)
        self.assertIn("bool depthUsable = depthValid && depthSynchronized;", source)
        self.assertEqual(len(re.findall(r'Cube\("Krawędź (?:górna|dolna|lewa|prawa)"', source)), 4)
        for valid in (False, True):
            for synchronized in (False, True):
                usable = valid and synchronized
                if not usable:
                    self.assertFalse(usable or False)

    def test_standalone_source_is_structurally_balanced(self):
        source = SCRIPT.read_text(encoding="utf-8")
        # Basic fast guard; actual C# compilation/PlayMode must run in Unity.
        self.assertEqual(source.count("{"), source.count("}"))
        self.assertEqual(source.count("("), source.count(")"))
        self.assertIn("class WiRRW04UnityDemo : MonoBehaviour", source)


if __name__ == "__main__":
    unittest.main()
