// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

// Go Auto-Instrumentation Reference Example
//
// This example demonstrates OpenTelemetry auto-instrumentation for Go applications.
//
// CAPABILITIES:
// - Automatic distributed tracing via eBPF instrumentation
// - HTTP request/response tracing without code changes
// - Integration with observability platforms (Jaeger, etc.)
// - Kubernetes-native deployment with operator injection
//
// LIMITATIONS:
// - Trace context not accessible to application code
// - Direct log/trace correlation not possible
// - trace.SpanFromContext(ctx) returns invalid spans
//
// This is a known limitation of Go auto-instrumentation compared to manual SDK
// or auto-instrumentation in other languages like Java/.NET.
//
// WORKAROUNDS FOR CORRELATION:
// - Use request IDs for correlation
// - Correlate by timestamp and service name
// - Use structured logging with consistent fields
package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	mathrand "math/rand"
	"net/http"
	"os"
	"os/signal"
	"strconv"
	"strings"
	"sync"
	"syscall"
	"time"

	"github.com/sirupsen/logrus"
	"go.opentelemetry.io/otel/trace"
)

type Response struct {
	Message   string    `json:"message"`
	Timestamp time.Time `json:"timestamp"`
	Server    string    `json:"server"`
	RequestID string    `json:"request_id"`
}

type HealthCheck struct {
	Status    string    `json:"status"`
	Timestamp time.Time `json:"timestamp"`
	Uptime    string    `json:"uptime"`
}

type ClientConfig struct {
	ServerURL     string
	RequestDelay  time.Duration
	ClientName    string
	TotalRequests int
}

// LogEntry represents a structured log entry
type LogEntry struct {
	Timestamp time.Time              `json:"timestamp"`
	Level     string                 `json:"level"`
	Message   string                 `json:"message"`
	Data      map[string]interface{} `json:"data,omitempty"`
}

var startTime = time.Now()

// Initialize structured JSON logging with logrus
func initLogger() {
	logrus.SetFormatter(&logrus.JSONFormatter{
		TimestampFormat: time.RFC3339Nano,
		FieldMap: logrus.FieldMap{
			logrus.FieldKeyTime:  "timestamp",
			logrus.FieldKeyLevel: "level",
			logrus.FieldKeyMsg:   "message",
		},
	})
	logrus.SetOutput(os.Stdout)
	logrus.SetLevel(logrus.InfoLevel)
}

// Auto-Instrumentation Trace Context Check
//
// In Go auto-instrumentation, trace context is NOT accessible to application code.
// This function demonstrates the limitation and provides consistent empty fields
// for log structure compatibility.
func getTraceContext(ctx context.Context) (string, string) {
	// NOTE: This will always return empty strings with auto-instrumentation
	// Go auto-instrumentation works at eBPF level, not application context level
	span := trace.SpanFromContext(ctx)
	if span != nil {
		sc := span.SpanContext()
		if sc.IsValid() {
			return sc.TraceID().String(), sc.SpanID().String()
		}
	}
	// Return empty strings for consistent log structure
	return "", ""
}

// Structured logging with auto-instrumentation best practices
func logWithStructure(ctx context.Context, level logrus.Level, msg string, fields logrus.Fields) {
	// Get trace context (will be empty with auto-instrumentation)
	traceID, spanID := getTraceContext(ctx)

	// Always include trace fields for consistent log structure
	if fields == nil {
		fields = logrus.Fields{}
	}
	fields["trace_id"] = traceID // Will be empty with auto-instrumentation
	fields["span_id"] = spanID   // Will be empty with auto-instrumentation

	// Add service identification for correlation
	fields["service_name"] = getEnv("SERVICE_NAME", "go-autoinject-demo")

	// Log with consistent structure
	logrus.WithFields(fields).Log(level, msg)
}

// Convenience wrapper for info level logging
func logInfo(ctx context.Context, msg string, fields logrus.Fields) {
	logWithStructure(ctx, logrus.InfoLevel, msg, fields)
}

// Convenience wrapper for error level logging
func logError(ctx context.Context, msg string, fields logrus.Fields) {
	logWithStructure(ctx, logrus.ErrorLevel, msg, fields)
}

