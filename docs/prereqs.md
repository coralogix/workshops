# OpenTelemetry Hand-On Workshop Prerequisites

These workshops are designed for advanced users such as devops, developers, and sysadmins to learn about OpenTelemetry and try it out with the Coralogix platform. They include easy-to-deploy examples and are meant to be conducted on an Ubuntu cloud environment, not on personal computers (unless otherwise specified).

Requirements for these workshops include:

- Proficiency with an Integrated Development Environment (IDE) like Visual Studio Code; simple text editors like Notepad are not adequate, though VI can be used if set up as an IDE.
- The computer should be set up as an integrated development environment (IDE).
- An active Coralogix account and a basic understanding of its features.
- Permissions to create resources like virtual machines and Kubernetes clusters in the cloud without restrictions.
- Ability to set up virtual machines and Kubernetes clusters and all surrounding cloud infra necessary quickly before the workshop begins.
- Direct internet access on the workshop computer, without needing a VPN.
- Unrestricted access on computer and in cloud to GitHub, Helm, and the ability to use open source software.
- Users should understand the programming languages being used (for example Python) if asking questions about OpenTelemetry for Python.
  
- A unique [Coralogix API Key](https://coralogix.com/docs/send-your-data-api-key/) should be ready for the workshop that will be discarded after.  
  
### Workshop Platform

The bulk of the workshop is designed for Kubernetes. If you have an existing Kubernetes cluster that can be used as a sandbox, you only need to make sure you have the following installed:
- [Helm](https://helm.sh/docs/intro/install/)
- [k9s](https://k9scli.io/)

#### If you do not have a Kubernetes sandbox, follow the instructions below:

- The following EC2 VM should be created, ready, and accessible before the workshop. It should be set up and already tested with access via your IDE with the software list below installed:
  
```aws ec2 run-instances \
--image-id ami-0e83be366243f524a \
--count 1 \
--instance-type t2.xlarge \
--key-name YOURKEYHERE \
--security-group-ids sg-0b255a03981b65b40 \
--subnet-id subnet-0517e5c4de64acc2b \
--associate-public-ip-address \
--block-device-mappings 'DeviceName=/dev/sda1,Ebs={VolumeSize=50}' \
--tag-specifications \
'ResourceType=instance, Tags=[{Key=user,Value=steve.lerner},{Key=Name,Value=slerner-2xl-32GB}]' 'ResourceType=volume, Tags=[{Key=Name,Value=YourVolumeName}]'`
```  

If that AMI is not available pick any Debian server AMI.  

Install the following in advance of the workshop:   
- [k3s](https://k3s.io/) 
- [Helm](https://helm.sh/docs/intro/install/)
- [k9s](https://k9scli.io/)