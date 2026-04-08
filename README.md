# SmartDesk AI — FAQ System
A full-stack AI powered FAQ chatbot built for Ekara Digital .

## Tech Stack
Backend: ASP.NET Core 8, C#
Frontend: Angular 20
AI: Hugging Face Inference API (`facebook/blenderbot-400M-distill`)

## Project Structure

SmartDeskAI/
├── SmartDeskAPI/               # Backend solution
│   ├── SmartDeskAPI/
│   │   ├── Controllers/        # API endpoints
│   │   ├── Services/           # Business logic (AI, Sentiment, Session, Knowledge)
│   │   ├── Strategies/         # Strategy pattern (AI / Fallback)
│   │   ├── Interfaces/         # IAiAdapter, IResponseStrategy
│   │   ├── Models/             # Request/Response models
│   │   ├── Utils/              # Response schema validator
│   │   └── Data/               # knowledge-base.json
│   └── .env                    # API keys (see setup below)
└── SmartDesk-ui/               # Angular frontend

## Setup

### 1. Clone the repo

git clone https://github.com/sahannadiranga-source/SmartDeskAI.git
cd SmartDeskAI

### 2. Configure environment variables

The '.env' file is must be located at SmartDeskAPI/.env

env

HUGGINGFACE_API_KEY=your-huggingface-api-key-here
HUGGINGFACE_MODEL=facebook/blenderbot-400M-distill
ALLOWED_ORIGIN=http://localhost:4200


### 3. Run the backend

```bash
cd SmartDeskAPI/SmartDeskAPI
dotnet run
```
API runs at: `http://localhost:5018`
Swagger UI: `http://localhost:5018/swagger`

### 4. Run the frontend

```bash
cd SmartDesk-ui
npm install
npm start
```
Frontend runs at: `http://localhost:4200`


## API Endpoints

| POST | `/api/chat` | Send a message, get AI response |
| DELETE | `/api/chat/session/{sessionId}` | Reset a session |

### POST `/api/chat` — Request body

```json
{
  "sessionId": "",
  "message": "What services does Ekara offer?"
}
```

### Response

```json
{
  "sessionId": "abc-123",
  "answer": "We specialize in custom Software Development...",
  "sentiment": 0.0,
  "priority_Escalation": false,
  "history": []
}


## Features

AI-first responses via Hugging Face, with keyword matching as fallback
Sentiment scoring from -1.0 (very negative) to 1.0 (very positive)
Priority escalation triggered when sentiment < -0.6
Session memory — stores last 3 messages per session
Strategy Pattern for response handling
Adapter Pattern for AI service integration
Response schema validation
CORS configured for Angular frontend
Angular chat UI with sentiment badges and escalation status
