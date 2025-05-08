## Reto Técnico

### Arquitectura 
La solución sigue los principios de **Arquitectura Hexagonal**, estructurada en los siguientes proyectos:

- **Ports**  
  Implementa los endpoints para la creación y consulta de transacciones. Incluye los _consumers_ de los _topics_ de Kafka, ya que están implementados como `BackgroundServices`. 
  Ademas, agrega las dependencias de persistencia de datos en PostreSQL y Redis.

- **Adapters**    
  Implementa los _producers_ de los topics de Kafka. 

- **Application**  
  Contiene la lógica de negocio implementada de la aplicación.

- **Domain**  
  Define las entidades, contratos (interfaces) de los repositorios y las reglas de negocio puras.

- **Infrastructure**  
  Agrega e implementa los servicios por los proyectos de Application, Ports y Adapters.

### Nomenclatura de los proyectos:

 Los nombres de los proyectos siguen una convención basada en el microservicio y su propósito.

  El nombrado de los proyectos esta divido por el microservicio en el que se usa:

- Antifraud
- Transaction
    
**Tipo de proyecto.** 

- Ports
- Adptares
- Application
- Domain
- Infrastructure

**Tecnología utilizada:** (opcional, como tercer segmento)

- Redis
- Kafka
- Postgres
- Api

Por ejemplo: Antifraud.Adapter.KafkaProducer o Antifraud.Ports.Redis.

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