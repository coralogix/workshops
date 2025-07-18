# Build stage
FROM --platform=linux/amd64 golang:1.23-alpine AS builder

WORKDIR /app

# Copy go mod and sum files
COPY go.mod go.sum ./

# Download dependencies (if any)
RUN go mod download

# Copy source code
COPY main.go ./

# Build the application explicitly for x86/amd64
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -a -installsuffix cgo -o app .

# Verify the binary was created
RUN ls -la app

# Final stage - explicitly use amd64
FROM --platform=linux/amd64 alpine:latest

# Install ca-certificates for HTTPS requests and wget for health checks
RUN apk --no-cache add ca-certificates wget

WORKDIR /root/

# Copy the binary from builder stage
COPY --from=builder /app/app .

# Verify the binary exists and is executable
RUN ls -la app && chmod +x app

# Expose port (for server mode)
EXPOSE 8080

# Add health check (will work when running in server or both mode)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Default to server mode, but can be overridden
CMD ["./app", "server"] 