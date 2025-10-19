yum update -y
yum install git -y
dnf install dotnet-sdk-9.0 -y

REPO_URL="https://github.com/miniconomy2025/recycler.git"
TARGET_DIR="/home/ec2-user/recycler"

# Clone only if the directory does not exist
if [ ! -d "$TARGET_DIR" ]; then
    echo "Directory $TARGET_DIR does not exist. Cloning repository..."
    git clone "$REPO_URL" "$TARGET_DIR"
else
    echo "Directory $TARGET_DIR already exists. Pulling latest changes..."
    cd "$TARGET_DIR" || exit 1
    git fetch origin
    git reset --hard origin/main   # or replace 'main' with your default branch
fi

pkill -f Recycler.API
cd "$TARGET_DIR" || exit 1
cd Recycler.API
nohup ASPNETCORE_ENVIRONMENT=Production dotnet run > app.log 2>&1 &
