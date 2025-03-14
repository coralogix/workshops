# OpenTelemetry Hand-On Workshop Prerequisites

These workshops are designed for advanced users such as devops, developers, and sysadmins to learn about OpenTelemetry / Coralogix RUM and try them with the Coralogix platform. They include easy-to-deploy examples and are meant to be conducted on an Ubuntu or Kubernetes production grade cloud environment, not on personal computers (unless otherwise specified).  
  
Requirements for these workshops include:  

- Proficiency with an Integrated Development Environment (IDE) like Visual Studio Code; simple text editors like Notepad are not adequate, though VI can be used if set up as an IDE.  
- The computer should be set up as an integrated development environment (IDE).  
- An active Coralogix account and a basic understanding of its features.  
- Permissions to create resources like virtual machines and Kubernetes clusters in the cloud without restrictions.  
- Ability to set up virtual machines and Kubernetes clusters and all surrounding cloud infra necessary quickly before the workshop begins.  
- Direct internet access on the workshop computer, without needing a VPN.  
- Unrestricted access on computer and in cloud to GitHub, Helm, and the ability to use open source software.  
- Users should understand the programming languages being used (for example Python) if asking questions about OpenTelemetry for Python.  
- All k8s examples should be tested in cloud or production grade k8s... local personal computers are not supported for testing due to the derived nature of k8s on local machines .  
  
- A unique [Coralogix API Key](https://coralogix.com/docs/send-your-data-api-key/) should be ready for the workshop that will be discarded after.  
  
### Workshop Platform

The bulk of the workshop is designed for Kubernetes. If you have an existing Kubernetes cluster that can be used as a sandbox, you only need to make sure you have the following installed:  
- [Helm](https://helm.sh/docs/intro/install/)  
- [k9s](https://k9scli.io/)