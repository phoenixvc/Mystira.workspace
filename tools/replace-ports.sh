#!/bin/bash
# Replace App ports namespace with workspace namespace
cd "$(dirname "$0")/.."

count=0
while IFS= read -r file; do
    sed -i 's/using Mystira\.App\.Application\.Ports\.Data;/using Mystira.Application.Ports.Data;/g' "$file"
    count=$((count + 1))
done < <(grep -rl "using Mystira\.App\.Application\.Ports\.Data;" apps/app --include="*.cs")

echo "Replaced namespace in $count files"

# Also replace Mystira.Shared.Data.Repositories with Mystira.Application.Ports.Data
count2=0
while IFS= read -r file; do
    sed -i 's/using Mystira\.Shared\.Data\.Repositories;/using Mystira.Application.Ports.Data;/g' "$file"
    count2=$((count2 + 1))
done < <(grep -rl "using Mystira\.Shared\.Data\.Repositories;" apps/app --include="*.cs")

echo "Replaced Shared.Data.Repositories in $count2 files"
