-- Seed data for ContentChunks (Ask AI retrieval content).
-- PLACEHOLDER TEXT -- replace every value below with your real bio/skills/experience
-- before going live. Section naming convention matches RagService's `sources` citation
-- (see Docs/03-Low-Level-Design.md, section 3.2 / 4).
--
-- Run against PortfolioDb in SSMS (or via `sqlcmd -S "(localdb)\mssqllocaldb" -d PortfolioDb -i SeedContentChunks.sql`).

USE PortfolioDb;
GO

INSERT INTO ContentChunks (Section, Text, DisplayOrder, UpdatedUtc)
VALUES
('About',
 'I am a software developer with 3 years of professional experience building web applications on the .NET stack. My day-to-day work centers on C#, ASP.NET Core, SQL Server, and Angular, and I enjoy working across the full stack -- from designing a database schema in SSMS to shipping the UI that consumes it. I care about clean architecture, readable code, and building things that are easy for the next developer to pick up.',
 10, SYSUTCDATETIME()),

('Skills',
 'Core skills: C#, ASP.NET Core Web API, Entity Framework Core, SQL Server (schema design, stored procedures, query tuning via SSMS), Angular, TypeScript, RESTful API design, Git. Comfortable with the full request lifecycle: relational data modeling, backend services, and building the Angular frontend that talks to them.',
 20, SYSUTCDATETIME()),

('Experience:Overview',
 'Three years of hands-on experience as a .NET developer, working on web applications that pair an ASP.NET Core backend with a SQL Server database and an Angular frontend. Responsibilities have included designing database schemas, building and consuming REST APIs, implementing business logic in C#, and building responsive UI components in Angular.',
 30, SYSUTCDATETIME()),

('Projects',
 'This portfolio site itself is a project worth asking about: a VS Code-themed personal site built with Angular and Tailwind CSS on the frontend, backed by a custom ASP.NET Core Web API and SQL Server database. It includes a retrieval-augmented "Ask AI" assistant that answers visitor questions using only this site''s own content, with the retrieval and LLM call handled entirely server-side so no API keys are ever exposed to the browser.',
 40, SYSUTCDATETIME()),

('Education',
 'Holds a degree relevant to software development, with a foundation in computer science fundamentals that supports day-to-day work in data structures, databases, and object-oriented design.',
 50, SYSUTCDATETIME()),

('Contact',
 'Open to new opportunities and collaborations. The best way to get in touch is through the Contact page on this site, which sends a message directly.',
 60, SYSUTCDATETIME());
GO
