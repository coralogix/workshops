# === BUILD STAGE ===
FROM golang:1.22.2 AS builder

# Set working directory at the root level
WORKDIR /

# Copy go.mod and go.sum first (to cache dependency downloads)
COPY go.mod go.sum ./

# Download dependencies
RUN go mod download

# Copy the main Go application file explicitly
COPY main.go ./

# Compile the Go application into a static binary
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o app main.go

# === RUNTIME STAGE ===
FROM alpine:latest AS runtime

# Install necessary runtime dependencies
RUN apk --no-cache add ca-certificates curl

# Set working directory
WORKDIR /

# Copy only the compiled binary from the builder stage
COPY --from=builder /app /app

# Run the compiled binary
CMD ["/app"]
