# BaseBridge

BaseBridge is a project consisting of a .NET backend and a SvelteKit frontend. It provides a bridge for various AI providers (Gemini, Groq, DeepSeek, OpenAI) and database management features.

## Project Structure

- `BaseBridge/`: The .NET backend API.
- `BaseBridge-Frontend/`: The SvelteKit frontend application.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or later)
- [npm](https://www.npmjs.com/)

## Getting Started

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd BaseBridge/BaseBridge
   ```
2. Create a local configuration file:
   - Copy `appsettings.json` to `appsettings.Development.json`.
   - Fill in your API keys in `appsettings.Development.json`.
3. (Optional) Create a `.env` file based on `.env.example` if you prefer environment variables.
4. Run the backend:
   ```bash
   dotnet run
   ```

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd BaseBridge-Frontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Run the development server:
   ```bash
   npm run dev
   ```

## License

This project is licensed under the MIT License - see the LICENSE file for details.
