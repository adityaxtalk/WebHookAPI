# Webhook Receiver

## Overview
This project is a webhook receiver that listens for incoming webhook requests, validates their signatures, and processes alert notifications. It includes email notifications for received alerts and supports GitHub webhook validation.

## Features
- Receive and process alert webhooks
- Validate webhook signatures using HMAC SHA-256
- Store alerts in a database
- Send email notifications for alerts
- Receive and validate GitHub push event webhooks
- Expose API endpoints for webhook processing
- Secure webhook communication using `ngrok` for local development

## Technologies Used
- ASP.NET Core 8
- Entity Framework Core (EF Core)
- SQL Server
- `ngrok` for secure local webhook testing

## Setup & Installation

### Prerequisites
- .NET 8 SDK
- SQL Server
- `ngrok` (for testing webhooks locally)

### Steps to Run the Project
1. Clone the repository:
   ```sh
   git clone https://github.com/adityaxtalk/WebHookAPI.git
   cd WebhookReceiver
   ```

2. Configure the database connection in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=WebhookDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
   }
   ```

3. Apply database migrations:
   ```sh
   dotnet ef database update
   ```

4. Run the application:
   ```sh
   dotnet run
   ```

5. Start `ngrok` to expose the local API:
   ```sh
   ngrok http 5000
   ```
   This will generate a public URL that can be used to receive webhooks.

## API Endpoints

### 1. Receive Alert Webhook
**Endpoint:** `POST /api/home/alerts`
- Accepts an alert payload and processes it.
- Sends an email notification upon successful processing.

### 2. Receive GitHub Webhook
**Endpoint:** `POST /api/home/ReceiveGithubWebHook`
- Accepts GitHub push event payloads.
- Validates webhook signature.
- Logs push event details.

## Webhook Security
Webhook requests are validated using HMAC SHA-256 signatures. Ensure that the `GitHubSecret` and other webhook secret keys are correctly configured in `appsettings.json`.

## Deployment
For production deployment:
- Use a cloud-hosted SQL Server.
- Deploy the API to a cloud provider (e.g., Azure App Service, AWS, or DigitalOcean).
- Set up webhook endpoints with HTTPS.

## License
This project is licensed under the MIT License.

---

For any issues or feature requests, please create an issue in the repository!

