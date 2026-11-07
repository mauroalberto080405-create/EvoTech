# Guía de trabajo con Git · EvoTech (T3)

**Plataforma:** GitHub · **Rama principal:** `main` · **Línea base:** tag `v1.0-T2`

Reglas del equipo:
1. Nadie trabaja directamente en `main`. Cada mejora va en su **propia rama**.
2. Cada mejora entra a `main` solo mediante un **pull request (PR)** que **revisa y fusiona el líder**.
3. Cada integrante hace commits **con su propia cuenta**, así la autoría queda registrada.
4. Nunca se suben `bin/`, `obj/`, `EvoTech.db` ni credenciales (ya lo impide el `.gitignore`).

---

## Fase A · Publicar el repositorio (solo el líder, una vez)

### A1. Crear el repositorio vacío en GitHub
En github.com: **New repository** → nombre `EvoTech` → **Private** → *sin* README, .gitignore ni licencia (ya existen localmente) → **Create repository**.

> Se recomienda privado porque contiene datos de demostración y el código de una empresa. El profesor puede entrar como colaborador.

### A2. Conectar y subir
**Quién:** líder · **Carpeta:** la raíz del proyecto (donde está `README.md`)

```bash
git remote add origin https://github.com/mauroalberto080405-create/EvoTech.git
```
- **Para qué:** registra la dirección del repositorio remoto con el nombre `origin`.
- **Comprobar:** `git remote -v` muestra la URL dos veces (fetch y push).

```bash
git push -u origin main
```
- **Para qué:** sube la rama `main` y la deja enlazada con `origin/main`.
- **Comprobar:** en GitHub aparecen las carpetas `TiendaTech/` y `docs/`, y el README en la portada.

```bash
git push origin --tags
```
- **Para qué:** sube los tags (`v1.0-T2`).
- **Comprobar:** en GitHub, **Releases/Tags** muestra `v1.0-T2`.

### A3. Invitar a los integrantes
GitHub → **Settings → Collaborators → Add people** → usuario de cada integrante (y del profesor si lo pide).
- En un repositorio personal todos los colaboradores tienen permiso de escritura; el control lo da la regla de A4.
- **Comprobar:** cada integrante acepta la invitación desde su correo o desde github.com/notifications.

### A4. Proteger `main`
**Settings → Branches → Add branch ruleset** (o *Add rule*) para `main`:
- ✅ *Require a pull request before merging*
- ✅ *Require approvals*: 1
- ✅ *Block force pushes*

**Comprobar:** un `git push origin main` directo de cualquier integrante es rechazado.

> Si GitHub no permite la regla en un repositorio privado gratuito, el equipo la cumple como acuerdo y el historial lo demuestra (todos los cambios entran por PR).

### A5. Trabajar fuera de OneDrive (recomendado para todos)
OneDrive sincroniza la carpeta `.git` archivo por archivo y puede dañarla. Cada integrante, **incluido el líder**, clona en una carpeta local:

```bash
cd C:/dev
git clone https://github.com/mauroalberto080405-create/EvoTech.git
```
- **Para qué:** descarga el repositorio con todo su historial.
- **Comprobar:** `cd EvoTech` y luego `git log --oneline` muestra el commit de la línea base.

---

## Fase B · Configuración de cada integrante (una vez)

**Quién:** cada integrante · **Carpeta:** cualquiera

```bash
git config --global user.name "Nombre Apellido"
git config --global user.email "correo-de-su-cuenta-github@ejemplo.com"
```
- **Para qué:** cada commit queda firmado con ese nombre y correo. El correo debe ser el de su cuenta de GitHub para que el aporte aparezca en su perfil y en *Insights → Contributors*.
- **Comprobar:** `git config --global user.email`

---

## Fase C · Implementar una mejora (cada responsable)

Ejemplo con la mejora **H3 · Anular venta**. Los nombres de rama sugeridos son:

| Hallazgo | Rama |
|---|---|
| H2 Claves con hash | `feature/claves-hash` |
| H3 Anular venta | `feature/anular-venta` |
| H4 Comprobante de venta | `feature/comprobante-venta` |
| H5 Stock mínimo por producto | `feature/stock-minimo-producto` |
| H6 Búsqueda | `feature/busqueda` |
| H7 Cambiar clave y roles | `feature/cambiar-clave-roles` |
| Cambio de emergencia | `hotfix/...` |

**Quién:** el integrante responsable · **Carpeta:** `C:/dev/EvoTech`

```bash
git switch main
git pull
```
- **Para qué:** parte de la versión más reciente de `main`.
- **Comprobar:** `git status` dice *Your branch is up to date with 'origin/main'*.

```bash
git switch -c feature/anular-venta
```
- **Para qué:** crea la rama de la mejora y se cambia a ella.
- **Comprobar:** `git branch` marca con `*` la rama nueva.

**Programar y probar.** Compilar con F5 en Visual Studio o con:
```bash
dotnet build TiendaTech/TiendaTech.sln
```

```bash
git status
git diff
```
- **Para qué:** revisar exactamente qué archivos y líneas cambiaron antes de guardar.

