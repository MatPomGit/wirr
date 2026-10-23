#!/usr/bin/env python3
"""Export accepted WiRR reports from private Supabase queue to course GitHub repository."""
import base64
import hashlib
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
import uuid

BASE = "https://ndebunrsdtulflzpemwd.supabase.co"
TARGET = "KIA-students/wirr"
API = "https://api.github.com"
SUPABASE_KEY = os.environ.get("WIRR_SUPABASE_SERVICE_ROLE_KEY", "")
GITHUB_KEY = os.environ.get("KIA_STUDENTS_SYNC_TOKEN", "")

def request(method, url, headers, data=None):
    payload = None if data is None else json.dumps(data, ensure_ascii=False).encode("utf-8")
    req = urllib.request.Request(url, data=payload, headers=headers, method=method)
    with urllib.request.urlopen(req, timeout=25) as response:
        content = response.read()
        return json.loads(content) if content else {}

def auth_supabase():
    return {"apikey": SUPABASE_KEY, "Authorization": "Bearer " + SUPABASE_KEY, "Content-Type": "application/json"}

def auth_github():
    return {"Authorization": "Bearer " + GITHUB_KEY, "Accept": "application/vnd.github+json", "Content-Type": "application/json", "X-GitHub-Api-Version": "2022-11-28"}

def report_blob_sha(raw):
    """Git blob identity includes the object header, not just the file bytes."""
    return hashlib.sha1(b"blob " + str(len(raw)).encode("ascii") + b"\0" + raw).hexdigest()

def find_report_pr(name):
    # Include closed history before any write: a retry must respect a reviewer's decision.
    url = API + "/repos/" + TARGET + "/pulls?" + urllib.parse.urlencode({
        "head": TARGET.split("/")[0] + ":" + name, "base": "main",
        "state": "all", "per_page": 100})
    prs = request("GET", url, auth_github())
    if len(prs) > 1:
        raise RuntimeError("Multiple report pull requests; operator reconciliation required")
    return prs[0] if prs else None

def acknowledge_report(submission_id, path, sha, pr):
    receipt = {"status":"reviewed", "github_path":path, "github_commit_sha":sha,
               "github_pr_url":pr["html_url"], "last_error":None}
    # A successful HTTP status is not proof that a row was updated. Constrain
    # the write to pending submissions and require the exact returned receipt.
    params = urllib.parse.urlencode({
        "submission_id": "eq." + submission_id,
        "status": "eq.pending",
        "select": "submission_id,status,github_path,github_commit_sha,github_pr_url",
    })
    rows = request("PATCH", BASE + "/rest/v1/wirr_public_intake?" + params,
                   {**auth_supabase(), "Prefer":"return=representation"}, receipt)
    if not isinstance(rows, list) or len(rows) != 1:
        raise RuntimeError("Supabase did not confirm exactly one pending report update")
    expected = {"submission_id": submission_id, **{
        key: receipt[key] for key in
        ("status", "github_path", "github_commit_sha", "github_pr_url")
    }}
    if not isinstance(rows[0], dict) or any(rows[0].get(key) != value for key, value in expected.items()):
        raise RuntimeError("Supabase receipt does not match the confirmed GitHub report")
    print(f"GitHub PR confirmed: {pr['html_url']}")

