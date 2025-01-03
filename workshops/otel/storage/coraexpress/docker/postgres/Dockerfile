# Use the official PostgreSQL image
FROM postgres

# Expose the default PostgreSQL port
EXPOSE 5432

# Set environment variables for PostgreSQL (these can also be set in Kubernetes)
# Replace these values with your own
ENV POSTGRES_DB=postgres
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=postgres

# Optional: Copy a custom PostgreSQL configuration file
# Uncomment the following lines if you have a custom config file named "postgresql.conf"
# COPY ./postgresql.conf /etc/postgresql/postgresql.conf

# Optional: Run PostgreSQL with custom config (uncomment if using a custom config file)
# CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]

# Default command (if no custom config is used)
CMD ["postgres"]

# Optional health check (recommended for production)
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s CMD pg_isready -U $POSTGRES_USER || exit 1
