yum update -y
yum install git -y
yum install dotnet-sdk-9.0 -y
yum install postgresql17-server.x86_64 -y
yum install nginx -y
yum install certbot -y
yum install python3-certbot-nginx -y
# /usr/bin/postgresql-setup --initdb
# systemctl enable postgresql
# systemctl start postgresql

# sudo -i -u postgres
# psql
# CREATE DATABASE recycler;
# CREATE USER myuser WITH ENCRYPTED PASSWORD 'somepassword';
# GRANT ALL PRIVILEGES ON DATABASE recycler TO myuser;
# ALTER ROLE myuser SUPERUSER CREATEDB CREATEROLE INHERIT LOGIN REPLICATION BYPASSRLS;

# echo "listen_addresses = '*'" >> /var/lib/pgsql/data/postgresql.conf
# echo "host    all    all    0.0.0.0/0    md5" >> /var/lib/pgsql/data/pg_hba.conf

systemctl restart postgresql
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
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Production
rm -rf app.log
dotnet test
nohup dotnet run > app.log 2>&1 &