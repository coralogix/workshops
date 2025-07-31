# Coralogix Noisy Alerts Investigation Tool

A Python script to identify and analyze "noisy alerts" in Coralogix - incidents that open and close within a short time period, indicating they might be too sensitive or have incorrect thresholds.

## What are Noisy Alerts?

Noisy alerts are incidents that:
- Open and close very quickly (default: within 5 minutes)
- Often indicate overly sensitive alert thresholds
- Can create alert fatigue for your team
- May need threshold adjustments or alert rule tuning

## Features

- 🔍 **Fetches incidents** from Coralogix API using gRPC
- ⚡ **Identifies noisy alerts** (configurable threshold, default: 5 minutes)
- 📊 **Groups alerts by name** and shows count and last fired time
- 🌍 **Supports multiple Coralogix regions** (EU1, US2, etc.)
- ⏰ **Configurable time windows** and thresholds
- 📈 **Easy-to-read output** with alert summaries

## Quick Start

### Prerequisites

- Python 3.6+
- `grpcurl` command-line tool
- Coralogix API key with incidents service access

### Installation

1. **Install Python dependencies:**
```bash
pip install -r requirements.txt
```

2. **Install grpcurl:**
```bash
# macOS
brew install grpcurl

# Ubuntu/Debian
sudo apt-get install grpcurl

# Or download from: https://github.com/fullstorydev/grpcurl/releases
```

### Usage

**Basic usage:**
```bash
# Set your API key
export CX_API_KEY="your_api_key_here"

# Run with default settings (EU1 region, 24-hour window, 5-minute threshold)
python find_noisy_alerts.py
```

**Advanced usage:**
```bash
# Specify region and API key
python find_noisy_alerts.py --api-key "your_api_key" --region "eu1.coralogix.com"

# Custom time window and threshold
python find_noisy_alerts.py --time-window-hours 48 --threshold-minutes 10

# Use environment variables
export CORALOGIX_REGION="eu1.coralogix.com"
export CX_API_KEY="your_api_key"
python find_noisy_alerts.py
```

## Underlying gRPC Command

The script uses `grpcurl` to communicate with the Coralogix API. Here's the underlying command structure:

### Basic gRPC Command
```bash
grpcurl -H "Authorization: Bearer YOUR_API_KEY" \
        -d "{}" \
        ng-api-grpc.REGION.coralogix.com:443 \
        com.coralogixapis.incidents.v1.IncidentsService/ListIncidents
```

### Examples by Region

**EU1 Region:**
```bash
grpcurl -H "Authorization: Bearer YOUR_API_KEY" \
        -d "{}" \
        ng-api-grpc.eu1.coralogix.com:443 \
        com.coralogixapis.incidents.v1.IncidentsService/ListIncidents
```

**US2 Region:**
```bash
grpcurl -H "Authorization: Bearer YOUR_API_KEY" \
        -d "{}" \
        ng-api-grpc.us2.coralogix.com:443 \
        com.coralogixapis.incidents.v1.IncidentsService/ListIncidents
```

### Testing API Connectivity

You can test if your API key has access to the incidents service:

**List available services:**
```bash
grpcurl -H "Authorization: Bearer YOUR_API_KEY" \
        ng-api-grpc.REGION.coralogix.com:443 \
        list
```

**Check if incidents service is available:**
```bash
grpcurl -H "Authorization: Bearer YOUR_API_KEY" \
        ng-api-grpc.REGION.coralogix.com:443 \
        list | grep -i incident
```

### Troubleshooting gRPC Commands

**If you get "target server does not expose service":**
- Check that your API key has access to the incidents service
- Verify the region is correct
- Try listing available services to see what's accessible

**If you get authentication errors:**
- Verify your API key is valid
- Check that the key is for the correct region
- Ensure the key has the necessary permissions

## Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `--api-key` | Coralogix API key | `CX_API_KEY` environment variable |
| `--region` | Coralogix region (e.g., eu1.coralogix.com) | `CORALOGIX_REGION` env var or "coralogix.com" |
| `--time-window-hours` | Time window in hours to look back | 24 |
| `--threshold-minutes` | Threshold in minutes to consider an alert as noisy | 5 |

## Example Output

```
Fetching incidents from 2025-07-29 19:46:26.114769+00:00 to 2025-07-30 19:46:26.114769+00:00...
Retrieved 3485 total incidents

Found 1179 noisy alerts (opened and closed in less than 5 minutes):

Noisy alerts grouped by alert name:
Alert Name                                         Count                     Last Fired
------------------------------------------------------------------------------------------
Test multi condition Roy                             832       2025-07-21T14:51:39.608Z
Veronika-Test-Alert                                  270       2025-07-30T17:33:29.539Z
Avg CPU per Pod alert                                 62       2025-07-26T19:08:52.124Z
metric_threshold alert example                         5       2025-07-26T22:00:53.091Z
```

## Supported Regions

- **EU1**: `eu1.coralogix.com` (Ireland)
- **US2**: `us2.coralogix.com` (Oregon)
- **Other regions**: Use the appropriate domain for your Coralogix instance

## Troubleshooting

### API Key Issues
- Ensure your API key has access to the incidents service
- Check that the key is valid for the specified region

### Region Issues
- Some regions may not have the incidents service available
- EU1 and US2 are confirmed to work with the incidents service

### grpcurl Issues
- Ensure grpcurl is installed and in your PATH
- Check that you can reach the Coralogix gRPC endpoints

## Getting Help

If you encounter issues:

1. **Check your API key permissions** - Ensure it has access to the incidents service
2. **Verify your region** - Make sure you're using the correct Coralogix domain
3. **Test connectivity** - Try listing available services with `grpcurl`
4. **Contact Coralogix support** - For API access or service availability issues

## Use Cases

### Alert Optimization
Use this tool to identify alerts that are firing too frequently and resolve too quickly, indicating they may need threshold adjustments.

### Alert Fatigue Reduction
Find alerts that are creating noise in your monitoring system and tune them appropriately.

### Monitoring Health Check
Regularly run this tool to audit your alert configurations and ensure they're working as intended.

## Integration with Coralogix Workshops

This tool is part of the Coralogix workshops collection, designed to help users optimize their observability setup. It complements other tools in the workshops for comprehensive monitoring and alerting best practices.

---

**Note:** This tool is designed to help identify and optimize your Coralogix alert configurations. Use the results to tune your alert thresholds and reduce alert fatigue in your monitoring setup.
