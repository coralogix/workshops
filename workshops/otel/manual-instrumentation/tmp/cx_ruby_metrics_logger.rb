# frozen_string_literal: true

require 'opentelemetry/sdk'
require 'opentelemetry-logs-sdk'
require 'opentelemetry-exporter-otlp-logs'
require 'opentelemetry-metrics-sdk'
require 'opentelemetry-exporter-otlp-metrics'

# Retrieve OpenTelemetry exporter endpoint and other settings from environment variables
otel_exporter_endpoint = ENV['OTEL_EXPORTER_OTLP_ENDPOINT'] || 'http://localhost:4318'  # Default to local if not set
otel_service_name = ENV['OTEL_SERVICE_NAME'] || 'cx_ruby_otel_app'

#  Configure OpenTelemetry
OpenTelemetry::SDK.configure

### ** Logging Setup using endpoint from environment variable **
logger_provider = OpenTelemetry::SDK::Logs::LoggerProvider.new
log_processor = OpenTelemetry::SDK::Logs::Export::BatchLogRecordProcessor.new(
  OpenTelemetry::Exporter::OTLP::Logs::LogsExporter.new(endpoint: otel_exporter_endpoint)
)
logger_provider.add_log_record_processor(log_processor)
logger = logger_provider.logger(name: otel_service_name, version: '0.1.0')

### ** Metrics Setup **
otlp_metric_exporter = OpenTelemetry::Exporter::OTLP::Metrics::MetricsExporter.new(endpoint: otel_exporter_endpoint)
OpenTelemetry.meter_provider.add_metric_reader(otlp_metric_exporter)
meter = OpenTelemetry.meter_provider.meter("Ruby_Meter")

# Create a histogram to track execution duration
duration_histogram = meter.create_histogram(
  'operation_duration_seconds',
  description: 'Duration of key operations in the application',
  unit: 's'
)

# Graceful shutdown on interrupt (e.g., Ctrl+C)
trap("SIGINT") do
  puts "\nShutting down OpenTelemetry providers..."
  OpenTelemetry.meter_provider.metric_readers.each do |reader|
    reader.pull if reader.respond_to?(:pull)
  end
  OpenTelemetry.meter_provider.shutdown
  exit
end

# Infinite loop to continuously emit logs and metrics
loop do
  start_time = Time.now
  random_sleep_duration = rand(1..2)  # Sleep for a random number of seconds between 1 and 5
  sleep(random_sleep_duration)  # Simulate some work
  duration = Time.now - start_time

# Emit a log entry with custom attributes
logger.on_emit(
  timestamp: Time.now,
  attributes: {
    'severity' => 'INFO',  # Explicitly add severity as an attribute
    'message' => "Ruby logs coming through - operation took #{duration.round(2)}s",  # Store message under "message" key
    'rainy' => [true, false].sample
  }
)

  # Record a metric
  duration_histogram.record(duration, attributes: { 'operation' => 'file_processing' })

  # Pull the metrics from metric readers (ensure they exist)
  OpenTelemetry.meter_provider.metric_readers.each do |reader|
    reader.pull if reader.respond_to?(:pull)
  end

  puts "Log and metrics emitted. Sleeping for 2 seconds..."
  sleep 2
end
