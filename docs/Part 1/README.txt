# RaceDay – Part 1: Database Design and Implementation

## 1. Project Overview

RaceDay is a database-driven event management system designed to support road running, walking and cycling events. The system stores and manages information relating to users, events, categories, participant enrolments, race results, routes and weather conditions.

The purpose of Part 1 is to design and implement the database structure required to support the RaceDay application.

## 2. Database Information

**Database name:** `RaceDay_PoE_Final`

**Database platform:** Microsoft SQL Server

The database provides structured storage for the information required by the RaceDay system.

## 3. Database Tables

The database contains the following main tables:

| Table | Description |
|---|---|
| RaceDayUsers | Stores participant and organiser information. |
| Events | Stores event details, including dates, locations and event types. |
| Categories | Stores the different participation categories available for events. |
| Enrolments | Records participants registered for events. |
| Results | Stores participant race results and finishing positions. |
| Routes | Stores information about event routes. |
| Weather | Stores weather information associated with events. |

## 4. Database Design

The database design is represented by the Entity Relationship Diagram (ERD) included in this folder.

The ERD illustrates the database entities, their attributes, primary keys, foreign keys and relationships. It provides an overview of how information is organised and connected within the RaceDay database.

## 5. SQL Implementation

The database was implemented using SQL Server and SQL scripts.

The SQL scripts are included in the `SQL_Queries` folder in the main repository. They cover the database creation, table structures, relationships and data insertion.

The scripts should be executed in their intended order when setting up the database.

## 6. API Endpoint Plan

The API Endpoint Plan is included in this folder as supporting project documentation.

It outlines the planned API operations required to access and manage RaceDay information, supporting the development of the application in the subsequent project parts.

## 7. Technologies Used

- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- SQL (Structured Query Language)
- Entity Relationship Diagrams (ERD)

## 8. Part 1 Deliverables

The Part 1 submission includes:

- Database implementation.
- SQL scripts.
- Entity Relationship Diagram.
- API Endpoint Plan.
- Database structure and relationships.

**Part 1 establishes the database foundation for the RaceDay application, which is further developed through the API and MVC implementation in Parts 2 and 3.**