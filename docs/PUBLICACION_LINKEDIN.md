# StockFlow — Kit de publicación para LinkedIn

Material basado en las características documentadas de StockFlow. **Este archivo es el borrador para publicar; no significa que ya esté publicado en LinkedIn.**

## Publicación recomendada (English)

🚀 **From inventory workflows to a deployed Full-Stack .NET application: meet StockFlow.**

I developed **StockFlow**, a multi-tenant business management platform that helps businesses organize their inventory, sales, purchases, customers and operational reports.

I wanted to go beyond simple CRUD operations and build something closer to real-world business workflows.

🔧 **Tech stack:** C# · .NET 10 · ASP.NET Core MVC · Web API · Dapper · SQL Server · Microsoft Azure · JWT · Chart.js · QuestPDF

What I implemented:
- **Company-level data isolation:** each business operates within its own tenant.
- **Inventory integrity:** purchases, sales and adjustments update stock through transactional database operations.
- **Authentication and roles:** JWT, BCrypt and Administrator / Employee / Read-only permissions.
- **Operational visibility:** sales, purchasing, gross profit, stock alerts, charts and downloadable PDF reports.
- **Cloud architecture:** MVC front end and REST API deployed as separate Azure App Services with Azure SQL.

My biggest learning: designing the database, authorization, and transaction boundaries together matters as much as building the UI.

🌐 **Live portfolio demo:** https://stockflow-web-hga-frgsfkb5emedg4e2.centralus-01.azurewebsites.net/

💻 **Code, architecture and screenshots:** https://github.com/HenryGonzalezAlguera/StockFlow

The demo uses free-tier cloud resources, so the first request may be slow after inactivity. Please use only fictitious information when testing.

I'm continuing to grow as a Software Developer and welcome technical feedback.

#DotNet #CSharp #SoftwareDevelopment #SQLServer #MicrosoftAzure

## Versión opcional en español

🚀 **Comparto StockFlow**, mi plataforma full-stack para administrar inventario, compras, ventas y reportes empresariales.

Construí la solución con **C# / .NET 10, ASP.NET Core MVC + Web API, Dapper, SQL Server y Azure**. Implementé separación de empresas (multi-tenant), autenticación JWT y roles, operaciones transaccionales de inventario, indicadores y reportes PDF.

Aprendí que la consistencia de datos y los controles de autorización son esenciales, incluso en un proyecto de portafolio.

🌐 Demo: https://stockflow-web-hga-frgsfkb5emedg4e2.centralus-01.azurewebsites.net/

💻 Código y arquitectura: https://github.com/HenryGonzalezAlguera/StockFlow

La demo utiliza recursos gratuitos y puede tardar en reactivarse. Utiliza datos ficticios.

#DotNet #CSharp #SQLServer #Azure #SoftwareDevelopment

## Material para adjuntar (real, ya existe)

| Orden | Archivo | Descripción alternativa (accesibilidad) |
| --- | --- | --- |
| 1 | [Dashboard](screenshots/dashboard.png) | Dashboard real de StockFlow con indicadores de negocio y gráficos de operaciones |
| 2 | [Inicio de sesión](screenshots/login.png) | Pantalla de inicio de sesión de StockFlow para acceso de empresas |

Antes de publicar:
1. Abrir el [dashboard](screenshots/dashboard.png) y la [pantalla de acceso](screenshots/login.png); revisar que no muestren datos personales ni información confidencial.
2. Abrir la demo desde una ventana privada y comprobar que responde. **No se verificó disponibilidad en tiempo real al preparar este documento.**
3. En LinkedIn: **Crear publicación → adjuntar imágenes reales → pegar el texto EN o ES → vista previa → publicar**.
4. Si la demo está temporalmente inactiva, priorizar el enlace al repositorio.
5. No describirlo como SaaS comercial en producción: es una **demo de portafolio con hosting gratuito y sin SLA**.

## Destacados y sección Proyectos

**Título:** StockFlow | Full-Stack .NET & Azure Business Management

**Descripción corta:** Multi-tenant inventory, purchasing and sales application with ASP.NET Core MVC, REST API, Dapper, SQL Server, JWT, Azure and PDF reporting.

**Habilidades:** C#, ASP.NET Core, SQL Server, Microsoft Azure, REST APIs.

**URL principal:** https://github.com/HenryGonzalezAlguera/StockFlow

**URL de demo:** https://stockflow-web-hga-frgsfkb5emedg4e2.centralus-01.azurewebsites.net/
