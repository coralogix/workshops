package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"os"
	"time"

	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/attribute"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracegrpc"
	"go.opentelemetry.io/otel/sdk/resource"
	sdktrace "go.opentelemetry.io/otel/sdk/trace"
	semconv "go.opentelemetry.io/otel/semconv/v1.26.0"
	"go.opentelemetry.io/otel/trace"
)

const (
	traceName = "my_trace"
)

func init() {
	ctx := context.Background()

	traceConnOpts := []otlptracegrpc.Option{
		otlptracegrpc.WithTimeout(1 * time.Second),
		otlptracegrpc.WithInsecure(), // Use plain HTTP for local Collector
	}

	exporter, err := otlptracegrpc.New(ctx, traceConnOpts...)
	if err != nil {
		log.Fatalf("failed to create trace exporter: %v", err)
	}

	res, err := resource.Merge(
		resource.Default(),
		resource.NewWithAttributes(
			semconv.SchemaURL,
			semconv.ServiceNameKey.String("go-manual-instro-traces-example"),
		),
	)

	sp := sdktrace.NewSimpleSpanProcessor(exporter)
	tp := sdktrace.NewTracerProvider(
		sdktrace.WithSampler(sdktrace.AlwaysSample()),
		sdktrace.WithResource(res),
		sdktrace.WithSpanProcessor(sp),
	)

	otel.SetTracerProvider(tp)
}

func logJSON(severity, message string, traceID, spanID string, data map[string]interface{}) {
	entry := map[string]interface{}{
		"timestamp": time.Now().Format(time.RFC3339),
		"severity":  severity,
		"message":   message,
		"trace_id":  traceID,
		"span_id":   spanID,
	}

	for k, v := range data {
		entry[k] = v
	}

	jsonData, err := json.Marshal(entry)
	if err != nil {
		log.Printf("Failed to marshal log entry: %v", err)
		return
	}

	fmt.Fprintln(os.Stdout, string(jsonData))
}

func rollhanler(w http.ResponseWriter, r *http.Request) {
	tracer := otel.Tracer("cx.example.tracer")
	ctx, span := tracer.Start(r.Context(), "rollhandle", trace.WithSpanKind(trace.SpanKindServer))
	defer span.End()

	roll := rolldice(ctx)

	span.SetAttributes(
		attribute.String("span.kind", "server"),
		attribute.String("resource.name", r.Method+" "+r.URL.Path),
		attribute.String("http.method", r.Method),
		attribute.String("http.url", r.URL.Path),
		attribute.String("http.route", r.URL.Path),
		attribute.String("http.target", r.URL.String()),
		attribute.String("http.useragent", r.UserAgent()),
		attribute.String("http.host", r.Host),
	)

	logJSON("INFO", "Dice roll handled", span.SpanContext().TraceID().String(), span.SpanContext().SpanID().String(), map[string]interface{}{
		"roll_result": roll,
		"http_method": r.Method,
		"http_url":    r.URL.Path,
	})

	fmt.Fprintf(w, "rolled a %d\n", roll)
}

func rolldice(ctx context.Context) int {
	roll := rand.Intn(6) + 1
	tracer := otel.Tracer(traceName)
	_, span := tracer.Start(ctx, "roll-dice")
	defer span.End()
	span.SetAttributes(attribute.Int("dice.roll", roll))

	logJSON("INFO", "Dice roll completed", span.SpanContext().TraceID().String(), span.SpanContext().SpanID().String(), map[string]interface{}{
		"dice_roll": roll,
	})

	return roll
}

func main() {
	http.HandleFunc("/roll", rollhanler)
	go func() {
		client := &http.Client{}
		for {
			time.Sleep(time.Duration(rand.Intn(1500)) * time.Millisecond)

			resp, err := client.Get("http://localhost:8080/roll")
			if err != nil {
				logJSON("ERROR", "Error making self-request", "", "", map[string]interface{}{
					"error": err.Error(),
				})
				continue
			}

			resp.Body.Close()
			logJSON("INFO", "Self-request completed", "", "", nil)
		}
	}()

	log.Println("serving... :8080/roll")
	http.ListenAndServe(":8080", nil)
}
