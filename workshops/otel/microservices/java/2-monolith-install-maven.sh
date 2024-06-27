rm -rf apache*
wget https://dlcdn.apache.org/maven/maven-3/3.9.8/binaries/apache-maven-3.9.8-bin.tar.gz
gunzip apache*
sudo tar -xvf apache* -C /opt
sudo ln -s /opt/apache-maven-3.9.8 /opt/maven
# sudo nano /etc/profile.d/maven.sh
# export M2_HOME=/opt/maven
# export MAVEN_HOME=/opt/maven
# export PATH=$M2_HOME/bin:$PATH
# sudo chmod +x /etc/profile.d/maven.sh
# source /etc/profile.d/maven.sh