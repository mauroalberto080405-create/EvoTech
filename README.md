# EvoTech · Sistema de ventas e inventario

Aplicativo de escritorio que automatiza **la venta en mostrador y el control de inventario** de EvoTech, tienda de equipos y accesorios tecnológicos.

**Tecnología:** C# · .NET 8 · Windows Forms · SQLite (`Microsoft.Data.Sqlite`) · inyección de dependencias (`Microsoft.Extensions.DependencyInjection`) · principios SRP, OCP y DIP.

## Estructura

```
EvoTech_Proyecto/
├── TiendaTech/                 Solución del aplicativo (abrir TiendaTech.sln)
│   ├── TiendaTech.Models/      Entidades (Cliente, Producto, Venta...)
│   ├── TiendaTech.Data/        Repositorios: BaseDatos/ (SQLite) y Memoria/
│   ├── TiendaTech.Business/    Servicios con las reglas del negocio
│   ├── TiendaTech.UI/          12 pantallas + Program.cs (composition root)
│   ├── BaseDatos/TiendaTech.sql  Script de las 8 tablas
│   └── LEEME.txt               Detalle de pantallas y cambios de la versión 2
├── docs/                       Entregas, diagramas, IC y solicitudes de cambio
│   └── T2/                     Presentación de la T2
└── .github/                    Plantilla de pull request
```

## Cómo ejecutar

1. Instalar **Visual Studio 2022** con la carga *Desarrollo de escritorio con .NET* (o el SDK de .NET 8 o superior).
2. Abrir `TiendaTech/TiendaTech.sln`. La primera vez se necesita internet para restaurar los paquetes NuGet.
3. Ejecutar con **F5** (el proyecto de inicio es `TiendaTech.UI`).
   - También desde una terminal: `dotnet run --project TiendaTech/TiendaTech.UI`
4. Los usuarios de demostración están definidos en los datos iniciales (`DbInicializador.cs`). Son credenciales **solo de prueba** y no deben usarse en un entorno real.

La base de datos `EvoTech.db` se crea sola junto al ejecutable (`TiendaTech.UI/bin/Debug/net8.0-windows/`). **No se versiona**: para volver a los datos de ejemplo, cierren el programa y borren ese archivo.

## Versiones

| Tag | Descripción |
|---|---|
| `v1.0-T2` | Versión presentada en la T2 (línea base) |
| `v2.0-T3` | Versión con las mejoras de la T3 *(pendiente)* |

Para comparar las versiones: `git diff v1.0-T2 v2.0-T3 --stat`

## Cómo contribuir

Ver [docs/GUIA_GIT.md](docs/GUIA_GIT.md): cada mejora se trabaja en su propia rama y entra a `main` mediante un pull request que revisa el líder.
