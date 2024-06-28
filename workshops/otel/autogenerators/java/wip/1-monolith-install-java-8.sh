# follow steps below to install open source jdk8
sudo apt update
sudo apt install -y wget
sudo rm -rf Open*
wget https://github.com/adoptium/temurin8-binaries/releases/download/jdk8u412-b08/OpenJDK8U-jdk_x64_linux_hotspot_8u412b08.tar.gz
sudo mkdir -p /usr/lib/jvm
sudo tar -xzf Open* -C /usr/lib/jvm
sudo update-alternatives --install /usr/bin/java java /usr/lib/jvm/jdk8u412-b08/bin/java 1
sudo update-alternatives --install /usr/bin/javac javac /usr/lib/jvm/jdk8u412-b08/bin/javac 1
sudo update-alternatives --config java
sudo update-alternatives --config javac
#add to .bashrc
export JAVA_HOME=/usr/lib/jvm/jdk8u412-b08
export PATH=$JAVA_HOME/bin:$PATH