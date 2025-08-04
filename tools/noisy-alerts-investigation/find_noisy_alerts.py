#!/usr/bin/env python3

import os
import argparse
import datetime
import json
import subprocess

# Default threshold for a noisy alert (in minutes)
DEFAULT_THRESHOLD_MINUTES = 5

def get_incidents(api_key, start_time, end_time, region):
    """
    Fetches incidents from the Coralogix API.
    """
    # Try with a minimal request body - just empty object
    request_body = {}

    # Handle region parameter - if it already contains .coralogix.com, use it directly
    if region.endswith('.coralogix.com'):
        endpoint = f"ng-api-grpc.{region}:443"
    else:
        endpoint = f"ng-api-grpc.{region}.coralogix.com:443"
    
    cmd = [
        "grpcurl",
        "-H", f"Authorization: Bearer {api_key}",
        "-d", json.dumps(request_body),
        endpoint,
        "com.coralogixapis.incidents.v1.IncidentsService/ListIncidents"
    ]

    try:
        result = subprocess.run(cmd, capture_output=True, text=True, check=True)
        response = json.loads(result.stdout)
        all_incidents = response.get("incidents", [])
        
        # Filter incidents by time locally since the API doesn't seem to support time filtering
        filtered_incidents = []
        for incident in all_incidents:
            if incident.get("createdAt"):
                created_at_str = incident["createdAt"].replace('Z', '+00:00')
                created_at = datetime.datetime.fromisoformat(created_at_str)
                if start_time <= created_at <= end_time:
                    filtered_incidents.append(incident)
        
        return filtered_incidents
    except subprocess.CalledProcessError as e:
        print(f"Error calling Coralogix API: {e}")
        print(f"Stderr: {e.stderr}")
        return None
    except json.JSONDecodeError as e:
        print(f"Error decoding JSON from Coralogix API: {e}")
        print(f"Response: {result.stdout}")
        return None

def find_noisy_alerts(incidents, threshold_minutes):
    """
    Identifies noisy alerts from a list of incidents.
    """
    noisy_alerts = []
    threshold = datetime.timedelta(minutes=threshold_minutes)

    for incident in incidents:
        if incident.get("createdAt") and incident.get("closedAt"):
            created_at_str = incident["createdAt"].replace('Z', '+00:00')
            closed_at_str = incident["closedAt"].replace('Z', '+00:00')

            created_at = datetime.datetime.fromisoformat(created_at_str)
            closed_at = datetime.datetime.fromisoformat(closed_at_str)
            
            duration = closed_at - created_at
            if duration < threshold:
                noisy_alerts.append(incident)
    
    return noisy_alerts

def summarize_noisy_alerts_by_name(noisy_alerts):
    """
    Groups noisy alerts by alert name, counts occurrences, and finds the last fired time.
    """
    from collections import defaultdict
    grouped = defaultdict(list)
    for alert in noisy_alerts:
        # Get alert name from contextualLabels.alert_name
        contextual_labels = alert.get("contextualLabels", {})
        name = contextual_labels.get("alert_name", "<no name>")
        grouped[name].append(alert)
    
    summary = []
    for name, alerts in grouped.items():
        count = len(alerts)
        # Find the latest createdAt
        last_fired = max(
            (a.get("createdAt") for a in alerts if a.get("createdAt")),
            default=None
        )
        summary.append({
            "name": name,
            "count": count,
            "last_fired": last_fired
        })
    # Sort by count descending, then name
    summary.sort(key=lambda x: (-x["count"], x["name"]))
    return summary

def main():
    """
    Main function to find noisy alerts.
    """
    parser = argparse.ArgumentParser(description="Find noisy alerts from Coralogix incidents.")
    parser.add_argument("--api-key", help="Coralogix API key", default=os.environ.get("CX_API_KEY"))
    parser.add_argument("--region", help="Coralogix region (e.g., eu1.coralogix.com)", default=os.environ.get("CORALOGIX_REGION", "coralogix.com"))
    parser.add_argument("--time-window-hours", type=int, default=24, help="Time window in hours to look back for incidents.")
    parser.add_argument("--threshold-minutes", type=int, default=DEFAULT_THRESHOLD_MINUTES, help="Threshold in minutes to consider an alert as noisy.")
    args = parser.parse_args()

    if not args.api_key:
        print("Error: Coralogix API key is required. Set the CX_API_KEY environment variable or use the --api-key flag.")
        return

    end_time = datetime.datetime.now(datetime.timezone.utc)
    start_time = end_time - datetime.timedelta(hours=args.time_window_hours)

    print(f"Fetching incidents from {start_time} to {end_time}...")
    
    incidents = get_incidents(args.api_key, start_time, end_time, args.region)

    if incidents is not None:
        print(f"Retrieved {len(incidents)} total incidents")
        if len(incidents) == 0:
            print("No incidents found in the specified time window.")
            return
            
        noisy_alerts = find_noisy_alerts(incidents, args.threshold_minutes)
        if noisy_alerts:
            print(f"\nFound {len(noisy_alerts)} noisy alerts (opened and closed in less than {args.threshold_minutes} minutes):")
            
            summary = summarize_noisy_alerts_by_name(noisy_alerts)
            print(f"\nNoisy alerts grouped by alert name:")
            print(f"{'Alert Name':50} {'Count':>5} {'Last Fired':>30}")
            print("-" * 90)
            for item in summary:
                print(f"{item['name'][:50]:50} {item['count']:>5} {item['last_fired']:>30}")
        else:
            print("No noisy alerts found.")

if __name__ == "__main__":
    main()

