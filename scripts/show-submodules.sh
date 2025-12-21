#!/bin/bash
# Script to display Git submodules in a readable format
# Usage: ./scripts/show-submodules.sh [submodule-path]

set -euo pipefail

SUBMODULE_PATH="${1:-.}"

cd "$SUBMODULE_PATH"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Get all submodules
SUBMODULES=$(git config --file .gitmodules --get-regexp path | sed 's/^submodule\..*\.path //' || true)

if [ -z "$SUBMODULES" ]; then
    echo -e "${YELLOW}No submodules found.${NC}"
    exit 0
fi

echo ""
echo -e "${CYAN}Submodule Status${NC}"
printf "${CYAN}%100s${NC}\n" | tr ' ' '='
echo ""

ALL_INFO=""

while IFS= read -r submodule; do
    if [ -z "$submodule" ]; then
        continue
    fi
    
    # Get the commit hash that the parent repo is pointing to
    SUBMODULE_COMMIT=$(git ls-tree HEAD -- "$submodule" | awk '{print $3}' || echo "")
    
    if [ -z "$SUBMODULE_COMMIT" ]; then
        continue
    fi
    
    cd "$submodule"
    
    # Get commit info
    COMMIT_INFO=$(git log -1 --pretty=format:"%h|%s|%cr|%an" "$SUBMODULE_COMMIT" 2>/dev/null || echo "")
    if [ -z "$COMMIT_INFO" ]; then
        SHORT_HASH="${SUBMODULE_COMMIT:0:7}"
        SUBJECT="(commit not found in submodule)"
        RELATIVE_DATE="unknown"
        AUTHOR="unknown"
    else
        SHORT_HASH=$(echo "$COMMIT_INFO" | cut -d'|' -f1)
        SUBJECT=$(echo "$COMMIT_INFO" | cut -d'|' -f2)
        RELATIVE_DATE=$(echo "$COMMIT_INFO" | cut -d'|' -f3)
        AUTHOR=$(echo "$COMMIT_INFO" | cut -d'|' -f4)
    fi
    
    # Get current branch or detached HEAD info
    CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo "detached")
    if [ "$CURRENT_BRANCH" = "HEAD" ]; then
        CURRENT_BRANCH="detached"
    fi
    
    # Check if submodule is modified
    STATUS=$(git status --porcelain 2>/dev/null || echo "")
    IS_MODIFIED=false
    if [ -n "$STATUS" ]; then
        IS_MODIFIED=true
    fi
    
    # Check if submodule commit matches current HEAD
    CURRENT_HEAD=$(git rev-parse HEAD 2>/dev/null || echo "")
    IS_UP_TO_DATE=false
    if [ "$SUBMODULE_COMMIT" = "$CURRENT_HEAD" ]; then
        IS_UP_TO_DATE=true
    fi
    
    # Check if submodule is behind/ahead of remote
    REMOTE_BRANCH=$(git rev-parse --abbrev-ref --symbolic-full-name @{u} 2>/dev/null || echo "")
    BEHIND=0
    AHEAD=0
    if [ -n "$REMOTE_BRANCH" ]; then
        git fetch --quiet 2>/dev/null || true
        BEHIND=$(git rev-list --count HEAD..@{u} 2>/dev/null || echo "0")
        AHEAD=$(git rev-list --count @{u}..HEAD 2>/dev/null || echo "0")
    fi
    
    # Display formatted output
    echo -e "${GREEN}📦 $submodule${NC}"
    echo -n "   Commit:    "
    echo -ne "${YELLOW}$SHORT_HASH${NC}"
    echo " (${SUBMODULE_COMMIT:0:12})"
    echo "   Message:   $SUBJECT"
    echo "   Date:      $RELATIVE_DATE by $AUTHOR"
    echo -n "   Branch:    "
    if [ "$CURRENT_BRANCH" != "detached" ]; then
        echo -e "${CYAN}$CURRENT_BRANCH${NC}"
    else
        echo -e "${YELLOW}detached HEAD${NC}"
    fi
    
    # Status indicators
    STATUS_PARTS=()
    if [ "$IS_MODIFIED" = "true" ]; then
        STATUS_PARTS+=("Modified")
    fi
    if [ "$IS_UP_TO_DATE" = "false" ]; then
        STATUS_PARTS+=("Not at referenced commit")
    fi
    if [ "$BEHIND" -gt 0 ]; then
        STATUS_PARTS+=("$BEHIND behind remote")
    fi
    if [ "$AHEAD" -gt 0 ]; then
        STATUS_PARTS+=("$AHEAD ahead of remote")
    fi
    if [ ${#STATUS_PARTS[@]} -eq 0 ]; then
        STATUS_PARTS+=("✓ Up to date")
    fi
    
    echo -n "   Status:    "
    if [ ${#STATUS_PARTS[@]} -eq 1 ] && [ "${STATUS_PARTS[0]}" = "✓ Up to date" ]; then
        echo -e "${GREEN}${STATUS_PARTS[0]}${NC}"
    else
        echo -e "${YELLOW}$(IFS=', '; echo "${STATUS_PARTS[*]}")${NC}"
    fi
    
    echo ""
    
    cd - > /dev/null
    
    # Track summary
    if [ "$IS_MODIFIED" = "true" ]; then
        MODIFIED_COUNT=$((MODIFIED_COUNT + 1))
    fi
    if [ "$IS_UP_TO_DATE" = "false" ]; then
        OUT_OF_DATE_COUNT=$((OUT_OF_DATE_COUNT + 1))
    fi
    if [ "$BEHIND" -gt 0 ]; then
        BEHIND_COUNT=$((BEHIND_COUNT + 1))
    fi
done <<< "$SUBMODULES"

MODIFIED_COUNT=${MODIFIED_COUNT:-0}
OUT_OF_DATE_COUNT=${OUT_OF_DATE_COUNT:-0}
BEHIND_COUNT=${BEHIND_COUNT:-0}

printf "${CYAN}%100s${NC}\n" | tr ' ' '='
echo ""

# Summary
if [ "$MODIFIED_COUNT" -eq 0 ] && [ "$OUT_OF_DATE_COUNT" -eq 0 ] && [ "$BEHIND_COUNT" -eq 0 ]; then
    echo -e "${GREEN}All submodules are up to date ✓${NC}"
else
    echo -e "${YELLOW}Summary:${NC}"
    if [ "$MODIFIED_COUNT" -gt 0 ]; then
        echo -e "  ${YELLOW}- $MODIFIED_COUNT submodule(s) have uncommitted changes${NC}"
    fi
    if [ "$OUT_OF_DATE_COUNT" -gt 0 ]; then
        echo -e "  ${YELLOW}- $OUT_OF_DATE_COUNT submodule(s) not at referenced commit${NC}"
    fi
    if [ "$BEHIND_COUNT" -gt 0 ]; then
        echo -e "  ${YELLOW}- $BEHIND_COUNT submodule(s) behind remote${NC}"
    fi
fi
echo ""
