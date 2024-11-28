aws ec2 run-instances \
    --image-id ami-0e4324800e861a32a \
    --instance-type t2.xlarge \
    --key-name slerner-us-west-2 \
    --security-group-ids sg-032d218698f31d86f \
    --subnet-id subnet-00774971f1ec66408 \
    --iam-instance-profile Name=ecsInstanceRole \
    --user-data file://user-data.txt