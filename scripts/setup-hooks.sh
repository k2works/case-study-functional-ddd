#!/bin/sh
#
# Setup Git hooks for the OrderTaking project
# Run this script after cloning the repository
#

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
HOOKS_DIR="$PROJECT_ROOT/.git/hooks"

echo "🔧 Setting up Git hooks..."

# Check if .git directory exists
if [ ! -d "$PROJECT_ROOT/.git" ]; then
    echo "❌ Error: .git directory not found"
    echo "Please run this script from the project root or ensure the repository is initialized"
    exit 1
fi

# Create symlink for pre-commit hook
if [ -f "$HOOKS_DIR/pre-commit" ]; then
    echo "⚠️  pre-commit hook already exists"
    read -p "Do you want to replace it? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Skipping pre-commit hook setup"
        exit 0
    fi
    rm "$HOOKS_DIR/pre-commit"
fi

# Copy pre-commit hook
cp "$SCRIPT_DIR/hooks/pre-commit" "$HOOKS_DIR/pre-commit"
chmod +x "$HOOKS_DIR/pre-commit"

echo "✅ Git hooks installed successfully!"
echo ""
echo "The pre-commit hook will run the following checks:"
echo "  1. Format check (Fantomas)"
echo "  2. Build (0 warnings)"
echo "  3. Tests (all passing)"
echo ""
echo "To skip hooks for a specific commit, use: git commit --no-verify"
