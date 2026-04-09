# SmartDesk AI — FAQ System
A full-stack AI powered FAQ chatbot built for Ekara Digital Partners.It answers questions from a structured knowledge base using Google Gemini, with real time sentiment analysis and automatic escalation for frustrated users.

## Tech Stack
Backend: ASP.NET Core 8, C#
Frontend: Angular 20
AI: Google Gemini 2.0 Flash
Sentiment: Lexicon-based scoring

## Architecture


User Message
     │
     ▼
SentimentService        ← scores tone (-1.0 to 1.0)
     │
     ▼
AiResponseStrategy
     ├── KnowledgeService.GetFullContext()   ← builds knowledge base prompt
     └── AiChatService (Gemini API)          ← generates grounded answer
              │ (if Gemini fails)
              └── KnowledgeService.GetAnswer()  ← keyword fallback
     │
     ▼
SentimentResponseLayer  ← wraps answer with tone-aware prefix
     │
     ▼
ChatResponse  { answer, sentiment, priority_Escalation, history }


**Design patterns used:** Strategy, Adapter, Singleton, Dependency Injection



## Setup

### 1. Clone the repo

git clone https://github.com/sahannadiranga-source/SmartDeskAI.git
cd SmartDeskAI

### 2. Configure environment variables

The '.env' file is must be located at SmartDeskAPI/.env

env
GEMINI_API_KEY=your-Gemini-api-key-here
ALLOWED_ORIGIN=http://localhost:4200



### 3. Run the backend

```bash
cd SmartDeskAPI
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

AI-first responses via Gemini, with keyword matching as fallback
Sentiment scoring from -1.0 (very negative) to 1.0 (very positive)
Priority escalation triggered when sentiment < -0.6
Auto escalation - when sentiment drops below -0.6 — flags for priority support
Session memory — stores last 3 messages per session
Strategy Pattern for response handling
Adapter Pattern for AI service integration
Response schema validation
CORS configured for Angular frontend
Angular chat UI with sentiment badges and escalation status,responsive, works on mobile
Swagger UI - included for API testing

