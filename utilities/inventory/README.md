## AWS Inventory Logger Container

Inventory container executes a Python app using AWS BOTO3 that runs a loop of describing a set of inventory on AWS accounts. 
It requires the following env variables- and can handle as many accounts as you'd like.  

```
export AWS_CONFIGURATIONS="YOUR_ACCESS_KEY_1:YOUR_SECRET_KEY_1:us-east-1,YOUR_ACCESS_KEY_2:YOUR_SECRET_KEY_2:eu-west-1"  
export INVENTORY_INTERVAL=30
``````

or translated properly to YAML for a k8s deployment- you can see an example here [./yaml](./yaml)

It will describe as many AWS accounts as one adds.  
It will not retrieve enviroment variables to prevent exposure of tokens etc...

The following services are included in the inventory- more can be added upon request:
- `EC2`
- `S3`
- `RDS`
- `Lambda`
- `VPCs`
- `ECR`
- `ECS`
- `EKS`

The inventory will be returned in a log body like this example:

```
{
    "resource": {
        "attributes": {
            "cx.application.name": "default",
            "cx.subsystem.name": "inventory",
            "k8s.container.name": "inventory",
            "k8s.container.restart_count": "0",
            "k8s.deployment.name": "inventory",
            "k8s.namespace.name": "default",
            "k8s.node.name": "ip-172-32-9-176",
            "k8s.pod.name": "inventory-5f6d8b4cf4-b2pnp",
            "k8s.pod.uid": "613c2cc5-2fd1-4d02-a44e-7523e705cb02"
        },
        "droppedAttributesCount": 0
    },
    "scope": {
        "name": "",
        "version": ""
    },
    "log_level": "INFO",
    "logRecord": {
        "timeUnixNano": 1690133531823891700,
        "observedTimeUnixNano": 1690133531894555000,
        "severityNumber": 0,
        "attributes": {
            "log.file.path": "/var/log/pods/default_inventory-5f6d8b4cf4-b2pnp_613c2cc5-2fd1-4d02-a44e-7523e705cb02/inventory/0.log",
            "log.iostream": "stderr",
            "logtag": "F",
            "time": "2023-07-23T17:32:11.823891719Z"
        }
    },
    "body": {
        "account_number": "104013952213",
        "resource_type": "ec2_instances",
        "resource": {
            "Groups": [],
            "Instances": [
                {
                    "AmiLaunchIndex": 1,
                    "ImageId": "ami-024e6efaf93d85776",
                    "InstanceId": "i-0dfe80e3b1ac64835",
                    "InstanceType": "t2.micro",
                    "KeyName": "stevelernercxuseast2",
                    "LaunchTime": "2023-07-23T17:03:19+00:00",
                    "Monitoring": {
                        "State": "disabled"
                    },
                    "Placement": {
                        "AvailabilityZone": "us-east-2a",
                        "GroupName": "",
                        "Tenancy": "default"
                    },
                    "PrivateDnsName": "ip-172-31-15-147.us-east-2.compute.internal",
                    "PrivateIpAddress": "172.31.15.147",
                    "ProductCodes": [],
                    "PublicDnsName": "ec2-3-135-195-1.us-east-2.compute.amazonaws.com",
                    "PublicIpAddress": "3.135.195.1",
                    "State": {
                        "Code": 16,
                        "Name": "running"
                    },
                    "StateTransitionReason": "",
                    "SubnetId": "subnet-0dbe4982f2d58d2e5",
                    "VpcId": "vpc-0a5807b43725900c1",
                    "Architecture": "x86_64",
                    "BlockDeviceMappings": [
                        {
                            "DeviceName": "/dev/sda1",
                            "Ebs": {
                                "AttachTime": "2023-07-23T17:03:20+00:00",
                                "DeleteOnTermination": true,
                                "Status": "attached",
                                "VolumeId": "vol-0a0cae282661d494f"
                            }
                        }
                    ],
                    "ClientToken": "009118c0-ad48-4a69-ad45-6c8f7ee056b7",
                    "EbsOptimized": false,
                    "EnaSupport": true,
                    "Hypervisor": "xen",
                    "NetworkInterfaces": [
                        {
                            "Association": {
                                "IpOwnerId": "amazon",
                                "PublicDnsName": "ec2-3-135-195-1.us-east-2.compute.amazonaws.com",
                                "PublicIp": "3.135.195.1"
                            },
                            "Attachment": {
                                "AttachTime": "2023-07-23T17:03:19+00:00",
                                "AttachmentId": "eni-attach-0a8f28ef1eec52e78",
                                "DeleteOnTermination": true,
                                "DeviceIndex": 0,
                                "Status": "attached",
                                "NetworkCardIndex": 0
                            },
                            "Description": "",
                            "Groups": [
                                {
                                    "GroupName": "launch-wizard-3",
                                    "GroupId": "sg-06506c2c0fc69121f"
                                }
                            ],
                            "Ipv6Addresses": [],
                            "MacAddress": "02:46:ab:55:4d:65",
                            "NetworkInterfaceId": "eni-0c90a054b07e01d1e",
                            "OwnerId": "104013952213",
                            "PrivateDnsName": "ip-172-31-15-147.us-east-2.compute.internal",
                            "PrivateIpAddress": "172.31.15.147",
                            "PrivateIpAddresses": [
                                {
                                    "Association": {
                                        "IpOwnerId": "amazon",
                                        "PublicDnsName": "ec2-3-135-195-1.us-east-2.compute.amazonaws.com",
                                        "PublicIp": "3.135.195.1"
                                    },
                                    "Primary": true,
                                    "PrivateDnsName": "ip-172-31-15-147.us-east-2.compute.internal",
                                    "PrivateIpAddress": "172.31.15.147"
                                }
                            ],
                            "SourceDestCheck": true,
                            "Status": "in-use",
                            "SubnetId": "subnet-0dbe4982f2d58d2e5",
                            "VpcId": "vpc-0a5807b43725900c1",
                            "InterfaceType": "interface"
                        }
                    ],
                    "RootDeviceName": "/dev/sda1",
                    "RootDeviceType": "ebs",
                    "SecurityGroups": [
                        {
                            "GroupName": "launch-wizard-3",
                            "GroupId": "sg-06506c2c0fc69121f"
                        }
                    ],
                    "SourceDestCheck": true,
                    "Tags": [
                        {
                            "Key": "user",
                            "Value": "steve.lerner"
                        },
                        {
                            "Key": "Name",
                            "Value": "steve-lerner-tempinstances"
                        }
                    ],
                    "VirtualizationType": "hvm",
                    "CpuOptions": {
                        "CoreCount": 1,
                        "ThreadsPerCore": 1
                    },
                    "CapacityReservationSpecification": {
                        "CapacityReservationPreference": "open"
                    },
                    "HibernationOptions": {
                        "Configured": false
                    },
                    "MetadataOptions": {
                        "State": "applied",
                        "HttpTokens": "optional",
                        "HttpPutResponseHopLimit": 1,
                        "HttpEndpoint": "enabled",
                        "HttpProtocolIpv6": "disabled",
                        "InstanceMetadataTags": "disabled"
                    },
                    "EnclaveOptions": {
                        "Enabled": false
                    },
                    "PlatformDetails": "Linux/UNIX",
                    "UsageOperation": "RunInstances",
                    "UsageOperationUpdateTime": "2023-07-23T17:03:19+00:00",
                    "PrivateDnsNameOptions": {
                        "HostnameType": "ip-name",
                        "EnableResourceNameDnsARecord": true,
                        "EnableResourceNameDnsAAAARecord": false
                    },
                    "MaintenanceOptions": {
                        "AutoRecovery": "default"
                    },
                    "CurrentInstanceBootMode": "legacy-bios"
                }
            ],
            "OwnerId": "104013952213",
            "ReservationId": "r-045b7e4bfba49e852"
        }
    },
    "timestamp": "2023-07-23T17:32:11.823599"
}
```