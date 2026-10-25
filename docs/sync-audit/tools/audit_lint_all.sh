#!/bin/bash
# Lint every audit table against its catalog slice. 03+04 share catalog/Bone.md, so they are checked as one file.
D=$(cd "$(dirname "$0")" && pwd); SELF="$D/$(basename "$0")"
cd "$D/.."
S="$D/audit_check.sh"
tmp=$(mktemp); cat audit/03_Bone_a.md audit/04_Bone_b.md | grep -v '^## Summary' > "$tmp"
run() { echo "######## $1"; "$S" "$2" "${@:3}" | awk '/^-- Does/{d=1;n=0;next} d{n++;next} {print} END{print "-- Does >8 words: " n}' | grep -v '^  missing: 0$'; }
run 01 audit/01_Tut.md Tut
run 02 audit/02_Bonetown_Mosstown_Bonegrave.md Bonetown Mosstown Bonegrave
run 03+04 "$tmp" Bone
run 05 audit/05_Bellway_Wisp_Crawl.md Bellway Wisp Crawl
run 06 audit/06_Dock.md Dock
run 07 audit/07_Bone_East.md Bone_East
run 08 audit/08_Greymoor_Ant.md Greymoor Ant
run 09 audit/09_Shellwood_Room.md Shellwood Room
run 10 audit/10_Belltown_Peak.md Belltown Peak
run 11 audit/11_Coral_Weave.md Coral Weave
run 12 audit/12_Dust_Shadow.md Dust Shadow
run 13 audit/13_Song.md Song
run 14 audit/14_Cog_Ward_Arborium.md Cog Ward Arborium
run 15 audit/15_Under_Aqueduct.md Under Aqueduct
run 16 audit/16_Slab_Library.md Slab Library
run 17 audit/17_Hang_Cradle_Clover.md Hang Cradle Clover
run 18 audit/18_Optional.md Optional
run 19 audit/19_Abyss_Cinematic_Meta.md Abyss Cinematic Meta
rm -f "$tmp"
echo "######## areas without an audit"
for c in catalog/*.md; do a=$(basename "$c" .md); grep -qw "$a" "$SELF" || echo "  $a"; done
