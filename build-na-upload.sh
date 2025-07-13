cd Recycler.API
rm -rf publish
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish
cd ..
ssh -i "ec2-key-pair.pem" ec2-user@ec2-13-245-149-83.af-south-1.compute.amazonaws.com "sudo pkill -f Recycler.API"
# ssh -i "ec2-key-pair.pem" ec2-user@ec2-13-245-149-83.af-south-1.compute.amazonaws.com "sudo rm -rf publish"

scp -r -i "ec2-key-pair.pem" Recycler.API/publish ec2-user@ec2-13-245-149-83.af-south-1.compute.amazonaws.com:~/
scp -r -i "ec2-key-pair.pem" Recycler.API/certs ec2-user@ec2-13-245-149-83.af-south-1.compute.amazonaws.com:~/publish