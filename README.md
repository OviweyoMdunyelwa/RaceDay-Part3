# RaceDay-Part3
Portfolio of Evidance 
# RaceDay – Event Management System

## Project Overview

RaceDay is a web-based event management system developed to support the organisation and management of running, walking and cycling events. The system provides separate functionality for organisers and participants, allowing event organisers to manage sporting events while participants can register, explore available events and manage their participation. The project was developed in three parts, progressing from database design and implementation to API development and the final MVC web application.

## Part 1 – Database Design and Implementation

The first part of the project focused on designing and implementing the RaceDay relational database using Microsoft SQL Server. The database is named RaceDay_PoE_Final and contains the main entities required to support the system, including users, events, categories, enrolments, results, routes and weather information. An Entity Relationship Diagram (ERD) was created to illustrate the relationships between these entities. SQL scripts were developed to create and populate the database, while additional queries were used to retrieve and verify related information.

## Part 2 – Web API Development

The second part involved developing the RaceDay Web API using C# and ASP.NET Core Web API on .NET 8. Entity Framework Core was used to connect the application to the SQL Server database. The API provides functionality for user registration and authentication, profile management, event management, categories, participant enrolments, results and routes. Role-based access distinguishes between Organisers and Participants, ensuring that users can access functionality according to their assigned roles. Swagger was used to document and test the available API endpoints.

## Part 3 – MVC Web Application and Deployment

The third part focused on developing the RaceDay MVC web application, which communicates with the Web API to provide an accessible interface for organisers and participants. The application provides separate interfaces for managing events and participating in available events. Azure Blob Storage was incorporated for cloud-based file storage. Docker was used to containerise the application, making it easier to package and run in a consistent environment.

## Technologies Used

The project uses C# and ASP.NET Core for application development, .NET 8 for the application framework, Entity Framework Core for database access, Microsoft SQL Server for relational data storage, and HTML and CSS for the web interface. Swagger supports API documentation and testing, while Azure Blob Storage provides cloud file storage. Docker supports containerisation, and GitHub is used for source-code management and project version control.

## Database Information

The final database is named RaceDay_PoE_Final and was implemented using Microsoft SQL Server. It contains the RaceDayUsers, Events, Categories, Enrolments, Results, Routes and Weather tables. The database scripts and supporting design documentation are included in this repository.

## Running the Project

To run the application, the required .NET 8 SDK and Microsoft SQL Server environment must be available. The database connection string must be configured for the local SQL Server instance, and the API and MVC projects must be configured to communicate with the correct addresses. Docker instructions will be provided once the final container configuration and required environment settings have been confirmed.

## Project Structure

The repository contains the RaceDay.API project, which provides the backend services, and the RaceDay.MVC project, which provides the web application. The SQL_Queries directory contains the database scripts, while the documentation directory contains supporting project evidence and documentation.

## Conclusion

RaceDay demonstrates the development of a database-driven event management system across database implementation, API development and MVC application development. The project brings these components together to provide a platform for managing sporting events and supporting participant registration and event administration.



<img width="1366" height="768" alt="Screenshot 2026-11-09 205407" src="https://github.com/user-attachments/assets/eb40a754-9fc2-42f1-a5f0-0a702708fbd9" />

