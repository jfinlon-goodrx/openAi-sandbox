# Glossary

Technical terms and concepts explained in accessible language for technical professionals who may not be developers.

## AI & Machine Learning Terms

### API (Application Programming Interface)
A way for different software applications to communicate with each other. Think of it as a menu at a restaurant - you order (make a request) and get food (receive a response).

### Embedding
A mathematical representation of text that captures its meaning. Similar words or concepts have similar embeddings, allowing computers to understand relationships between text. For example, "dog" and "puppy" would have similar embeddings.

### GPT (Generative Pre-trained Transformer)
A type of AI model that can generate human-like text, answer questions, write code, and perform various language tasks. GPT-4 is the latest and most capable version.

### Model
A trained AI system that can perform specific tasks. Different models are optimized for different purposes (e.g., GPT-4 for text, Whisper for speech-to-text).

### Prompt
The input text you provide to an AI model to get a response. Like asking a question or giving instructions to a colleague.

### RAG (Retrieval-Augmented Generation)
A technique that combines document search with AI generation. Instead of sending entire documents to AI, it finds relevant sections first, then generates answers based on those sections. This reduces costs and improves accuracy.

### Token
A unit of text that AI models process. Roughly 1 token = 4 characters or 0.75 words. "Hello world" = ~2 tokens. Token usage determines API costs.

### Vector Database
A specialized database that stores embeddings and allows fast similarity searches. Used in RAG workflows to find relevant documents quickly.

## Development Terms

### .NET
A software framework developed by Microsoft for building applications. The projects in this portfolio use .NET 8.0.

### API Endpoint
A specific URL where you can send requests to get information or perform actions. For example, `/api/requirements/summarize` is an endpoint for summarizing documents.

### Build
The process of compiling source code into an executable program that can run.

### CLI (Command Line Interface)
A text-based way to interact with your computer using commands instead of clicking buttons. The terminal or command prompt.

### Code Review
The process of examining code changes before they're merged, looking for bugs, security issues, and improvements.

### Configuration File
A file (like `appsettings.json`) that stores settings for an application, such as API keys, connection strings, and feature flags.

### Dependency Injection
A design pattern where components receive their dependencies from an external source rather than creating them internally. Makes code more flexible and testable.

### Endpoint
See "API Endpoint"

### Framework
A collection of pre-built code and tools that provides a foundation for building applications. .NET is a framework.

### Git
A version control system that tracks changes to files and allows collaboration on code projects.

### GitHub
A web-based platform for hosting Git repositories and collaborating on code projects.

### HTTP Request
A message sent from a client (like a web browser or API client) to a server asking for information or to perform an action.

### JSON (JavaScript Object Notation)
A lightweight data format used for storing and exchanging data. Easy for both humans and machines to read and write.

### Middleware
Software components that process requests and responses in a web application. Like a series of checkpoints that requests pass through.

### Package
A pre-built library of code that provides specific functionality. Packages are downloaded and added to projects to avoid writing code from scratch.

### REST API
A way of designing web services where you use standard HTTP methods (GET, POST, PUT, DELETE) to interact with resources. Most modern APIs follow REST principles.

### SDK (Software Development Kit)
A collection of tools, libraries, and documentation for building applications for a specific platform or service.

### Swagger
A tool that automatically generates interactive documentation for APIs. Shows available endpoints, parameters, and allows testing directly in a web browser.

### Webhook
A way for one application to notify another application when something happens. Like a callback URL that gets triggered by events.

## Integration Terms

### Confluence
Atlassian's collaboration and documentation platform. Used for storing requirements, meeting notes, and project documentation.

### Jira
Atlassian's project management and issue tracking tool. Used for managing tasks, bugs, and project workflows.

### Slack
A team communication platform. Used for notifications, alerts, and team updates in this portfolio.

### GitHub Actions
GitHub's automation platform that runs workflows (like tests, deployments) automatically when code changes.

## OpenAI Platform Terms

### Batch Processing
Processing multiple requests together at a lower cost (50% reduction). Useful for high-volume operations that don't need immediate responses.

### Chat Completion
An API call where you send messages (like a conversation) and receive a text response from the AI model.

### DALL-E
OpenAI's image generation model. Can create images from text descriptions.

### Function Calling
A feature that allows AI models to request structured data or trigger specific functions, making it easier to integrate AI into applications.

### JSON Mode
A feature that forces AI models to return responses in valid JSON format, making it easier to parse and use programmatically.

### Moderation API
An OpenAI service that checks content for safety, identifying potentially harmful or inappropriate content.

