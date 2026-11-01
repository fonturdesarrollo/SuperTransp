# SuperTransp

Aplicación web de FONTUR para la supervisión de unidades de transporte público en Venezuela: organizaciones (PTG), asociados, vehículos, supervisiones, trámites y pagos, reportes y mapas.

ASP.NET Core MVC sobre **.NET 8**, SQL Server, Razor + jQuery, publicada en IIS.

## Estructura
- `SuperTransp/Controllers/`: MVC. Heredan de `BaseController`, que pone `ViewBag.EmployeeName` desde la sesión. `ApiController` es la API con JWT.
- `SuperTransp/Core/`: acceso a datos y servicios. Las interfaces están en `Core/Interfaces.cs` y se registran en `Program.cs`.
- `SuperTransp/Models/`: ViewModels.
- `SuperTransp/Views/`: Razor. `wwwroot/js/` tiene el JS por pantalla (`driver-add.js`, `supervision-edit.js`, ...).
- `Database/` y `SuperTransp/Database/`: algunos scripts `.sql`. **No son el esquema completo.** La mayoría de las vistas y SPs solo existen en la BD.

## Ejecutar
- Compilar: `dotnet build SuperTransp.sln`
- Ejecutar: `dotnet watch run --project SuperTransp/SuperTransp.csproj --launch-profile http` → http://localhost:5073
- En VS Code: F5 ("SuperTransp (web)"), o la tarea `watch`.
- No hay proyecto de pruebas. Verifica compilando y probando en el navegador o contra la BD.

## Windows y macOS
El proyecto se desarrolla en los dos sistemas, y cambiar de uno a otro debe ser transparente.
- `.vscode/` se comparte: lo exclusivo de Mac va en bloques `"osx"`, y toda tarea `shell` necesita una variante `"windows"` (PowerShell). No rompas ninguno de los dos.
- En Mac, .NET está en `~/.dotnet`, que no está en el PATH por defecto; lo agregan los bloques `osx`. SQL Server corre en Docker: contenedor `supertransp-sql`, volumen `supertransp-sql-data`, tareas `sql: start/stop (docker)`.
- En Windows, SQL Server es local (SQLEXPRESS, autenticación de Windows).
- **`SuperTransp/appsettings.json` NO está en git** (lo ignora `.gitignore`). Cada máquina tiene el suyo, con su cadena de conexión y sus claves. Si agregas una clave de configuración, avisa que hay que copiarla a mano a la otra máquina y a producción. Nunca lo agregues al repo.

## Base de datos (reglas importantes)
- Acceso con ADO.NET puro (`SqlConnection`/`SqlCommand`/`SqlDataReader`), con la cadena `connectionString`. No hay EF.
- Convenciones: tablas sin prefijo (`Bank`, `ProcedureStatus`); vistas de consulta y SPs de escritura con prefijo `SuperTransp_` (`SuperTransp_ProcedureByDriverDetail`, `SuperTransp_...AddOrEdit`).
- **Hay 3 BDs independientes**: la local de Windows, la local de Mac (Docker) y producción (sin acceso desde desarrollo). Ningún cambio de esquema se propaga solo. Cada cambio de tabla, vista o SP se entrega como **script idempotente** (`CREATE OR ALTER` para vistas y SPs, `IF NOT EXISTS` para tablas, columnas y constraints) para aplicarlo en las otras.
- El usuario edita vistas y SPs directamente en SSMS, fuera del repo. **Antes de confiar en código de `Core/`, verifica contra la BD real** que las columnas existen (`SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.X'))`).
- Errores que ya ocurrieron y no hay que repetir:
  - `(string)dr["X"]` sobre columnas que pueden ser NULL o que no son string: usa un chequeo de `DBNull` o `.ToString()`/`Convert.To*`.
  - Leer una sola fila sin `if (dr.Read())`: el caso de cero filas suele ser el más común.
  - Nombres de parámetro copiados de otro método (`@AccounTypeId` en lugar de `@ProcedureId`).
  - Columnas NOT NULL sin `DEFAULT` que el SP de INSERT no envía.

## Convenciones de código
- **Idioma:** comentarios, variables, funciones e ids internos en **inglés**. Todo lo que ve el usuario final (labels, botones, mensajes, columnas, `alert`, `TempData`) en **español**. No traduzcas siglas del dominio: `Rif`, `Bcv`, `Ptg`.
- **Terminología de la UI:** "asociado" (no "socio"), "flota habilitada" (no "cupo"), "asientos" (no "puestos").
- **JSON:** `return Json(...)` serializa en camelCase; `JsonSerializer.Serialize` dentro de Razor, en PascalCase. Prefiere un endpoint AJAX antes que serializar colecciones grandes inline.
- Si un endpoint hace `Json(data.Select(m => new { ... }))` y agregas un campo al ViewModel, **agrégalo también a la proyección anónima**.
- **Formularios:** para enviar desde JS usa `form[0].submit()` (nativo). `$(form).submit()`/`.trigger('submit')` en jQuery 3 no hace el POST.
- **Hay un antiforgery global** (`AutoValidateAntiforgeryTokenAttribute`): los POST por AJAX deben enviar el token.
- **`<script>` inline en `.cshtml`:** no pongas HTML literal (`<html>`, `</style>`, ...) dentro de strings JS; construye nodos DOM.
- `Geography.GetAllMunicipalities()` no mapea `StateId`. Para filtrar por estado usa `GetMunicipalityByStateId` vía AJAX.
- **Estilo de UI:** sigue `Views/Reports/Dashboard.cshtml`: gradiente `#667eea → #764ba2`, fondo `#f2f4f8`, tarjetas blancas con `border-radius: 16px`, fuente Segoe UI.

## Cambios temporales marcados
Busca estas marcas antes de tocar las áreas relacionadas:
- `TEMP-EMAIL`: el email no es obligatorio (vacío → "N/D").
- `TEMP-HIDDEN`: elementos "Universo" ocultos en dashboards.
- `CheckDuplicatedSerials=false`: validación de seriales desactivada.
- `Bdv:Enabled=false`: la integración BDV C2P está lista pero deshabilitada; el wizard simula el pago.

## Git
- Se trabaja en `develop`; `master` recibe merges desde `develop`.
- No hagas commit ni push sin que el usuario lo pida.
