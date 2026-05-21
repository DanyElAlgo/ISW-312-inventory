# Examen Parcial — Daniel Roland Peñaranda Colque

## Sección 1 — Identificación

- **Nombre completo:** Daniel Roland Peñaranda Colque
- **Pareja asignada para el sábado:**
- **Repositorio de Inventario:** [https://github.com/DanyElAlgo/ISW-312-inventory]
- **Repositorio de Ventas:** [https://github.com/DanyElAlgo/ISW-312-inventory] (este mismo)
- **Contrato API acordado en grupo:** [link al archivo contrato-api.yaml en este repo]
- **URL del Swagger autogenerado** (cuando levantás el backend localmente): http://localhost:5002/swagger/index.html

## Sección 2 — Decisiones técnicas con snippets

### 2.1 Árbol de carpetas del backend de Ventas

Pegá la estructura de carpetas de tu proyecto de Ventas. Ejemplo:

```
Sales.API/
├── Controllers/
├── DTOs/
├── HttpClients/
├── Migrations/
├── Models/
├── Repositories/
├── Services/
```

Explicá en 2-3 líneas por qué la organizaste así.

Lo hice de esta forma para tener una estructura simple y directa (parecida a Clean Architecture, pero sin interfaces porque a mi parecer son redundantes). Controllers, DTOs, Servicios y Modelos. La carpeta HttpClients aloja las solicitudes que se enviarán a inventario para mantenerlo separado del resto y así conservar la claridad.

### 2.2 Flujo de "registrar una venta"

Pegá los snippets del código que se ejecuta cuando un usuario confirma una venta, en orden:

1. El endpoint que recibe el request (Controller).
2. La capa intermedia que procesa la lógica (Service / Use Case / Handler).
3. La parte que llama al Inventario del compañero (HttpClient o equivalente).
4. La parte que persiste la venta en tu BD.

Explicá en 3-5 líneas por qué dividiste así las responsabilidades.

### 2.3 Llamada al Inventario del compañero

Pegá el código exacto donde tu Ventas llama al API del Inventario del compañero.

Respondé brevemente:
- ¿Qué pasa si el compañero responde con código 200 OK?
- ¿Qué pasa si responde con 404 o 500?
- ¿Qué pasa si el compañero está caído (timeout)?

### 2.4 Configuración de la URL del compañero

Pegá:
- La línea relevante de tu `.env.example` o `appsettings.json`.
- El código que lee esa configuración y la usa para construir la llamada HTTP.

Explicá en 1 línea cómo cambiarías esa URL si el sábado tu pareja levanta su backend en otra IP.

## Sección 3 — Sobre el trabajo en grupo del contrato API

- **3.1** ¿Hubo desacuerdos al definir el contrato? ¿Cuáles?
- **3.2** ¿Cómo se resolvieron?
- **3.3** ¿Qué propusiste vos específicamente que quedó en el contrato final?

## Sección 4 — Teoría aplicada

Respondé cada pregunta en 1-2 párrafos. Está permitido usar IA para mejorar redacción, pero las respuestas deben hacer referencia explícita a tu propio código o decisiones.

**4.1** Tu compañero te avisa que va a cambiar el campo `cantidad` por `qty` en su respuesta del endpoint de stock. Tu sistema ya consume ese endpoint. Explicá qué riesgos genera ese cambio y qué prácticas conocés para evitar que un cambio así rompa los sistemas que dependen de su API.

Usar qty, aunque suene más sencillo y hasta obvio, es mala práctica porque es un diminutivo, qué tal si este diminutivo tiene otro significado en otros sistemas? Es por eso que se sugiere siempre utilizar nombre completos. Y esto es solo la nomenclatura, aún no se considera el hecho de que los sistemas ya consumen el endpoint, en casos así, es mucho peor ya que los otros sistemas no detectarán el nombre al ser este distinto, causando nulos, perdidas de información y caídas de sistema si los campos son obligatorios.

Si se va a realizar un cambio de ese tipo, no solo hay que informar al equipo completo, también hay que llegar a un acuerdo sobre si es verdaderamente necesario (para que TODOS apliquen el mismo cambio) o si es irrelevante (nadie cambia nada, sigue siendo `cantidad`).

**4.2** Tu sistema de Ventas hace una petición al Inventario para descontar stock. La red se cae justo después de que Inventario procesó el descuento pero antes de que la respuesta llegue a Ventas. ¿Qué problema se genera? ¿Cómo lo manejarías?

**4.3** Si el Inventario del compañero está caído, ¿debería tu Ventas permitir seguir registrando ventas? Justificá considerando ventajas y desventajas de cada postura. ¿Qué hace TU sistema hoy en ese caso?

**4.4** Explicá por qué tener la URL del compañero hardcodeada como `http://localhost:5000` es un problema. ¿Cuál es la solución correcta y cómo la implementaste vos?