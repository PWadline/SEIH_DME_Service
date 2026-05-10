To start the project 
This project is build on clean architecture
1) Change the connection string on Infrastructure/DependencyInjection.cs with your database connection
2) Open your terminal to the Aurabe-POS folder
   **Create Migration**
   paste the below command
   dotnet ef migrations add "migrationName" -p .\src\Infrastructure\ -s .\src\WebAPI\
   **Update the database**
   dotnet ef database update -p .\src\Infrastructure\ -s .\src\WebAPI\

3) You can run the project
