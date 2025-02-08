# Use official Ruby image as a base
FROM ruby:3.2.3

# Set the working directory in the container
WORKDIR /app

# Install dependencies for building native gems (for example, building 'google-protobuf')
RUN apt-get update && apt-get install -y build-essential 

# Install Bundler
RUN gem install bundler -v 2.4.20

# Copy the Gemfile and Gemfile.lock into the container
COPY Gemfile Gemfile.lock ./

# Install Ruby dependencies using Bundler
RUN bundle install

# Copy the rest of the application files into the container
COPY . .

# Run the Ruby application when the container starts
CMD ["ruby", "cx_ruby_metrics_logger.rb"]
