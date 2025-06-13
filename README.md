---
# FONTUR - Sistema de Supervisión de Unidades de Transporte Público

Este repositorio contiene el código fuente de la aplicación web desarrollada por la Gerencia de Tecnología de FONTUR para la **supervisión y monitoreo de las unidades de transporte público en Venezuela**.

---

## 📋 Descripción General

La aplicación tiene como objetivo principal brindar una herramienta eficiente y centralizada para la supervisión en tiempo real de la flota de transporte público, permitiendo a FONTUR garantizar un servicio de calidad, optimizar la gestión operativa y mejorar la experiencia de los usuarios.

---

## 🚀 Características Principales

* **Monitoreo en Tiempo Real:** Visualización de la ubicación actual de las unidades de transporte.
* **Gestión de Rutas:** Registro y seguimiento de las rutas asignadas a cada unidad.
* **Reportes y Estadísticas:** Generación de informes sobre el desempeño, puntualidad e incidencias de las unidades.
* **Alertas y Notificaciones:** Sistema de alertas para eventos importantes (desviaciones de ruta, paradas no autorizadas, etc.).
* **Gestión de Unidades y Conductores:** Módulo para administrar la información de las unidades y el personal de conducción.
* **Interfaz Intuitiva:** Diseño amigable y fácil de usar para los supervisores.

---

## 🛠️ Tecnologías Utilizadas

* **Frontend:** HTML, CSS, JavaScript
* **Backend:** ASP.NET Core (MVC, C#)
* **Base de Datos:** SQL Server
* **Servidor:** IIS

---

## ⚙️ Configuración y Ejecución (Modo Desarrollo)

Sigue estos pasos para configurar y ejecutar la aplicación en tu entorno local:

### Prerrequisitos

Asegúrate de tener instalados los siguientes componentes:

* **.NET SDK:** 8.0
* **Visual Studio** (recomendado para desarrollo con .NET) o **Visual Studio Code**
* **SQL Server:** Una instancia de SQL Server (LocalDB, Express, Developer, etc.)
* **SSMS (SQL Server Management Studio)** o alguna herramienta para gestionar la base de datos.

### Instalación

1.  **Clona el repositorio:**
    ```bash
    git clone https://github.com/FONTUR-GT/sistema-supervision-transporte.git
    cd sistema-supervision-transporte
    ```

2.  **Restaura las dependencias de NuGet:**
    ```bash
    dotnet restore
    ```

3.  **Configura la base de datos:**
    * Abre el archivo `appsettings.json` (o `appsettings.Development.json`) y configura tu cadena de conexión a SQL Server en la sección `ConnectionStrings`.

    Ejemplo:
    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FonturTransporteDB;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```
    * Ejecuta las migraciones de la base de datos (si usas Entity Framework Core):
        ```bash
        dotnet ef database update
        ```
        (Asegúrate de que tus proyectos de migración estén configurados correctamente).

### Ejecución

1.  **Compila y ejecuta la aplicación:**
    ```bash
    dotnet run
    ```

    Alternativamente, puedes abrir la solución en **Visual Studio** y ejecutarla desde allí.

La aplicación debería estar disponible en `https://localhost:[PUERTO]` (el puerto se mostrará en la consola o en la configuración de lanzamiento de Visual Studio).

---

## 📄 Licencia

Este proyecto está bajo la licencia [Menciona la licencia, e.g., MIT, Apache 2.0, etc.]. Consulta el archivo `LICENSE` para más detalles.

---

## ✉️ Contacto

Para cualquier consulta o soporte, puedes contactar a la Gerencia de Tecnología de FONTUR a través de:

* **Sitio Web:** [www.fontur.gob.ve]

---
