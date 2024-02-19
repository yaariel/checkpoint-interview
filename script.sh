#!/bin/bash

set -e

EC2_INSTANCE="ec2-user@13.53.92.72"
API_ENDPOINT="http://13.53.92.72/wordcount/countinfile"
FILE_PATH="test.txt"
KEY_PAIR="test_key_pair.pem" #bad security, but I couldn't figure out a better way

dotnet publish -c Release -r linux-x64 --self-contained true -o ./published || {echo "failed to publish the API"; exit 1;}

scp -i KEY_PAIR -r ./published/* $EC2_INSTANCE:/home/ec2-user/test || {echo "failed to copy the files to the AWS server"; exit 1;}

ssh -i KEY_PAIR $EC2_INSTANCE "sudo systemctl restart checkpoint.service" || {echo "failed to restart the checkpoint.service service on the AWS server"; exit 1;}
#the checkpoint.service service sets up the server using the files in /home/ec2-user/test

curl -X POST -F "file=@$FILE_PATH" $API_ENDPOINT || {echo "failed to test the API to check if it was deployed successfully"; exit 1;}

echo "API deployed and tested successfully"
