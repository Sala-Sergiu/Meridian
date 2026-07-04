## Decisions

This document will highlight my decisions on what I decided to build, and on what I left intentionally out. And the reasoning behind both.

## Product Decisions

### Which feature did I include?

**Onboarding board (core)**
An HR-authored template is cloned into a personal board for each new hire. The board has three columns(To do, In Progress and Done) with cards for onboarding materials. Safety guidelines, resources to read, and direct contact links (team, manager, HR, IT).

**Progress tracking**
The hire can move cards or mark them as read as they go through the onboarding. HR can see how the status of the onboarding is, if they have progressed or not.

**HR template**
HR authors the onboarding content once, as a template, and assings it to each hire.

**Calendar**
Each hire can see whether a given day is remote or office, they can also see the public holidays.

**Relevant contact**
Direct link to the tools the company uses: slack, help desk channel etc. the board routes the new hires to the right place.

###How did I prioritize them?
I started by identifying a single purpose of the application: onboarding. Everything that directly serves a new hire's first month come first. I also deliberately kept the app as lean as possible. I didn't want it bloated with useless features that dont serve the core purpose.

### Which feature did I intentionally leave out?
I left out a company wide directory/list of all the employees. I deliberately did not build a place listing 200 employees. That would overlap with Slack and turn Meridian into a all in one platform compiting with tools the company already uses. Meridian should stay focused on onboarding tasks. The application is meant to fit into the organizations tools, not replace them.

## Technical decisions

### Architecture
I used a layered architecture: API/BLL/DAL/Domain with dependecies pointing inwards. Controllers are kept thin, all business logic lives in BLL. Repository interfaces live in Domain and are implemeted in DAL, so the BLL depends on abstractions rather than on EF Core directly.

To make this more than a claim I added architecture tests that asserts the layering rules.

### Database and ORM
I chose MSSQL because I have solid experience with it and because the data is naturally relational and maps cleanly to databse. I used EF core with code first appropach, migrations and seed data via 'HasData', which is idemnpotent by key, re-running never duplicates rows.

### ### Repository without Unit of Work
I used repository pattern but deliberatley did not add UoW. Ef Core's 'DbContext' already ia a unit of work, so wrapping it again would make it just a ceremony. I kept generic repository base for resue, with specific repositoryes only where they are needed.

### Query pipeline
Filtering, sorting and paging on the board are handled by a composable query pipeline. The pipeline lives in the BLL and operates on 'IQueriable<T>' using only standarrd LINQ operators.

### Authorization
Authentication uses JWT, and authorization is policy-based and
resource-aware, not a flat role check. A new hire can only write to their own
board

### Supporting libraries

- FluentValidation — validation lives in the BLL, keeping controllers thin.
- Mapster — mapping between entities and DTOs, so entities are never exposed
  across the API boundary.
- Serilog + correlation id — structured logging with a correlation id
  generated per request (and propagated from the Angular frontend), so any
  request is traceable end to end, from the browser into the server logs.
- lobal exception middleware — a single catch-all returning RFC 7807
  ProblemDetails, with the correlation id included in the error response, so a
  user-facing error can be matched to the logs.
- Scrutor — used to register a caching decorator over the template
  repository (the hot, rarely-changing read path).
  
### Frontend

Angular with signals for state, deliberately without NgRx — signals cover
the state needs at this scope without the extra ceremony. HTTP interceptors
attach the JWT and propagate the correlation id.

### If I had more time, what would I do differently?

I would invest more in UX, and I would validate it with real people — asking
colleagues to try the flow and incorporating their input, rather than relying
only on my own assumptions about what a new hire needs.

## UX decisions

### Why this user flow?

The flow is built around the two real actors: HR and the new hire. On the HR
side, things stay as smooth as possible: author a template once, assign it, with
minimal manual overrides. On the new hire's side, they can work through their
onboarding materials at their own pace, without depending on a colleague being
available. That independence is the core value.

### Did I test it with anyone? What changed after feedback?

I did not run formal user testing, and I did not receive external feedback on
this version. I'm noting that honestly: validating the flow with real users is
exactly what I'd prioritize with more time.

## Assumptions that changed during the project

Two initial assumptions I deliberately reversed, which I see as a healthy part of
the work:

- Slack as the people layer. I first assumed Slack would cover the "getting
  to know people" side, then realized Meridian's job is to link into Slack and
  the existing tools, not to replace or duplicate them.
- The profile as a core feature. I first thought the employee profile
  (hobbies, personal details) was central. Over time I concluded the onboarding
  plan itself is the core, and the profile is secondary — so it moved to future
  work.