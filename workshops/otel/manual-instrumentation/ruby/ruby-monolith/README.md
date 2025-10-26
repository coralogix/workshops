# Ruby OpenTelemetry Workshop

This workshop demonstrates how to instrument a Ruby on Rails application with OpenTelemetry, following the official [OpenTelemetry Ruby Getting Started Guide](https://opentelemetry.io/docs/languages/ruby/getting-started/).

## What You'll Learn

- How to set up OpenTelemetry in a Rails application
- Automatic instrumentation with `opentelemetry-instrumentation-all`
- How to view traces in console output
- Understanding span data and trace structure

## Prerequisites

- Ruby 3.2+ (✅ Already installed: Ruby 3.2.3)
- Bundler (✅ Installed during setup)

## Workshop Structure

### Quick Start (Use Existing App)
The repository includes a pre-built `dice-ruby/` Rails application with OpenTelemetry already configured. You can skip setup and go directly to:
```bash
./02-run-workshop-collector.sh
```

### Full Experience (Build From Scratch)
To experience the complete setup process:
```bash
rm -rf dice-ruby/  # Remove existing app
./01-configure-ruby.sh  # Build from scratch
```

### 1. Setup Ruby Environment (Optional)
```bash
./01-configure-ruby.sh
```
This script:
- Installs Rails with proper user permissions
- Installs Bundler
- Creates the dice-ruby application (if it doesn't exist)
- Sets up OpenTelemetry dependencies

### 2. Run the Workshop
```bash
./02-run-workshop-collector.sh
```
This script:
- Starts the Rails application with OTLP exporter on **port 8081**
- Sends traces to OpenTelemetry Collector at http://localhost:4318
- Works with any OTLP-compatible backend (Jaeger, Zipkin, etc.)

### 3. Test the Application  
#### ----> In a separate terminal:
```bash
./03-test-endpoints.sh
```
This script:
- Tests the health check endpoint
- Makes multiple requests to the dice rolling endpoint
- Shows you what to look for in the trace output  

You can now check to see traces sent via the OpenTelemetry Collector  
  
## Application Details

### The Dice Application
- **Framework**: Rails 8.1.0 (API-only)
- **Service Name**: `dice-ruby`
- **Endpoints**:
  - `GET /rolldice` - Returns a random number 1-6
  - `GET /up` - Health check endpoint

### OpenTelemetry Configuration
Located in `dice-ruby/config/initializers/opentelemetry.rb`:

```ruby
require 'opentelemetry/sdk'
require 'opentelemetry/instrumentation/all'
require 'opentelemetry-exporter-otlp'

OpenTelemetry::SDK.configure do |c|
  c.service_name = 'dice-ruby'
  c.use_all() # enables all instrumentation!
end
```

This configuration follows the [official OpenTelemetry Ruby exporters documentation](https://opentelemetry.io/docs/languages/ruby/exporters/).

### Automatic Instrumentation
The `opentelemetry-instrumentation-all` gem provides automatic instrumentation for:
- **Rails**: Request/response cycles, controller actions
- **Rack**: HTTP middleware instrumentation  
- **ActiveRecord**: Database query instrumentation
- **ActionView**: Template rendering instrumentation
- **ActiveJob**: Background job instrumentation
- **And many more...**

## Understanding the Trace Output

When you make requests, you'll see trace spans in the console like:

```ruby
#<struct OpenTelemetry::SDK::Trace::SpanData
 name="GET /rolldice",
 kind=:server,
 status=#<OpenTelemetry::Trace::Status:0x... @code=1, @description="">,
 attributes={
   "http.method"=>"GET",
   "http.host"=>"localhost:8080",
   "http.target"=>"/rolldice",
   "code.namespace"=>"DiceController",
   "code.function"=>"roll",
   "http.status_code"=>200
 },
 # ... more trace data
>
```

### Key Span Attributes
- **name**: Operation name (e.g., "GET /rolldice")
- **kind**: Span type (:server, :client, :internal)
- **attributes**: Key-value pairs with request details
- **trace_id**: Unique identifier linking related spans
- **span_id**: Unique identifier for this specific span

## Next Steps

After completing this workshop, you can:

1. **Export to Real Backends**: Replace console exporter with OTLP exporter
   ```bash
   env OTEL_EXPORTER_OTLP_ENDPOINT=http://your-collector:4318 rails server
   ```

2. **Add Custom Instrumentation**: Add manual spans and metrics
3. **Configure Sampling**: Control trace volume in production
4. **Add Context Propagation**: Connect traces across services

## Troubleshooting

### Permission Errors
If you get gem permission errors, ensure you're using user-level installation:
```bash
export PATH="$HOME/.local/share/gem/ruby/3.2.0/bin:$PATH"
gem install <gem_name> --user-install
```

### Missing System Dependencies
If native gem compilation fails:
```bash
sudo apt update
sudo apt install -y libyaml-dev build-essential
```

### Bundle Install Issues
Configure bundler to install locally:
```bash
bundle config set --local path 'vendor/bundle'
bundle install
```

## Debug Mode (Advanced)

For debugging and learning purposes, you can run the workshop with console trace output instead of sending to a collector:

```bash
./debug-run-workshop-console.sh
```

This debug script:
- Starts the Rails application with console trace exporter on **port 8080**
- Shows trace data directly in the terminal output
- Perfect for learning how OpenTelemetry spans are structured
- Useful for troubleshooting trace generation issues

**Note**: The debug mode can run simultaneously with the main workshop since it uses a different port.

## Workshop Files

- `01-configure-ruby.sh` - Ruby environment setup
- `02-run-workshop-collector.sh` - Start Rails app with OTLP exporter (production mode)
- `debug-run-workshop-console.sh` - Start Rails app with console traces (debug mode)
- `03-test-endpoints.sh` - Test the application endpoints
- `dice-ruby/` - The Rails application with OpenTelemetry
- `README.md` - This documentation

## Resources

- [OpenTelemetry Ruby Documentation](https://opentelemetry.io/docs/languages/ruby/)
- [OpenTelemetry Ruby Getting Started](https://opentelemetry.io/docs/languages/ruby/getting-started/)
- [OpenTelemetry Ruby Registry](https://opentelemetry.io/ecosystem/registry/?language=ruby)
- [Rails Guides](https://guides.rubyonrails.org/)

---

**Happy Tracing!** 🚀
