import re
from pathlib import Path

# Description:
# 1. For every C# file, find occurrences where a member has both [Offset(...)] and [OffsetDawntrail(...)]
#    directly stacked (optionally with blank/comment lines between).
#    Remove the first [Offset(...)] and rename [OffsetDawntrail(...)] to [Offset(...)].
# 2. For any lone [OffsetDawntrail(...)] (not commented) replace with [Offset(...)].
# 3. Leave commented out lines (//[OffsetDawntrail(...)] or //[Offset(...)] ) untouched.
# 4. Preserve original spacing / newlines as much as possible.
# 5. Print a summary of replacements.

ATTR_DUAL_REGEX = re.compile(
    r"(^[ \t]*\[Offset\([^\n\]]*\)\][ \t]*(?:\r?\n(?:[ \t]*//.*\r?\n)*)?)"  # First attribute block (global)
    r"(^[ \t]*\[OffsetDawntrail\([^\n\]]*\)\])",                                 # Dawntrail attribute
    re.MULTILINE
)

# Lone OffsetDawntrail (ensure line start and not commented)
ATTR_SINGLE_REGEX = re.compile(r"(^[ \t]*)\[OffsetDawntrail(\([^\n\]]*\)\])", re.MULTILINE)


def process_text(text: str, file_path: Path):
    dual_replacements = 0
    single_replacements = 0

    # Step 1: Dual attributes
    def dual_repl(match: re.Match):
        nonlocal dual_replacements
        dual_replacements += 1
        dawntrail_attr = match.group(2)
        # Convert second attribute to Offset
        new_attr = dawntrail_attr.replace('[OffsetDawntrail', '[Offset')
        return new_attr  # drop the first attribute entirely

    text, dual_count = ATTR_DUAL_REGEX.subn(dual_repl, text)

    # Step 2: Lone OffsetDawntrail
    def single_repl(match: re.Match):
        nonlocal single_replacements
        single_replacements += 1
        indent = match.group(1)
        body = match.group(2)
        return f"{indent}[Offset{body}"

    text, single_count = ATTR_SINGLE_REGEX.subn(single_repl, text)

    return text, dual_replacements, single_replacements


def main():
    root = Path(__file__).resolve().parent.parent  # project root (assumes script in Scripts/)
    cs_files = list(root.rglob('*.cs'))

    total_dual = 0
    total_single = 0
    touched = 0

    for cs in cs_files:
        original = cs.read_text(encoding='utf-8')
        updated, dual, single = process_text(original, cs)
        if dual or single:
            cs.write_text(updated, encoding='utf-8')
            touched += 1
            total_dual += dual
            total_single += single
            print(f"Updated {cs}: dual={dual}, single={single}")

    print('\nSummary:')
    print(f" Files modified: {touched}")
    print(f" Dual attribute collapses (removed original [Offset] + renamed [OffsetDawntrail]): {total_dual}")
    print(f" Lone [OffsetDawntrail] renamed: {total_single}")

if __name__ == '__main__':
    main()
