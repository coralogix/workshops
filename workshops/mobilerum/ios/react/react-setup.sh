# Install Homebrew if not already installed
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
brew update

# Install necessary packages
brew install node
brew install watchman
brew install ruby-build
brew install rbenv

# Install `n` to manage Node.js versions
brew install n

# Install Node.js version 20.16.0 using `n`
sudo n 20.16.0

# Verify the Node.js installation
node -v

# Initialize rbenv
rbenv init

# Add rbenv init to .bash_profile
echo 'if command -v rbenv 1>/dev/null 2>&1; then
  eval "$(rbenv init -)"
fi' >> ~/.bash_profile

# Source the profile to load rbenv
source ~/.bash_profile

# Install Ruby and set it globally
rbenv install 3.3.4
rbenv global 3.3.4

# Update RubyGems system software
gem update --system 3.5.16

# Verify Ruby installation
ruby -v

# Install CocoaPods
sudo gem install cocoapods

# Uninstall any globally installed react-native-cli
npm uninstall -g react-native-cli

# Initialize a new React Native project with npm
npx react-native init MyReactApp --template react-native-template-typescript

#select Yes to installing gems

# copy contents of App.tsx into MyReactApp/App.tsx

# Start React Native Bundler- better to do this manually
# cd MyReactApp
# npx react-native start

# At this time, you can run in Xcode and send app to Simulator