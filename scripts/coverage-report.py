#!/usr/bin/env python3
"""Summarise a Cobertura coverage file as per-source-file line/branch rates.

Usage: coverage-report.py <cobertura.xml> [path-substring-filter]

Aggregates every <class> whose filename matches the filter (default: the
src/BuildingBlock/Foundation/Querying* trees) and prints a table sorted by
filename, followed by the overall totals.
"""
import re
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict


def main() -> int:
    if len(sys.argv) < 2:
        print(__doc__)
        return 2

    path = sys.argv[1]
    needle = sys.argv[2] if len(sys.argv) > 2 else "src/BuildingBlock/Foundation/Querying"

    root = ET.parse(path).getroot()
    lines_covered = defaultdict(int)
    lines_valid = defaultdict(int)
    branches_covered = defaultdict(int)
    branches_valid = defaultdict(int)

    for cls in root.iter("class"):
        filename = cls.get("filename", "")
        if needle not in filename:
            continue
        lines = cls.find("lines")
        if lines is None:
            continue
        for line in lines.findall("line"):
            lines_valid[filename] += 1
            if line.get("hits", "0") != "0":
                lines_covered[filename] += 1
            cond = line.get("condition-coverage")
            if cond:
                match = re.search(r"\((\d+)/(\d+)\)", cond)
                if match:
                    branches_covered[filename] += int(match.group(1))
                    branches_valid[filename] += int(match.group(2))

    def rate(covered: int, valid: int) -> float:
        return (100.0 * covered / valid) if valid else 100.0

    print(f"{'line%':>6} {'branch%':>7}  {'lines':>7}  {'branches':>8}  file")
    total_lc = total_lv = total_bc = total_bv = 0
    for filename in sorted(lines_valid):
        lc, lv = lines_covered[filename], lines_valid[filename]
        bc, bv = branches_covered[filename], branches_valid[filename]
        total_lc += lc
        total_lv += lv
        total_bc += bc
        total_bv += bv
        print(
            f"{rate(lc, lv):6.1f} {rate(bc, bv):7.1f}  {lc:3d}/{lv:<3d}  {bc:3d}/{bv:<4d}  {filename}"
        )

    print(
        f"\nTOTAL  line {rate(total_lc, total_lv):.1f}%  "
        f"branch {rate(total_bc, total_bv):.1f}%  "
        f"({total_lc}/{total_lv} lines, {total_bc}/{total_bv} branches)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
