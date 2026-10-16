"""Fault-injection tests with a local queue/GitHub double; no network or student data."""
import base64
import contextlib
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import unittest
from unittest.mock import patch
from urllib.error import HTTPError
from urllib.parse import parse_qs, urlsplit

SOURCE = Path(__file__).resolve().parents[1] / 'scripts' / 'export_wirr_reports.py'
SPEC = importlib.util.spec_from_file_location('wirr_exporter', SOURCE)
exporter = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(exporter)
ID = '4ac6f686-bbb1-42f3-9668-a678d9187a52'


def queue_row():
    return {'submission_id': ID, 'group_code': 'G1', 'lab_no': 1,
            'report': {'labNumber': 1, 'observation': 'Syntetyczna obserwacja: ąęł'},
            'status': 'pending', 'github_path': f'student-work/G1/lab01-{ID}.json'}


class Services:
    """Model durable side effects and an acknowledgement lost after PR creation."""
    def __init__(self):
        self.row = queue_row()
        self.calls = []
        self.branch = False
        self.branch_sha = 'base-commit'
        self.blob = None
        self.pr = None
        self.receipt = None
        self.fail_ack = False
        self.fail_pr = False
        self.lose_pr_response = False
        self.wrong_pr_head = False
        self.missing_commit = False
        self.merged_blob = None
        self.extra_pr = None
        self.close_on_file_read = False

    def request(self, method, url, headers, data=None):
        self.calls.append((method, url, copy.deepcopy(data)))
        if '/rest/v1/' in url:
            if method == 'GET':
                return [copy.deepcopy(self.row)] if self.row['status'] == 'pending' else []
            if method == 'PATCH':
                if self.fail_ack:
                    self.fail_ack = False
                    raise HTTPError(url, 503, 'acknowledgement unavailable', {}, None)
                self.receipt = copy.deepcopy(data)
                self.row.update(data)
                return {}
        if url.endswith('/git/ref/heads/main'):
            return {'object': {'sha': 'base-commit'}}
        if url.endswith('/git/refs') and method == 'POST':
            if self.branch:
                raise HTTPError(url, 422, 'reference exists', {}, None)
            self.branch = True
            return {}
        if '/git/ref/heads/wirr-report/' in url:
            if not self.branch:
                raise HTTPError(url, 404, 'branch deleted', {}, None)
            return {'object': {'sha': self.branch_sha}}
        if '/contents/' in url:
            if method == 'GET':
                blob = self.merged_blob if parse_qs(urlsplit(url).query).get('ref') == ['merge-commit'] else self.blob
                if blob is None:
                    raise HTTPError(url, 404, 'not found', {}, None)
                if self.close_on_file_read:
                    self.pr['state'] = 'closed'
                # Use git itself as the independent blob-identity oracle in the test.
                import subprocess
                sha = subprocess.run(['git', 'hash-object', '--stdin'], input=blob,
                                     capture_output=True, check=True).stdout.decode().strip()
                return {'sha': sha}
            if method == 'PUT':
                self.blob = base64.b64decode(data['content'])
                self.branch_sha = 'report-commit'
                return {} if self.missing_commit else {'commit': {'sha': self.branch_sha}}
        if '/pulls' in url:
            if method == 'GET':
                state = parse_qs(urlsplit(url).query).get('state', ['open'])[0]
                return [copy.deepcopy(pr) for pr in (self.pr, self.extra_pr)
                        if pr and (state == 'all' or pr['state'] == state)]
            if method == 'POST':
                if self.fail_pr:
                    self.fail_pr = False
                    raise HTTPError(url, 503, 'PR temporarily unavailable', {}, None)
                self.pr = {'html_url': 'https://github.com/KIA-students/wirr/pull/999999',
                           'state': 'open', 'merged_at': None, 'merge_commit_sha': None,
                           'head': {'sha': 'unrelated' if self.wrong_pr_head else self.branch_sha}}
                if self.lose_pr_response:
                    self.lose_pr_response = False
                    raise HTTPError(url, 503, 'PR response lost after creation', {}, None)
                return copy.deepcopy(self.pr)
        raise AssertionError(f'Unexpected request: {method} {url}')

    def count(self, method, path):
        return sum(m == method and path in url for m, url, _ in self.calls)


