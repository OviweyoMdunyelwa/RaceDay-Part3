## API Endpoint Plan

The following API endpoint plan outlines the proposed RESTful API for the RaceDay event management system. The endpoints are designed around the main system resources, including users, events, categories, enrolments, results, routes and weather information. Each endpoint identifies the HTTP method, route, responsible user role, purpose, request information and expected response codes. This plan provides a clear structure for how the frontend and backend components will communicate and how different users will interact with the RaceDay system.

# RaceDay API Endpoint Plan

## 1. User Management

| Method | Endpoint          | Role                        | Description                      | Request Body         | Response                               |
| ------ | ----------------- | --------------------------- | -------------------------------- | -------------------- | -------------------------------------- |
| GET    | `/api/users`      | Administrator               | Retrieves all registered users.  | None                 | 200 OK, 404 Not Found                  |
| GET    | `/api/users/{id}` | Administrator / Participant | Retrieves a specific user by ID. | None                 | 200 OK, 404 Not Found                  |
| POST   | `/api/users`      | Administrator               | Creates a new user account.      | User details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/users/{id}` | Administrator               | Updates an existing user.        | Updated user details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/users/{id}` | Administrator               | Removes a user from the system.  | None                 | 204 No Content, 404 Not Found          |

## 2. Events

| Method | Endpoint           | Role                    | Description                             | Request Body          | Response                               |
| ------ | ------------------ | ----------------------- | --------------------------------------- | --------------------- | -------------------------------------- |
| GET    | `/api/events`      | Participant / Organiser | Retrieves all available events.         | None                  | 200 OK                                 |
| GET    | `/api/events/{id}` | Participant / Organiser | Retrieves details for a specific event. | None                  | 200 OK, 404 Not Found                  |
| POST   | `/api/events`      | Organiser               | Creates a new event.                    | Event details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/events/{id}` | Organiser               | Updates an existing event.              | Updated event details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/events/{id}` | Organiser               | Deletes an event.                       | None                  | 204 No Content, 404 Not Found          |

## 3. Categories

| Method | Endpoint               | Role                    | Description                      | Request Body             | Response                               |
| ------ | ---------------------- | ----------------------- | -------------------------------- | ------------------------ | -------------------------------------- |
| GET    | `/api/categories`      | Participant / Organiser | Retrieves all event categories.  | None                     | 200 OK                                 |
| GET    | `/api/categories/{id}` | Participant / Organiser | Retrieves a specific category.   | None                     | 200 OK, 404 Not Found                  |
| POST   | `/api/categories`      | Organiser               | Creates a category for an event. | Category details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/categories/{id}` | Organiser               | Updates a category.              | Updated category details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/categories/{id}` | Organiser               | Removes a category.              | None                     | 204 No Content, 404 Not Found          |

## 4. Enrolments

| Method | Endpoint               | Role                      | Description                                    | Request Body              | Response                               |
| ------ | ---------------------- | ------------------------- | ---------------------------------------------- | ------------------------- | -------------------------------------- |
| GET    | `/api/enrolments`      | Organiser / Administrator | Retrieves participant enrolments.              | None                      | 200 OK                                 |
| GET    | `/api/enrolments/{id}` | Participant / Organiser   | Retrieves a specific enrolment.                | None                      | 200 OK, 404 Not Found                  |
| POST   | `/api/enrolments`      | Participant               | Registers a participant for an event category. | Enrolment details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/enrolments/{id}` | Organiser                 | Updates an enrolment.                          | Updated enrolment details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/enrolments/{id}` | Participant / Organiser   | Cancels an enrolment.                          | None                      | 204 No Content, 404 Not Found          |

## 5. Results

| Method | Endpoint            | Role                    | Description                          | Request Body           | Response                               |
| ------ | ------------------- | ----------------------- | ------------------------------------ | ---------------------- | -------------------------------------- |
| GET    | `/api/results`      | Participant / Organiser | Retrieves race results.              | None                   | 200 OK                                 |
| GET    | `/api/results/{id}` | Participant / Organiser | Retrieves a specific result.         | None                   | 200 OK, 404 Not Found                  |
| POST   | `/api/results`      | Organiser               | Records a participant's race result. | Result details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/results/{id}` | Organiser               | Updates an existing result.          | Updated result details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/results/{id}` | Organiser               | Removes an incorrect result.         | None                   | 204 No Content, 404 Not Found          |

## 6. Routes

| Method | Endpoint           | Role                    | Description                       | Request Body          | Response                               |
| ------ | ------------------ | ----------------------- | --------------------------------- | --------------------- | -------------------------------------- |
| GET    | `/api/routes`      | Participant / Organiser | Retrieves available event routes. | None                  | 200 OK                                 |
| GET    | `/api/routes/{id}` | Participant / Organiser | Retrieves a specific route.       | None                  | 200 OK, 404 Not Found                  |
| POST   | `/api/routes`      | Organiser               | Adds a route to an event.         | Route details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/routes/{id}` | Organiser               | Updates route information.        | Updated route details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/routes/{id}` | Organiser               | Removes a route.                  | None                  | 204 No Content, 404 Not Found          |

## 7. Weather

| Method | Endpoint            | Role                    | Description                                         | Request Body            | Response                               |
| ------ | ------------------- | ----------------------- | --------------------------------------------------- | ----------------------- | -------------------------------------- |
| GET    | `/api/weather`      | Participant / Organiser | Retrieves weather information for events.           | None                    | 200 OK                                 |
| GET    | `/api/weather/{id}` | Participant / Organiser | Retrieves weather information for a specific event. | None                    | 200 OK, 404 Not Found                  |
| POST   | `/api/weather`      | Organiser               | Adds weather information for an event.              | Weather details         | 201 Created, 400 Bad Request           |
| PUT    | `/api/weather/{id}` | Organiser               | Updates weather information.                        | Updated weather details | 200 OK, 400 Bad Request, 404 Not Found |
| DELETE | `/api/weather/{id}` | Organiser               | Removes weather information.                        | None                    | 204 No Content, 404 Not Found          |

## Common Response Codes

* **200 OK** – Request completed successfully.
* **201 Created** – A new resource was successfully created.
* **204 No Content** – Resource was successfully deleted.
* **400 Bad Request** – Request contains invalid or incomplete data.
* **401 Unauthorized** – Authentication is required.
* **403 Forbidden** – User does not have permission to perform the operation.
* **404 Not Found** – Requested resource does not exist.
* **500 Internal Server Error** – Unexpected server-side error.

## API Design Notes

The RaceDay API follows a REST-style approach and uses HTTP methods according to the operation being performed. Resources are separated according to the main database entities, allowing the API to support user management, event management, categories, enrolments, results, routes and weather information. Role-based access is included in the endpoint plan to ensure that participants, event organisers and administrators only perform operations appropriate to their responsibilities.



