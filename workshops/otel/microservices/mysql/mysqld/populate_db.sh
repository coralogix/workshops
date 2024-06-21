#!/bin/bash
# Wait for MySQL to fully start and be ready to accept queries
echo "Waiting for MySQL to fully start..."
until mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" -e "SELECT 1" "$MYSQL_DATABASE" &> /dev/null; do
    echo "MySQL is not up yet - waiting..."
    sleep 1
done
echo "MySQL started."

echo "Creating schema if it does not exist..."
mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DATABASE" -e "
CREATE TABLE IF NOT EXISTS random_numbers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    number INT NOT NULL
);
"

echo "Populating the database with 100,000 random numbers."
for i in $(seq 1 10); do
    sql="INSERT INTO random_numbers (number) VALUES "
    for j in $(seq 1 10000); do
        num=$((RANDOM % 1000 + 1))
        sql+="($num),"
    done
    sql="${sql%,};" # Remove the last comma and end the statement
    mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DATABASE" -e "$sql"
done
echo "Data population complete."