class TestWiRRReportExporter(unittest.TestCase):
    def run_export(self, services):
        with patch.object(exporter, 'request', services.request), \
                patch.object(exporter, 'SUPABASE_KEY', 'synthetic'), \
                patch.object(exporter, 'GITHUB_KEY', 'synthetic'), \
                contextlib.redirect_stdout(io.StringIO()), \
                contextlib.redirect_stderr(io.StringIO()):
            return exporter.main()

    def test_success_records_exact_commit_after_pr_confirmation(self):
        services = Services()
        self.assertEqual(self.run_export(services), 0)
        self.assertEqual(services.receipt['github_commit_sha'], services.pr['head']['sha'])
        self.assertEqual(services.receipt['status'], 'reviewed')
        self.assertEqual(json.loads(services.blob)['observation'], 'Syntetyczna obserwacja: ąęł')
        self.assertEqual(services.count('PUT', '/contents/'), 1)

    def test_lost_ack_retry_reuses_file_commit_and_pr(self):
        services = Services()
        services.fail_ack = True
        self.assertEqual(self.run_export(services), 1)
        self.assertEqual(services.row['status'], 'pending')
        self.assertIsNone(services.receipt)
        self.assertEqual(self.run_export(services), 0)
        self.assertEqual(services.count('PUT', '/contents/'), 1)
        self.assertEqual(services.count('POST', '/pulls'), 1)
        self.assertEqual(services.receipt['github_commit_sha'], 'report-commit')

    def test_retry_after_pr_failure_reuses_existing_file(self):
        services = Services()
        services.fail_pr = True
        self.assertEqual(self.run_export(services), 1)
        self.assertEqual(self.run_export(services), 0)
        self.assertEqual(services.count('PUT', '/contents/'), 1)
        self.assertEqual(services.receipt['github_commit_sha'], 'report-commit')

    def test_changed_payload_cannot_overwrite_existing_submission(self):
        services = Services()
        services.fail_ack = True
        self.assertEqual(self.run_export(services), 1)
        original = services.blob
        services.row['report']['observation'] = 'Zmieniona wersja'
        self.assertEqual(self.run_export(services), 1)
        self.assertEqual(services.blob, original)
        self.assertEqual(services.row['status'], 'pending')
        self.assertEqual(services.count('PUT', '/contents/'), 1)

    def test_lost_pr_response_retry_finds_the_created_pr(self):
        services = Services()
        services.lose_pr_response = True
        self.assertEqual(self.run_export(services), 1)
        self.assertIsNotNone(services.pr)
        self.assertEqual(self.run_export(services), 0)
        self.assertEqual(services.count('PUT', '/contents/'), 1)
        self.assertEqual(services.count('POST', '/pulls'), 1)

    def test_unrelated_pr_head_is_not_delivery_confirmation(self):
        services = Services()
        services.wrong_pr_head = True
        self.assertEqual(self.run_export(services), 1)
        self.assertEqual(services.count('PATCH', '/rest/v1/'), 0)
        self.assertEqual(services.row['status'], 'pending')

    def test_missing_commit_cannot_acknowledge_queue(self):
        services = Services()
        services.missing_commit = True
        self.assertEqual(self.run_export(services), 1)
        self.assertEqual(services.count('POST', '/pulls'), 0)
        self.assertEqual(services.count('PATCH', '/rest/v1/'), 0)

    def test_invalid_queue_rows_fail_before_github(self):
        for changes in ({'lab_no': True}, {'lab_no': 9}, {'group_code': 'G3'},
                        {'github_path': 'other/report.json'}, {'submission_id': '../other'},
                        {'report': []}, {'report': {'labNumber': 2}},
                        {'report': {'labNumber': True}}):
            with self.subTest(changes=changes):
                services = Services()
                services.row.update(changes)
                self.assertEqual(self.run_export(services), 1)
                self.assertEqual(sum(exporter.API in url for _, url, _ in services.calls), 0)

    def test_second_scan_after_success_does_no_github_work(self):
        services = Services()
        self.assertEqual(self.run_export(services), 0)
        calls = services.count('PUT', '/contents/')
        self.assertEqual(self.run_export(services), 0)
        self.assertEqual(services.count('PUT', '/contents/'), calls)

    def test_git_blob_identity_matches_known_git_object(self):
        self.assertEqual(exporter.report_blob_sha(b'hello\n'), 'ce013625030ba8dba906f756967f9e9ca394464a')
        self.assertNotEqual(exporter.report_blob_sha(b'hello\n'), hashlib.sha1(b'hello\n').hexdigest())

    def pending_after_lost_ack(self):
        services = Services()
        services.fail_ack = True
        self.assertEqual(self.run_export(services), 1)
        services.calls.clear()
        return services

    def assert_no_github_writes(self, services):
        self.assertEqual([(method, url) for method, url, _ in services.calls
                          if exporter.API in url and method != 'GET'], [])

    def test_closed_unmerged_pr_is_not_recreated_or_acknowledged(self):
        services = self.pending_after_lost_ack()
        services.pr['state'] = 'closed'
        for deleted in (False, True):
            with self.subTest(branch_deleted=deleted):
                services.branch = not deleted
                self.assertEqual(self.run_export(services), 1)
                self.assert_no_github_writes(services)
                self.assertIsNone(services.receipt)

    def test_merged_pr_recovers_after_branch_deletion_without_github_writes(self):
        services = self.pending_after_lost_ack()
        services.pr.update(state='closed', merged_at='2026-10-16T10:00:00Z',
                           merge_commit_sha='merge-commit')
        services.merged_blob = services.blob
        services.blob = None
        services.branch = False
        self.assertEqual(self.run_export(services), 0)
        self.assert_no_github_writes(services)
        self.assertEqual(services.receipt['github_commit_sha'], 'merge-commit')
        self.assertEqual(services.receipt['github_pr_url'], services.pr['html_url'])

    def test_merged_pr_with_missing_or_changed_content_stays_pending(self):
        for blob in (None, b'{"changed": true}'):
            with self.subTest(blob=blob):
                services = self.pending_after_lost_ack()
                services.pr.update(state='closed', merged_at='2026-10-16T10:00:00Z',
                                   merge_commit_sha='merge-commit')
                services.merged_blob = blob
                self.assertEqual(self.run_export(services), 1)
                self.assert_no_github_writes(services)
                self.assertIsNone(services.receipt)

    def test_merged_pr_without_commit_is_not_acknowledged(self):
        services = self.pending_after_lost_ack()
        services.pr.update(state='closed', merged_at='2026-10-16T10:00:00Z')
        self.assertEqual(self.run_export(services), 1)
        self.assert_no_github_writes(services)
        self.assertIsNone(services.receipt)

    def test_multiple_historical_prs_require_operator_reconciliation(self):
        services = self.pending_after_lost_ack()
        services.extra_pr = copy.deepcopy(services.pr)
        services.extra_pr.update(state='closed', html_url='https://github.com/KIA-students/wirr/pull/999998')
        self.assertEqual(self.run_export(services), 1)
        self.assert_no_github_writes(services)
        self.assertIsNone(services.receipt)

    def test_pr_closed_during_retry_is_not_acknowledged(self):
        services = self.pending_after_lost_ack()
        services.close_on_file_read = True
        self.assertEqual(self.run_export(services), 1)
        self.assertIsNone(services.receipt)
        self.assertEqual(services.count('POST', '/pulls'), 0)

    def test_merged_recovery_can_retry_lost_queue_acknowledgement(self):
        services = self.pending_after_lost_ack()
        services.pr.update(state='closed', merged_at='2026-10-16T10:00:00Z',
                           merge_commit_sha='merge-commit')
        services.merged_blob = services.blob
        services.fail_ack = True
        self.assertEqual(self.run_export(services), 1)
        self.assertEqual(self.run_export(services), 0)
        self.assert_no_github_writes(services)
        self.assertEqual(services.receipt['github_commit_sha'], 'merge-commit')


if __name__ == '__main__':
    unittest.main()
