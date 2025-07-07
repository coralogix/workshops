# Simple Coralogix Metrics Sender

A minimal TypeScript application that generates a random metric every 3 seconds and sends it to Coralogix using OpenTelemetry and the OTLP gRPC exporter.

## Features

- 🎲 Generates a random metric (`simple_counter`) every 3 seconds
- 📡 Sends metrics to Coralogix via OpenTelemetry OTLP gRPC
- 📱 Customizable application and subsystem names
- 🛑 Graceful shutdown handling

## Prerequisites

- Node.js (v14 or higher)
- npm

## Installation

1. Install dependencies:
   ```bash
   npm install
   ```
2. Build the TypeScript code:
   ```bash
   npm run build
   ```

## Usage

1. Set your Coralogix private key as an environment variable:
   ```bash
   export CORALOGIX_PRIVATE_KEY="your-private-key-here"
   ```
   Optionally, set the application and subsystem names:
   ```bash
   export CORALOGIX_APP_NAME="your-app-name"
   export CORALOGIX_SUBSYSTEM="your-subsystem-name"
   ```
2. Start the metrics sender:
   ```bash
   npm start
   ```

## Output Example

```
✅ Coralogix setup complete
📊 Metric Name: simple_counter
📱 Application: simple-app
🔧 Subsystem: simple-metrics
🚀 Simple Coralogix Metrics Sender
📊 Sending simple_counter every 3 seconds
🔍 Look for in Coralogix: simple_counter (simple-app/simple-metrics)
⏹️  Press Ctrl+C to stop

📊 simple_counter #1 | Time: 12:00:00 PM | Value: 7 | Sent to Coralogix
📊 simple_counter #2 | Time: 12:00:03 PM | Value: 2 | Sent to Coralogix
...
```

## Environment Variables

| Variable                | Required | Default         | Description                        |
|-------------------------|----------|----------------|------------------------------------|
| `CORALOGIX_PRIVATE_KEY` | Yes      | (none)         | Your Coralogix private key         |
| `CORALOGIX_APP_NAME`    | No       | simple-app     | Application name in Coralogix      |
| `CORALOGIX_SUBSYSTEM`   | No       | simple-metrics | Subsystem name in Coralogix        |

## Project Structure

```
coralogix-typescript-example/
├── simple-coralogix.ts      # Main application file
├── package.json             # Dependencies and scripts
├── package-lock.json        # Dependency lock file
├── tsconfig.json            # TypeScript configuration
└── README.md                # This file
```

## Stopping the Application

Press `Ctrl+C` to gracefully stop the metrics sender. The application will display a summary of metrics sent.

## Troubleshooting

- **No metrics in Coralogix?**
  - Double-check your `CORALOGIX_PRIVATE_KEY` and Coralogix dashboard filters.
  - Ensure your network allows outbound gRPC connections.
- **TypeScript errors?**
  - Run `npm run type-check` to check for issues.
  - Run `npm install` to ensure all dependencies are installed.

## License

This project is for educational and demonstration purposes. 