
# Setup Guide

## Install Xcode and Android Studio

## Install Java Temurin 17 LTS

1. **Update Homebrew:**
   ```sh
   brew update
   ```

2. **Install Temurin 17:**
   ```sh
   brew install --cask temurin17
   ```

3. **Add JAVA_HOME to the bash configuration file:**
   ```sh
   echo 'export JAVA_HOME=$(/usr/libexec/java_home -v 17)' >> ~/.bashrc
   ```

4. **Reload the bash configuration:**
   ```sh
   source ~/.bashrc
   ```

## Install Ruby

1. **Update Homebrew:**
   ```sh
   brew update
   ```

2. **Install `rbenv` and `ruby-build`:**
   ```sh
   brew install rbenv ruby-build
   ```

3. **Add `rbenv` to bash configuration and initialize it:**
   ```sh
   echo 'if which rbenv > /dev/null; then eval "$(rbenv init -)"; fi' >> ~/.bashrc
   source ~/.bashrc
   ```

4. **Install Ruby 3.3.4:**
   ```sh
   rbenv install 3.3.4
   rbenv global 3.3.4
   ```

5. **Verify the installation:**
   ```sh
   ruby -v
   ```

## Follow React Setup Guide

Follow the React setup guide to ensure Ruby/React is installed:
[React Native Environment Setup](https://reactnative.dev/docs/set-up-your-environment)

## Install Node and Watchman

1. **Install Node:**
   ```sh
   brew install node
   ```

2. **Install Watchman:**
   ```sh
   brew install watchman
   ```

## Create React Project

1. **Uninstall previous React Native CLI:**
   ```sh
   npm uninstall -g react-native-cli @react-native-community/cli
   ```

2. **Initialize a new React Native project:**
   ```sh
   npx @react-native-community/cli@latest init CXDemoReact
   cd CXDemoReact
   ```

3. **Install necessary packages:**
   ```sh
   npm install react-native-url-polyfill
   npm install @coralogix/react-native-sdk
   npm install --save react-native-device-info
   ```

4. **Copy contents of App.tsx**
  
   Copy the contents of the premade 'App.tsx' overwriting the existing template in 'CXDemoReact'.
  

## iOS Setup

1. **Install Pods:**
   ```sh
   cd CXDemoReact/ios
   pod install
   ```

## Android Setup

1. **Add the following to your shell configuration file:**
   ```sh
   export ANDROID_HOME=$HOME/Library/Android/sdk
   export PATH=$PATH:$ANDROID_HOME/emulator
   export PATH=$PATH:$ANDROID_HOME/tools
   export PATH=$PATH:$ANDROID_HOME/tools/bin
   export PATH=$PATH:$ANDROID_HOME/platform-tools
   ```

## Start the App

1. **Run on iOS simulator:**
   ```sh
   npx react-native run-ios --simulator="iPhone 15"
   ```

2. **Run on Android:**
   ```sh
   npx react-native run-android
   ```
