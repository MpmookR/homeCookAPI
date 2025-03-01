HomeCookAPI : A Recipe Sharing platform for all homecook

HomeCookAPI is a RESTful API that allows users to:
👩🏽‍🍳 Create, update, and delete recipes 
👍🏼 Like and save recipes 
⭐️ Rate and comment on recipes 
🔐 Manage user authentication and roles 

This project follows a clean architecture approach by 
separating business logic, data access, and API layers for maintainability and scalability

Features
- User Authentication (Register, Login, Logout, Email Verification)
- JWT Authentication & Role-Based Authorization
- CRUD Operations for Recipes
- Commenting, Liking, and Rating Recipes
- User Role Management (SuperAdmin, Admin, User)
- DTO-Based Data Transfer (No Direct Entity Exposure)
- ILogger Integration for Debugging
- Swagger API Documentation for Easy Testing

Installation & Setup
1. Clone repository
  git clone https://github.com/MpmookR/homeCookAPI.git
  cd homeCookAPI

2. make sure you have .NET 9.0 installed
   dotnet restore

3. Configure the Database
   dotnet ef database update
   
4. Run API
   dotnet run
   API will be available at: http://localhost:5057

5. API document to find the endpoint:
   http://localhost:5057/api-docs

