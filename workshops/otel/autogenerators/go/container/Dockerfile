# Stage 1: Build the Go application
FROM golang:alpine3.21 AS build

# Set the Current Working Directory inside the container
WORKDIR /app

# Copy the Go Modules manifests to the container
COPY go.mod go.sum ./

# Download all dependencies. Dependencies will be cached if the go.mod and go.sum files are not changed
RUN go mod tidy

# Copy the rest of the application code
COPY . .

# Build the Go app, explicitly naming the binary 'main'
RUN go build -o main .

# Stage 2: Create the final minimal image to run the app
FROM alpine:latest  

# Set the Current Working Directory inside the container
WORKDIR /root/

# Copy the binary from the build container to the current working directory
COPY --from=build /app/main .

# Expose port 8080 to the outside world
EXPOSE 8080

# Command to run the executable
CMD ["./main"]
