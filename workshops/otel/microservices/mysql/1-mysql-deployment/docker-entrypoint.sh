#!/bin/bash
# Check if the database is populated, and if not, populate it
if [ ! -f "/var/lib/mysql/ibdata1" ]; then
    populate_db.sh
fi

# Execute the main command (provided as CMD in Dockerfile or from docker run)
exec "$@"
