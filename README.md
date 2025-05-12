
# SocialMediaAPI

## Contents
- [Introduction](#introduction)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [API Endpoints](#api-endpoints)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

### Introduction
SocialMediaAPI is a backend service designed to provide APIs for a social media application. It enables users to interact with features such as user authentication, posts, comments, likes, and more.

---

### Features
- User registration and authentication
- Create, read, update, and delete (CRUD) operations for posts
- Support for comments and likes
- API documentation for easy integration
- Scalable and secure design principles

---

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/michaelomopekun/SocialMediaAPI.git
   ```
2. Navigate to the project directory:
   ```bash
   cd SocialMediaAPI
   ```
3. Install dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```

---

### Usage
1. Run the project:
   ```bash
   dotnet run
   ```
2. Access the API documentation (e.g., Swagger UI) at:
   - Locally: 
     ```
     http://localhost:5149/swagger
     ```
   - Production: 
     [Swagger API Documentation](https://socialmediaapi-production-74e1.up.railway.app/swagger/index.html)

---

### API Endpoints
| Endpoint                | Method | Description               |
|-------------------------|--------|---------------------------|
| `/api/posts`            | POST   | Create a new post         |
| `/api/posts/{id}`       | DELETE | Delete a post by ID       |
| `/api/posts/{id}/like`  | POST   | Like a post               |

_(Add more endpoints as necessary based on your API)_

---

### Technologies Used
- **Backend**: C# (.NET Core)
- **Containerization**: Docker (optional)

---

### Contributing
Contributions are welcome! Follow these steps:
1. Fork the repository.
2. Create a new branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add your message here"
   ```
4. Push to your branch:
   ```bash
   git push origin feature-name
   ```
5. Create a pull request.

---

### Contact
For questions or inquiries, contact [michaelomopekun](https://www.linkedin.com/in/michael-omopekun-6308b6281/).

---