def export_report(row):
    submission_id, group, lab = row["submission_id"], row["group_code"], row["lab_no"]
    if group not in ("G1", "G2") or type(lab) is not int or not 1 <= lab <= 8:
        raise ValueError("Invalid group or laboratory")
    try:
        uuid.UUID(submission_id)
    except (ValueError, TypeError, AttributeError):
        raise ValueError("Invalid submission identifier")
    path = f"student-work/{group}/lab{lab:02d}-{submission_id}.json"
    # Single source of truth: never publish if the group/lab differs from the validated path.
    if row["github_path"] != path:
        raise ValueError("Report path mismatch")
    if not isinstance(row["report"], dict) or type(row["report"].get("labNumber")) is not int or row["report"]["labNumber"] != lab:
        raise ValueError("Report laboratory mismatch")
    raw = (json.dumps(row["report"], ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    destination = API + "/repos/" + TARGET + "/contents/" + urllib.parse.quote(path, safe="/")
    name = "wirr-report/" + submission_id
    previous_pr = find_report_pr(name)
    if previous_pr and previous_pr.get("state") == "closed":
        if not previous_pr.get("merged_at"):
            raise RuntimeError("Report PR was closed without merging; operator review required")
        # Verify immutable merged content even when the source branch was deleted.
        sha = previous_pr.get("merge_commit_sha")
        if not sha:
            raise RuntimeError("Merged PR has no confirmed merge commit")
        merged_file = request("GET", destination + "?ref=" + urllib.parse.quote(sha), auth_github())
        if merged_file.get("sha") != report_blob_sha(raw):
            raise RuntimeError("Merged report content does not match the queued submission")
        acknowledge_report(submission_id, path, sha, previous_pr)
        return
    # Create a separate branch and a reviewable PR, never commit student reports to main directly.
    base = request("GET", API + "/repos/" + TARGET + "/git/ref/heads/main", auth_github())
    try:
        request("POST", API + "/repos/" + TARGET + "/git/refs", auth_github(),
                {"ref":"refs/heads/" + name, "sha":base["object"]["sha"]})
    except urllib.error.HTTPError as branch_error:
        if branch_error.code != 422: raise
    head = request("GET", API + "/repos/" + TARGET + "/git/ref/heads/" + urllib.parse.quote(name, safe="/"), auth_github())
    head_sha = head.get("object", {}).get("sha")
    if not head_sha:
        raise RuntimeError("GitHub did not confirm the report branch head")
    try:
        old = request("GET", destination + "?ref=" + urllib.parse.quote(head_sha), auth_github())
        existing_sha = old.get("sha")
    except urllib.error.HTTPError as missing:
        if missing.code != 404: raise
        existing_sha = None
    if existing_sha:
        if existing_sha != report_blob_sha(raw):
            raise RuntimeError("Submission content changed; preserve the existing report and use a new submission identifier")
        # Recover after a lost queue acknowledgement without another file write.
        sha = head_sha
    else:
        body = {"message":f"student-work: WiRR {group} Lab {lab:02d} report", "content":base64.b64encode(raw).decode(), "branch":name}
        commit = request("PUT", destination, auth_github(), body)
        sha = commit.get("commit",{}).get("sha")
    if not sha: raise RuntimeError("GitHub did not confirm report commit")
    # Re-read after the write to catch a PR closed/merged during this attempt.
    pr = find_report_pr(name)
    if pr and pr.get("state") != "open":
        raise RuntimeError("Report PR changed state; retry to reconcile its final content")
    if not pr:
        pr = request("POST",API + "/repos/" + TARGET + "/pulls",auth_github(),{
            "title":f"WiRR {group} Lab {lab:02d}: zgłoszenie raportu",
            "head":name,"base":"main",
            "body":"Automatyczne zgłoszenie z Unity. Sprawdź zawartość i uprawnienie do jej publikacji przed scaleniem."
        })
    if pr.get("head", {}).get("sha") != sha:
        raise RuntimeError("GitHub pull request head does not match the confirmed report commit")
    acknowledge_report(submission_id, path, sha, pr)

PAGE_SIZE = 30


def pending_reports():
    # Seek by immutable UUID: changing pending rows must not shift an offset page.
    cursor = None
    while True:
        params = {"select": "submission_id,group_code,lab_no,report,status,github_path",
                  "status": "eq.pending", "order": "submission_id.asc", "limit": PAGE_SIZE}
        if cursor:
            params["submission_id"] = "gt." + cursor
        rows = request("GET", BASE + "/rest/v1/wirr_public_intake?" + urllib.parse.urlencode(params), auth_supabase())
        if not isinstance(rows, list):
            raise RuntimeError("Queue response is not a list")
        for row in rows:
            yield row
        if len(rows) < PAGE_SIZE:
            return
        next_cursor = str(uuid.UUID(rows[-1]["submission_id"]))
        if cursor and uuid.UUID(next_cursor).int <= uuid.UUID(cursor).int:
            raise RuntimeError("Queue cursor did not advance")
        cursor = next_cursor


def main():
    if not SUPABASE_KEY or not GITHUB_KEY:
        print("Required WIRR_SUPABASE_SERVICE_ROLE_KEY and KIA_STUDENTS_SYNC_TOKEN secrets are missing.", file=sys.stderr)
        return 2
    processed = failed = 0
    try:
        for row in pending_reports():
            processed += 1
            try:
                export_report(row)
            except Exception as exc:
                failed += 1
                identifier = row.get("submission_id", "unknown") if isinstance(row, dict) else "unknown"
                print(f"Failed to publish {identifier}: {exc}", file=sys.stderr)
                # Global auth/rate failures should not spend requests on every student.
                if isinstance(exc, urllib.error.HTTPError) and exc.code in (401, 403, 429):
                    raise
                # Keep the failed report pending, then process the next independent report.
    except Exception as exc:
        print(f"Queue scan interrupted: {exc}", file=sys.stderr)
        return 1
    print(f"Queue scan complete: {processed} rows, {failed} failed")
    return 1 if failed else 0

if __name__ == "__main__":
    raise SystemExit(main())