### Rate Limit
A restriction on how many API requests you can make in a certain time period. Prevents abuse and ensures fair usage.

### Streaming
Receiving responses in real-time as they're generated, rather than waiting for the complete response. Provides better user experience for long responses.

### Vision API
OpenAI's image analysis capability. Can describe images, extract text, and answer questions about visual content.

### Whisper
OpenAI's speech-to-text model. Converts audio recordings into text transcripts.

## Technical Concepts

### Authentication
The process of verifying who you are. API keys and JWT tokens are forms of authentication.

### Authorization
The process of determining what you're allowed to do after you've been authenticated.

### Caching
Storing frequently accessed data in memory for faster retrieval. Reduces API calls and improves performance.

### Chunking
Breaking large documents into smaller pieces for processing. Used in RAG to handle large documents efficiently.

### Circuit Breaker
A pattern that prevents cascading failures by stopping requests to a failing service temporarily, giving it time to recover.

### Correlation ID
A unique identifier attached to requests that allows tracking a request across multiple services and systems. Essential for debugging distributed systems.

### CORS (Cross-Origin Resource Sharing)
A security feature that allows web applications running on one domain to access resources from another domain. Required for web apps to call APIs.

### Health Check
An endpoint that reports whether a service is running and healthy. Used by monitoring systems and load balancers.

### Logging
Recording events and information about application behavior. Helps with debugging and monitoring.

### Metrics
Quantitative measurements about system performance, such as request counts, response times, and error rates.

### Rate Limiting
Controlling how many requests can be made in a time period to prevent overload and ensure fair usage.

### Retry Logic
Automatically retrying failed requests with exponential backoff (waiting longer between each retry). Handles temporary failures gracefully.

### Semantic Search
Searching for information based on meaning rather than exact word matches. Uses embeddings to find conceptually similar content.

### Structured Logging
Logging in a format (like JSON) that makes it easy for machines to parse and analyze. Better than plain text logs.

### Token Bucket
An algorithm for rate limiting that allows bursts of requests up to a limit, then enforces a steady rate.

## Workflow Terms

### CI/CD (Continuous Integration/Continuous Deployment)
Automated processes that test code changes and deploy them to production environments.

### Sprint
A time-boxed period (usually 2-4 weeks) in agile development where a team works on a set of features.

### User Story
A description of a feature from the user's perspective, typically in the format: "As a [user type], I want [goal], so that [benefit]."

### Velocity
A measure of how much work a team completes in a sprint, used for planning future sprints.

## Cost & Performance Terms

### Cost per Token
The price charged by OpenAI for processing tokens. Different models have different costs.

### Latency
The time it takes for a request to complete. Lower latency means faster responses.

### Throughput
The number of requests a system can handle per unit of time. Higher throughput means more capacity.

### Token Budget
A limit on token usage to control costs. Important for managing API expenses.

## Common Acronyms

- **API**: Application Programming Interface
- **BA**: Business Analyst
- **CI/CD**: Continuous Integration/Continuous Deployment
- **CLI**: Command Line Interface
- **CORS**: Cross-Origin Resource Sharing
- **GPT**: Generative Pre-trained Transformer
- **HTTP**: Hypertext Transfer Protocol
- **JSON**: JavaScript Object Notation
- **JWT**: JSON Web Token
- **PM**: Product Manager
- **RAG**: Retrieval-Augmented Generation
- **REST**: Representational State Transfer
- **SDK**: Software Development Kit
- **SDM**: Software Development Manager
- **SSE**: Server-Sent Events
- **UI**: User Interface

## Understanding the Project Structure

### Solution (.sln file)
A container that organizes multiple related projects. Like a folder that holds all the pieces of a larger application.

### Project (.csproj file)
A single application or library. Each project has its own purpose (e.g., API, core logic, tests).

### Namespace
A way to organize code and avoid naming conflicts. Like folders for code.

### Class
A blueprint for creating objects in object-oriented programming. Defines properties and behaviors.

### Method/Function
A block of code that performs a specific task. Can be called to execute that task.

### Service
A class that provides specific functionality to other parts of the application. Follows the "service pattern" for organization.

## How to Use This Glossary

- **New to AI?** Start with AI & Machine Learning Terms
- **New to development?** Read Development Terms
- **Setting up integrations?** Check Integration Terms
- **Understanding costs?** See Cost & Performance Terms
- **Working with workflows?** Review Workflow Terms

If you encounter a term not listed here, check the specific documentation for that feature or concept.