func main() {
	// Initialize structured logging
	initLogger()

	mode := getEnv("MODE", "server")
	if len(os.Args) > 1 {
		mode = strings.ToLower(os.Args[1])
	}

	ctx := context.Background()

	logInfo(ctx, "Starting application", logrus.Fields{"mode": mode})

	ctx, cancel := context.WithCancel(ctx)
	defer cancel()

	// Handle graceful shutdown
	c := make(chan os.Signal, 1)
	signal.Notify(c, os.Interrupt, syscall.SIGTERM)

	var wg sync.WaitGroup

	switch mode {
	case "server":
		wg.Add(1)
		go func() {
			defer wg.Done()
			runServer(ctx)
		}()
	case "client":
		wg.Add(1)
		go func() {
			defer wg.Done()
			runClient(ctx)
		}()
	case "both":
		wg.Add(2)
		go func() {
			defer wg.Done()
			runServer(ctx)
		}()
		go func() {
			defer wg.Done()
			// Wait a bit for server to start before starting client
			time.Sleep(2 * time.Second)
			runClient(ctx)
		}()
	default:
		logInfo(ctx, "Invalid mode", logrus.Fields{
			"mode":        mode,
			"valid_modes": []string{"server", "client", "both"},
		})
		os.Exit(1)
	}

	// Wait for interrupt signal
	go func() {
		<-c
		logInfo(ctx, "Received shutdown signal", nil)
		cancel()
	}()

	wg.Wait()
	logInfo(ctx, "Application stopped", nil)
}

func runServer(ctx context.Context) {
	port := getEnv("PORT", "8080")
	serverName := getEnv("SERVER_NAME", "go-server")

	mux := http.NewServeMux()

	// Simple handlers - auto-instrumentation will handle tracing automatically
	mux.HandleFunc("/", homeHandler(serverName))
	mux.HandleFunc("/api/data", dataHandler(serverName))
	mux.HandleFunc("/api/slow", slowHandler(serverName))
	mux.HandleFunc("/api/error", errorHandler(serverName))
	mux.HandleFunc("/health", healthHandler())
	mux.HandleFunc("/healthz", healthHandler())

	server := &http.Server{
		Addr:    ":" + port,
		Handler: mux,
	}

	logInfo(ctx, "Server starting", logrus.Fields{
		"server_name": serverName,
		"port":        port,
		"endpoints":   []string{"/", "/api/data", "/api/slow", "/api/error", "/health", "/healthz"},
	})

	go func() {
		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			logError(ctx, "Server failed", logrus.Fields{"error": err.Error()})
		}
	}()

	<-ctx.Done()
	logInfo(ctx, "Shutting down server", nil)

	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer shutdownCancel()

	if err := server.Shutdown(shutdownCtx); err != nil {
		logError(shutdownCtx, "Server shutdown error", logrus.Fields{"error": err.Error()})
	} else {
		logInfo(shutdownCtx, "Server stopped gracefully", nil)
	}
}

func runClient(ctx context.Context) {
	config := ClientConfig{
		ServerURL:     getEnv("SERVER_URL", "http://localhost:8080"),
		RequestDelay:  getDurationEnv("REQUEST_DELAY", 5*time.Second),
		ClientName:    getEnv("CLIENT_NAME", "go-client"),
		TotalRequests: getIntEnv("TOTAL_REQUESTS", 0), // 0 means infinite
	}

	logInfo(ctx, "Client starting", logrus.Fields{
		"client_name":    config.ClientName,
		"server_url":     config.ServerURL,
		"request_delay":  config.RequestDelay.String(),
		"total_requests": config.TotalRequests,
	})

	client := &http.Client{
		Timeout: 10 * time.Second,
	}

	endpoints := []string{
		"/",
		"/api/data",
		"/api/slow",
		"/api/error",
		"/health",
	}

	requestCount := 0
	ticker := time.NewTicker(config.RequestDelay)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			logInfo(ctx, "Client stopping", logrus.Fields{"total_requests_made": requestCount})
			return
		case <-ticker.C:
			if config.TotalRequests > 0 && requestCount >= config.TotalRequests {
				logInfo(ctx, "Client completed all requests", logrus.Fields{"requests_completed": requestCount})
				return
			}

			// Rotate through different endpoints
			endpoint := endpoints[requestCount%len(endpoints)]
			makeRequest(ctx, client, config.ServerURL+endpoint, config.ClientName, requestCount+1)
			requestCount++
		}
	}
}

func makeRequest(ctx context.Context, client *http.Client, url, clientName string, requestNum int) {
	start := time.Now()

	req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
	if err != nil {
		logError(ctx, "Error creating HTTP request", logrus.Fields{"error": err.Error(), "url": url, "request_num": requestNum})
		return
	}

	// Add custom headers
	req.Header.Set("User-Agent", clientName)
	req.Header.Set("X-Client-Name", clientName)
	req.Header.Set("X-Request-Number", fmt.Sprintf("%d", requestNum))

	resp, err := client.Do(req)
	if err != nil {
		logError(ctx, "HTTP request failed", logrus.Fields{"error": err.Error(), "url": url, "request_num": requestNum, "client_name": clientName})
		return
	}
	defer resp.Body.Close()

	duration := time.Since(start)

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		logError(ctx, "Error reading response body", logrus.Fields{"error": err.Error(), "url": url, "request_num": requestNum})
		return
	}

	// Try to parse the response
	var serverResp Response
	if err := json.Unmarshal(body, &serverResp); err != nil {
		logInfo(ctx, "HTTP request completed (raw response)", logrus.Fields{
			"request_num":   requestNum,
			"url":           url,
			"status_code":   resp.StatusCode,
			"duration_ms":   duration.Milliseconds(),
			"response_body": string(body),
		})
	} else {
		logInfo(ctx, "HTTP request completed", logrus.Fields{
			"request_num": requestNum,
			"url":         url,
			"status_code": resp.StatusCode,
			"duration_ms": duration.Milliseconds(),
			"server_name": serverResp.Server,
			"message":     serverResp.Message,
			"request_id":  serverResp.RequestID,
		})
	}
}

