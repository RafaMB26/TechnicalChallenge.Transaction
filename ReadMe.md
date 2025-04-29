## Reto Técnico

### Arquitectura 
La solución sigue los principios de **Clean Architecture**, estructurada en los siguientes proyectos:

- **Presentation**  
  Implementa los endpoints para la creación y consulta de transacciones. También incluye los _consumers_ de los _topics_ de Kafka, ya que están implementados como `BackgroundServices`.

- **Application**  
  Contiene la lógica de negocio de la aplicación implementada.

- **Domain**  
  Define las entidades, contratos (interfaces) de los repositorios y las reglas de negocio puras.

- **Infrastructure**  
  Implementa los repositorios, la persistencia en base de datos y los _producers_ de los _topics_ de Kafka.


### Diagrama

![Diagrama](/Diagram.png)

El proyecto está compuesto por dos Web APIs desarrolladas en .NET:

- **Transactions Web API**
- **Antifraud Web API**

Estas se comunican entre sí utilizando Kafka a través de los siguientes _topics_:

- `topic-transaction`
- `topic-transaction-status`

### Flujo de operación:

1. Al crear una transacción vía POST, esta se guarda en la base de datos con estado Pendiente.
1. Se publica un mensaje en el topic topic-transaction.
1. La Antifraud Web API consume el mensaje, evalúa la transacción y determina si debe ser aprobada o rechazada.
1. Luego, publica un nuevo mensaje en topic-transaction-status con la decisión.
1. **Transactions Web API** consume este mensaje y actualiza el estado de la transacción en la base de datos.

*Nota*: Redis se utiliza para almacenar y consultar el balance diario de transacciones de la cuenta de origen.


## Ejecución del Proyecto

1. Ejecutar `docker-compose` para levantar los servicios necesarios (Kafka, Redis, etc.).
2. Iniciar la solución en modo múltiple, asegurándose de que los proyectos con el sufijo `.Presentation` se ejecuten simultáneamente.
3. Probar el flujo con la siguiente petición de ejemplo:

```bash
curl --request POST \
  --url https://localhost:7137/ \
  --header 'Accept: */*' \
  --header 'Content-Type: application/json' \
  --data '{
    "sourceAccountId": "5A9D579B-62C8-4608-8466-C5F343BF121C",
    "targetAccountId": "C841A560-EDA7-4E90-A36A-154B5C72AC31",
    "transferTypeId": 1,
    "value": 200
}'
```

## Componentes de la solucion

1. EntityFramework (Code-First)
1. Confluent.Kafka
1. Stack Exchange Redis
1. Swashbuckle (Swagger)