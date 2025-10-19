# IndustrialInference.PersistentHeap - Build and Test Automation
# ==========================================================

# Default recipe - show available commands
default:
    @just --list

# Clean all build artifacts and packages
clean:
    @echo "🧹 Cleaning build artifacts..."
    dotnet clean
    @echo "✅ Clean completed"

# Restore NuGet packages
restore:
    @echo "📦 Restoring NuGet packages..."
    dotnet restore
    @echo "✅ Restore completed"

# Build the entire solution
build: clean restore
    @echo "🔨 Building solution..."
    dotnet build --no-restore
    @echo "✅ Build completed"

# Build in release mode
build-release: clean restore
    @echo "🔨 Building solution (Release)..."
    dotnet build --no-restore --configuration Release
    @echo "✅ Release build completed"

# Run all tests
test: build
    @echo "🧪 Running all tests..."
    dotnet test --no-build --verbosity normal
    @echo "✅ Tests completed"

# Run tests with coverage
test-coverage: build
    @echo "🧪 Running tests with coverage..."
    dotnet test --no-build --collect:"XPlat Code Coverage" --verbosity normal
    @echo "✅ Test coverage completed"

# Run only working tests (skip problematic ones)
test-working: build
    @echo "🧪 Running working tests only..."
    dotnet test --no-build --verbosity normal --filter "Category!=Failing"
    @echo "✅ Working tests completed"

# Run tests excluding property-based tests that might hit bugs
test-safe: build
    @echo "🧪 Running safe tests (no property-based tests)..."
    dotnet test test/Tests/Tests.csproj --no-build --verbosity normal --filter "Category!=Property"
    @echo "✅ Safe tests completed"

# Run only specific test class
test-class CLASS: build
    @echo "🧪 Running tests for class {{CLASS}}..."
    dotnet test --no-build --verbosity normal --filter "FullyQualifiedName~{{CLASS}}"
    @echo "✅ Class tests completed"

# Run only the PersistentHeap.Tests project  
test-persistent: build
    @echo "🧪 Running PersistentHeap tests..."
    dotnet test test/PersistentHeap.Tests/PersistentHeap.Tests.csproj --no-build --verbosity normal
    @echo "✅ PersistentHeap tests completed"

# Run benchmarks
benchmark: build-release
    @echo "⚡ Running benchmarks..."
    dotnet run --project test/Benchmarks/Benchmarks.csproj --configuration Release
    @echo "✅ Benchmarks completed"

# Watch and rebuild on file changes
watch:
    @echo "👀 Watching for file changes..."
    dotnet watch --project IndustrialInference.PersistentHeap/IndustrialInference.BPlusTree.csproj build

# Watch and run tests on file changes
watch-test:
    @echo "👀 Watching for file changes and running tests..."
    dotnet watch --project test/Tests/Tests.csproj test

# Format code
format:
    @echo "✨ Formatting code..."
    dotnet format
    @echo "✅ Code formatting completed"

# Check for security vulnerabilities
audit:
    @echo "🔒 Checking for security vulnerabilities..."
    dotnet list package --vulnerable --include-transitive
    @echo "✅ Security audit completed"

# Update all packages to latest versions
update-packages:
    @echo "📦 Updating packages..."
    dotnet list package --outdated
    @echo "ℹ️  To update packages, run: dotnet add package <PackageName>"

# List all packages and their versions
list-packages:
    @echo "📦 Listing all packages..."
    dotnet list package --include-transitive

# Quick check - build and test fast
check: 
    @echo "⚡ Quick check - build and test..."
    dotnet build --verbosity quiet
    dotnet test --verbosity quiet --no-build
    @echo "✅ Quick check completed"

# Full CI pipeline - what runs in continuous integration
ci: clean restore build test
    @echo "🚀 CI pipeline completed successfully"

# Development setup - restore and build for first time setup
setup: restore build
    @echo "🛠️  Development environment setup completed"

# Show project information
info:
    @echo "📊 Project Information:"
    @echo "======================"
    @echo "Solution: IndustrialInference.PersistentHeap.sln"
    @echo "Target Framework: .NET 9.0"
    @echo ""
    @echo "Projects:"
    @echo "  • IndustrialInference.BPlusTree (main library)"
    @echo "  • Tests (xUnit tests)"
    @echo "  • PersistentHeap.Tests (NUnit tests)"
    @echo "  • Benchmarks (BenchmarkDotNet)"