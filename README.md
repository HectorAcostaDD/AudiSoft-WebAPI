# WebAPI - Sistema de Gestión Académica

## Introducción
Esta es una API robusta desarrollada con .NET como prueba tecnica para la gestión de estudiantes, profesores y calificaciones. La aplicación proporciona un sistema de autenticación seguro mediante JWT (JSON Web Tokens) y permite la integración con aplicaciones frontend (como Angular) gracias a su configuración flexible de CORS.

### Características principales:
- **Gestión de Entidades:** CRUD completo para Estudiantes, Profesores y Calificaciones.
- **Seguridad:** Autenticación y autorización basada en JWT.
- **Documentación:** Integración con Swagger/OpenAPI para facilitar las pruebas y visualización de endpoints.
- **Base de Datos:** Uso de Entity Framework Core con soporte para SQL Server.
- **Manejo de Errores:** Middleware personalizado para respuestas de error estandarizadas.

## Configuración

La configuración principal del proyecto se encuentra en el archivo `WebAPI/appsettings.json`.

### 1. Configuración de Base de Datos
Asegúrese de configurar correctamente la cadena de conexión en la sección `ConnectionStrings` según su instancia local de SQL Server:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PruebaAudisoftDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 2. Configuración de CORS
Para permitir que aplicaciones externas (como un cliente Angular) consuman esta API, debe configurar la URL de su cliente en la llave `CorsSettings`:
```json
"CorsSettings": {
  "AllowedOrigins": "http://localhost:4200"
}
```
> **Nota:** El valor actual permite peticiones desde `http://localhost:4200`. Si su cliente corre en otro puerto o dominio, actualice este valor.

### 3. Ajustes de JWT
En la sección `JwtSettings` se definen los parámetros de seguridad para la generación de tokens:
- `Secret`: Clave secreta para la firma (mínimo 32 caracteres recomendados).
- `Issuer`: Identificador del emisor del token.
- `Audience`: Identificador del destinatario del token.

## Requisitos Previos
- [.NET 8 SDK](https://dotnet.microsoft.com/download) o superior.
- [SQL Server](https://www.microsoft.com/sql-server/) (Express o LocalDB).

## Instrucciones para Iniciar el Servicio

Siga estos pasos para poner en marcha la aplicación:

### Paso 1: Restaurar Dependencias
Abra una terminal en la carpeta raíz de la solución y ejecute:
```bash
dotnet restore
```

### Paso 2: Actualizar la Base de Datos
Para crear la base de datos y las tablas necesarias a partir de las migraciones existentes, ejecute:
```bash
dotnet ef database update --project WebAPI
```

### Paso 3: Ejecutar la Aplicación
Para iniciar el servicio web, utilice el siguiente comando:
```bash
dotnet run --project WebAPI
```

Una vez que el servicio esté activo, podrá acceder a la documentación interactiva de Swagger en la siguiente dirección (por defecto):
- `https://localhost:7193/swagger` o `http://localhost:5242/swagger`

---