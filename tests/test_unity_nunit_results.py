"""Reject false-green results independently of Unity process exit status."""
import importlib.util
from pathlib import Path
import tempfile
import unittest

FILE = Path(__file__).resolve().parents[1] / 'tools/unity-ci/validate_nunit.py'
spec = importlib.util.spec_from_file_location('nunit', FILE)
nunit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(nunit)


class NUnitResultTests(unittest.TestCase):
    def check(self, text):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'results.xml'
            path.write_text(text)
            return nunit.validate(path, 'PlayMode')

    def xml(self, result='Passed', **attrs):
        values = {'total': '1', 'result': 'Passed', **attrs}
        attributes = ' '.join(f'{key}="{value}"' for key, value in values.items())
        return f'<test-run {attributes}><test-case fullname="WiRRStudentKitPlayModeTests.World" result="{result}"/></test-run>'

    def test_complete_playmode_suite_passes(self):
        methods = ('AllEightLessonsCreateADesktopWorld', 'CadLodSwitchesActualMeshAndReturnsToHigh',
                   'InteractionTeleportHasStableHomeAndDoesNotMoveTheTask',
                   'AnchorViewChangesPreserveReferenceAndHideMissingPose',
                   'DepthReferenceIsIndependentOfRenderingLabelsAndLight',
                   'CadMaterialsPreserveMeshAndReferenceScale',
                   'ReplayStaleReactionPreservesPoseAndRestoresVisibility',
                   'MissionTransferChangesDistanceWithoutMovingGoals')
        cases = ''.join(f'<test-case fullname="WiRRStudentKitPlayModeTests.{method}" result="Passed"/>' for method in methods)
        self.assertEqual(self.check('<test-run total="8" result="Passed">' + cases + '</test-run>')['passed'], 8)

    def test_partial_wirr_suite_is_rejected(self):
        with self.assertRaises(ValueError):
            self.check(self.xml())

    def test_missing_unknown_or_skipped_case_result_is_rejected(self):
        for result in ('Skipped', 'Inconclusive', 'Failed', ''):
            with self.subTest(result=result), self.assertRaises(ValueError):
                self.check(self.xml(result))

    def test_inconsistent_counts_and_missing_root_result_are_rejected(self):
        for text in (self.xml(total='2'), self.xml(skipped='1'), self.xml(result=''), '<test-run total="0" result="Passed"/>'):
            with self.subTest(text=text), self.assertRaises(ValueError):
                self.check(text)

    def test_unrelated_suite_does_not_validate_wirr(self):
        with self.assertRaises(ValueError):
            self.check(self.xml().replace('WiRRStudentKitPlayModeTests.World', 'Unrelated.Test'))

    def test_duplicate_case_names_are_rejected(self):
        text = self.xml(total='2').replace('</test-run>', '<test-case fullname="WiRRStudentKitPlayModeTests.World" result="Passed"/></test-run>')
        with self.assertRaises(ValueError):
            self.check(text)