```bash
git add TiendaTech/TiendaTech.Business/VentaService.cs TiendaTech/TiendaTech.UI/FrmHistorialVentas.cs
git commit -m "feat(ventas): anular venta y devolver el stock al inventario"
```
- **Para qué:** guarda una foto del cambio con un mensaje descriptivo. Conviene hacer commits pequeños: uno por paso lógico.
- **Formato del mensaje:** `tipo(área): qué hace`. Tipos: `feat` (función nueva), `fix` (corrección), `docs`, `refactor`, `chore`.
- **Comprobar:** `git log --oneline -3`

```bash
git push -u origin feature/anular-venta
```
- **Para qué:** sube la rama a GitHub.
- **Comprobar:** GitHub muestra el aviso *Compare & pull request*.

### Abrir el pull request
En GitHub: **Compare & pull request** → base `main` ← compare `feature/anular-venta` → llenar la plantilla (RFC, IC afectados, pasos de prueba, capturas del antes y el después) → **Create pull request** → asignar al líder como *Reviewer*.

---

## Fase D · Revisar e integrar (solo el líder)

**Carpeta:** `C:/dev/EvoTech`

```bash
git fetch origin
git switch feature/anular-venta
dotnet build TiendaTech/TiendaTech.sln
```
- **Para qué:** descarga la rama del integrante y la compila en el equipo del líder.
- **Comprobar:** *Compilación correcta. 0 Errores.* Luego ejecutar y seguir los pasos de prueba del PR.

```bash
git diff main...feature/anular-venta
```
- **Para qué:** muestra solo lo que aporta la rama respecto de `main`.

En GitHub, pestaña **Files changed**:
- Si hay que corregir algo: **Review changes → Request changes** con un comentario. El integrante corrige en la misma rama, hace commit y push, y el PR se actualiza solo.
- Si todo está bien: **Approve**.

Para fusionar, usar **Merge pull request → Create a merge commit** (no *Squash*): así se conservan los commits originales con el nombre de su autor.

```bash
git switch main
git pull
git log --oneline --graph -10
```
- **Comprobar:** el grafo muestra el merge y los commits del integrante con su autoría.

---

## Fase E · Resolver conflictos sin perder trabajo

Un conflicto aparece cuando dos ramas modificaron las mismas líneas (por ejemplo, ambas registran algo en `Program.cs`). Lo resuelve **quien es dueño de la rama**, no el líder.

**Quién:** el integrante · **Carpeta:** `C:/dev/EvoTech`, en su rama

```bash
git switch feature/anular-venta
git fetch origin
git merge origin/main
```
- **Para qué:** trae a su rama lo que ya se integró en `main`.
- Si Git dice `CONFLICT (content): Merge conflict in TiendaTech/TiendaTech.UI/Program.cs`, abran el archivo y busquen:

```
<<<<<<< HEAD
        services.AddTransient<FrmAnularVenta>();      // lo de su rama
=======
        services.AddTransient<FrmComprobante>();      // lo que ya está en main
>>>>>>> origin/main
```
- Casi siempre hay que **conservar ambas líneas**: borren solo las marcas `<<<<<<<`, `=======` y `>>>>>>>`.

```bash
dotnet build TiendaTech/TiendaTech.sln
git add TiendaTech/TiendaTech.UI/Program.cs
git commit -m "merge: integra main en feature/anular-venta y resuelve conflicto en Program.cs"
git push
```
- **Comprobar:** el PR en GitHub deja de mostrar *This branch has conflicts*.
- **Si se equivocan a mitad de camino:** `git merge --abort` vuelve todo a como estaba antes del merge. No se pierde nada.

**Nunca usen** `git push --force` ni `git reset --hard` sobre trabajo compartido.

---

## Fase F · Versión que se presenta (líder)

Cuando todas las mejoras estén integradas y probadas en `main`:

```bash
git switch main
git pull
dotnet build TiendaTech/TiendaTech.sln
git tag -a v2.0-T3 -m "Versión presentada en la T3"
git push origin v2.0-T3
```
- **Comprobar:** `git tag` lista `v1.0-T2` y `v2.0-T3`. La PC de la exposición debe tener exactamente esa versión: `git switch --detach v2.0-T3`.

---

## Comandos de evidencia para la exposición

| Comando | Qué demuestra |
|---|---|
| `git log --oneline --graph --all` | Ramas por mejora y su integración a `main` mediante merges |
| `git shortlog -sne v1.0-T2..v2.0-T3` | Cuántos commits hizo cada integrante en la T3 |
| `git log --author="Nombre" --oneline` | Los commits de un integrante en particular |
| `git show <id-commit> --stat` | Autor, fecha, mensaje y archivos de un aporte |
| `git diff v1.0-T2 v2.0-T3 --stat` | Todos los archivos que cambiaron desde la T2 |
| `git diff v1.0-T2 v2.0-T3 -- TiendaTech/TiendaTech.UI/FrmHistorialVentas.cs` | El antes y el después de una pantalla concreta |
| `git tag -n` | Versiones identificadas (línea base y versión presentada) |
| GitHub → *Pull requests → Closed* | Revisión y aprobación del líder en cada integración |
