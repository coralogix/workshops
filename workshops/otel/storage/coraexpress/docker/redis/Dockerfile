# Use the official Redis image as a base
FROM redis

# Expose the default Redis port
EXPOSE 6379

# Copy the custom Redis configuration file
COPY ./redis.conf /usr/local/etc/redis/redis.conf

# Create the log directory
RUN mkdir -p /var/log/redis

# Set up Redis to use the custom configuration file on startup
CMD ["redis-server", "/usr/local/etc/redis/redis.conf"]

# Optional: Add a health check to ensure the Redis server is up and running
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD redis-cli ping || exit 1