// Server handlers
func homeHandler(serverName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		ctx := r.Context()

		response := Response{
			Message:   "Hello from Go Server!",
			Timestamp: time.Now(),
			Server:    serverName,
			RequestID: generateRequestID(),
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(response)

		logInfo(ctx, "HTTP request served", logrus.Fields{
			"path":        r.URL.Path,
			"method":      r.Method,
			"remote_addr": r.RemoteAddr,
			"user_agent":  r.Header.Get("User-Agent"),
			"request_id":  response.RequestID,
		})
	}
}

func dataHandler(serverName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		ctx := r.Context()

		// Simulate some processing time
		processingTime := time.Duration(mathrand.Intn(100)) * time.Millisecond
		time.Sleep(processingTime)

		response := Response{
			Message:   "Data from Go Server API",
			Timestamp: time.Now(),
			Server:    serverName,
			RequestID: generateRequestID(),
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(response)

		logInfo(ctx, "API data request served", logrus.Fields{
			"path":          r.URL.Path,
			"method":        r.Method,
			"remote_addr":   r.RemoteAddr,
			"processing_ms": processingTime.Milliseconds(),
			"request_id":    response.RequestID,
		})
	}
}

func slowHandler(serverName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		ctx := r.Context()

		// Simulate slow operation
		delay := time.Duration(mathrand.Intn(2000)+500) * time.Millisecond
		time.Sleep(delay)

		response := Response{
			Message:   fmt.Sprintf("Slow response (took %v)", delay),
			Timestamp: time.Now(),
			Server:    serverName,
			RequestID: generateRequestID(),
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(response)

		logInfo(ctx, "Slow request served", logrus.Fields{
			"path":        r.URL.Path,
			"method":      r.Method,
			"remote_addr": r.RemoteAddr,
			"delay_ms":    delay.Milliseconds(),
			"request_id":  response.RequestID,
		})
	}
}

func errorHandler(serverName string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		ctx := r.Context()

		// Randomly return errors
		if mathrand.Intn(100) < 30 { // 30% chance of error
			w.WriteHeader(http.StatusInternalServerError)

			response := Response{
				Message:   "Internal Server Error",
				Timestamp: time.Now(),
				Server:    serverName,
				RequestID: generateRequestID(),
			}

			json.NewEncoder(w).Encode(response)
			logError(ctx, "Error endpoint returned 500", logrus.Fields{
				"path":        r.URL.Path,
				"method":      r.Method,
				"remote_addr": r.RemoteAddr,
				"status_code": 500,
				"request_id":  response.RequestID,
			})
			return
		}

		response := Response{
			Message:   "Success response",
			Timestamp: time.Now(),
			Server:    serverName,
			RequestID: generateRequestID(),
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(response)
		logInfo(ctx, "Error endpoint returned success", logrus.Fields{
			"path":        r.URL.Path,
			"method":      r.Method,
			"remote_addr": r.RemoteAddr,
			"status_code": 200,
			"request_id":  response.RequestID,
		})
	}
}

func healthHandler() http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		ctx := r.Context()
		uptime := time.Since(startTime)

		health := HealthCheck{
			Status:    "healthy",
			Timestamp: time.Now(),
			Uptime:    uptime.String(),
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(health)

		logInfo(ctx, "Health check requested", logrus.Fields{
			"path":        r.URL.Path,
			"method":      r.Method,
			"remote_addr": r.RemoteAddr,
			"uptime":      health.Uptime,
			"status":      health.Status,
		})
	}
}

func generateRequestID() string {
	return fmt.Sprintf("req-%d", mathrand.Intn(1000000))
}

// Utility functions
func getEnv(key, defaultValue string) string {
	if value := os.Getenv(key); value != "" {
		return value
	}
	return defaultValue
}

func getDurationEnv(key string, defaultValue time.Duration) time.Duration {
	if value := os.Getenv(key); value != "" {
		if duration, err := time.ParseDuration(value); err == nil {
			return duration
		}
	}
	return defaultValue
}

func getIntEnv(key string, defaultValue int) int {
	if value := os.Getenv(key); value != "" {
		if intValue, err := strconv.Atoi(value); err == nil {
			return intValue
		}
	}
	return defaultValue
}
