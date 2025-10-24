echo "Running Recycler Integration Tests"
echo "====================================="
if ! docker info > /dev/null 2>&1; then
    echo "Docker is not running. Please start Docker and try again."
    exit 1
fi

echo "Docker is running"

cd Recycler.Tests

echo "Building test project..."
dotnet build

if [ $? -ne 0 ]; then
    echo "Build failed"
    exit 1
fi

echo "Build successful"
echo "Running integration tests..."
dotnet test --verbosity normal

if [ $? -eq 0 ]; then
    echo "All tests passed!"
else
    echo "Some tests failed"
    exit 1
fi
