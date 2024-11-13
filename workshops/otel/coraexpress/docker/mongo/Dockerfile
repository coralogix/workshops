# Start with the official MongoDB image
FROM mongo

# Copy custom MongoDB configuration file to the container
# COPY mongod.conf /etc/mongod.conf

# Expose the default MongoDB port
EXPOSE 27017

# Run MongoDB with the custom configuration file
# CMD ["mongod", "--config", "/etc/mongod.conf"]

# Optional: Add a health check to ensure MongoDB is up and responsive
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s CMD mongo --eval "db.adminCommand('ping')" || exit 1
