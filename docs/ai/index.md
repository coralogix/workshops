# AI Observability

## Coralogix AI Center Workshop

- This example shows small reference for exercising OpenAI and sending AI telemetry to the Coralogix AI Center  
- An OpenAI Platform Key is required    
- Result will be telemetry in the Coralogix AI Center and associated traces in the Coralogix Trace Explorer and APM dashboards 
- This example is designed to run on a Mac or Linux environment although can be ported to Windows  

In advance be sure to study the [Coralogix AI Center Documentation](https://coralogix.com/docs/user-guides/ai-observability/ai-center/)

### Step 1 - Setup

Clone the repository:

```bash
git clone https://github.com/coralogix/workshops
```

### Step 2 - Configure Example     

Navigate to the `ai/host` directory:

```bash
cd ./workshops/workshops/ai/host
```

Edit the `2-setup-python-env.sh` file and add your Coralogix API key, Endpoint, and OpenAI key  
[Coralogix endpoints are found here](https://coralogix.com/docs/integrations/coralogix-endpoints/)  
  
### Step 3 - Execute the Ai workshop

0. **Start a new terminal** - this must be run in a dedicated terminal

1. **Set up Python env**

    ```bash
    source 1-setup-python.sh
    ```

2. **Set up env variables:**

    ```bash
    source 2-setup-python-env.sh
    ```

3. **Run the Python OpenAI example:**

    ```bash
    source 3-run-python-app.sh
    ```

Study telemetry in Coralogix AI Center, Explore->Tracing, and APM  

4. **Clean up:**

    ```bash
    source 4-cleanup-python.sh
    ```