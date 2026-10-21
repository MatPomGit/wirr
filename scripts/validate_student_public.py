#!/usr/bin/env python3
"""Validate public student submissions and index-only grade CSV (no network)."""
import argparse
import csv
import json
import re
from pathlib import Path

INDEX = re.compile(r"^[0-9]{5,10}$")
PII_KEYS = {"name", "firstname", "lastname", "first_name", "last_name", "full_name", "email", "pesel", "nazwisko", "imie", "imię", "surname"}
ALLOWED_GRADES = {"2.0", "3.0", "3.5", "4.0", "4.5", "5.0"}

def inspect(value, source="$"):
    errors = []
    if isinstance(value, dict):
        for key, item in value.items():
            if str(key).lower().strip() in PII_KEYS:
                errors.append(f"{source}.{key}: forbidden identifying field")
            errors.extend(inspect(item, f"{source}.{key}"))
    elif isinstance(value, list):
        for i, item in enumerate(value):
            errors.extend(inspect(item, f"{source}[{i}]"))
    return errors

def validate_report(path):
    try:
        data = json.loads(Path(path).read_text(encoding="utf-8-sig"))
    except (OSError, ValueError) as ex:
        return [f"{path}: invalid JSON: {ex}"]
    if not isinstance(data, dict):
        return [f"{path}: JSON root must be object"]
    return [f"{path}: {error}" for error in inspect(data)]

def validate_grades(path):
    errors = []
    try:
        with Path(path).open(newline="", encoding="utf-8-sig") as fh:
            table = csv.DictReader(fh)
            if table.fieldnames != ["student_index", "grade"]:
                return [f"{path}: header must be student_index,grade"]
            seen = set()
            for line, row in enumerate(table, 2):
                idx = (row.get("student_index") or "").strip()
                grade = (row.get("grade") or "").strip()
                if not INDEX.fullmatch(idx):
                    errors.append(f"{path}:{line}: invalid index")
                if grade not in ALLOWED_GRADES:
                    errors.append(f"{path}:{line}: invalid grade")
                if idx in seen:
                    errors.append(f"{path}:{line}: duplicate index")
                seen.add(idx)
                if None in row:
                    errors.append(f"{path}:{line}: extra columns")
    except (OSError, UnicodeError) as ex:
        errors.append(f"{path}: {ex}")
    return errors

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--report", action="append", default=[])
    parser.add_argument("--grades")
    args = parser.parse_args()
    errors = []
    for path in args.report:
        errors.extend(validate_report(path))
    if args.grades:
        errors.extend(validate_grades(args.grades))
    if not args.report and not args.grades:
        parser.error("Provide --report and/or --grades")
    for error in errors:
        print("ERROR:", error)
    if errors:
        print(f"FAIL: {len(errors)} violations")
        return 1
    print("PASS: public-file structural/PII key checks")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
