# BaseBridge

A personal project that ties together a .NET backend with a SvelteKit frontend. The idea behind it is pretty simple — a unified interface for chatting with different AI providers (Gemini, Groq, DeepSeek, OpenAI) and managing some database stuff on the side.

## What's inside

```
BaseBridge/             → .NET 9 backend (REST API, SQLite, auth)
BaseBridge-Frontend/    → SvelteKit frontend
```

The backend handles routing requests to whichever AI provider you configure, manages users, and exposes a dynamic API layer. The frontend connects to it and gives you a clean UI to interact with everything.

## Stack

- **Backend**: .NET 9, Entity Framework Core, SQLite
- **Frontend**: SvelteKit, TypeScript
- **AI Providers**: Gemini, Groq, DeepSeek, OpenAI

## Getting started

### Backend

```bash
cd BaseBridge/BaseBridge
```

Copy `appsettings.json` to `appsettings.Development.json` and fill in your API keys. Then just:

```bash
dotnet run
```

### Frontend

```bash
cd BaseBridge-Frontend
npm install
npm run dev
```

That's it. The dev server will start and connect to the backend automatically.
