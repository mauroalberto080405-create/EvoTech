# Proceso "Alerta de stock bajo" · AS-IS / TO-BE

**Mejora T3:** Stock mínimo por producto (rama `feature/stock-minimo-producto`).

## AS-IS (T2)

```mermaid
flowchart LR
    A[Usuario abre Alertas de stock bajo] --> B[Escribe un número global de mínimo, p. ej. 10]
    B --> C[El sistema lista TODO producto con stock <= ese número]
    C --> D[El usuario decide qué reponer]
```

Problema: un mismo número no sirve para todos (un cargador necesita 30 de colchón; una laptop, 2).

## TO-BE (T3)

```mermaid
flowchart LR
    A[Admin/Vendedor define el Stock mín. de cada producto en Productos] --> B[Se guarda en Producto.StockMinimo]
    B --> C[Usuario abre Alertas de stock bajo]
    C --> D[El sistema compara Stock <= StockMinimo de CADA producto]
    D --> E[Lista los productos críticos primero]
    E --> F[Aumentar stock al proveedor]
```

Además: en **Productos** hay un buscador (nombre, marca o proveedor) y las filas con stock bajo se pintan en rojo.
Las bases de datos antiguas se actualizan solas (`StockMinimo` = 5 por defecto, sin perder datos).
