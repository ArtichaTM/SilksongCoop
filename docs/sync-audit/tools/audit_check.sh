#!/bin/bash
# usage: audit_check.sh <table.md> <Area> [Area...]; paths are relative to docs/sync-audit/
cd "$(dirname "$0")/.."
out=$1; shift
cats=(); for a in "$@"; do cats+=(catalog/$a.md); done
echo "-- headings missing/extra/order:"
diff <(grep -h '^## ' "${cats[@]}") <(grep '^## ' "$out" | grep -v '^## Summary') && echo ok
echo "-- rows with !=9 cols:"
grep '^|' "$out" | awk -F'|' 'NF!=11' | head
echo "-- catalog refs not in audit:"
miss=0
while read -r line; do
  ref=$(grep -o 'scenes/[^ ]*:[0-9]*' <<<"$line" | head -1); [ -z "$ref" ] && continue
  if ! grep -qF "$ref" "$out"; then
    id=$(grep -o 'id=[0-9a-f]*' <<<"$line" | head -1 | cut -d= -f2)
    typ=$(grep -o 'C# [A-Za-z0-9_]*' <<<"$line" | cut -c4-)
    if { [ -n "$id" ] && grep -qF "$id" "$out"; } || { [ -n "$typ" ] && grep -qw "$typ" "$out"; }; then :; else echo "  $line" | cut -c1-160; miss=$((miss+1)); fi
  fi
done < <(grep -h -e 'FSM \[' -e 'C# ' "${cats[@]}")
echo "  missing: $miss"
echo "-- mod terms / hr:"
grep -n -i -E 'puppet|EnemySync|PickupSync|Authority|^---' "$out" | head
echo "-- Does >8 words:"
grep '^|' "$out" | awk -F'|' 'NR>2 && split($5,w," ")>8 {print $2, "|", $5}' | grep -v -- '---' | head
