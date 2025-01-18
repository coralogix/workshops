npm init -y

npm install typescript \
  ts-node \
  @types/node \
  express \
  @types/express \
  pino

# initialize typescript
npx tsc --init

npm install @opentelemetry/sdk-node \
  @opentelemetry/api \
  @opentelemetry/auto-instrumentations-node \
  @opentelemetry/sdk-trace-node \
  @opentelemetry/semantic-conventions