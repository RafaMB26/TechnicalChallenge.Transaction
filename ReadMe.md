## Reto Técnico

### Diagrama

![Diagrama](/Diagram.png)

El proyecto consiste en dos web api's en .net

- Transactions Web Api
- Antifraud Web Api

Ambos web apis se comunican por medio de Kafka teniendo los siguientes topics

- topic-transaction
- topic-transaction-status

Cuando una transaccion es creada por medio del endpoint de tipo POST, la transaccion se guarda en la base de 
datos con el estado Pendiente y se publica el topico `topic-transaction`. Este es recibido por _consumer_ del web api _Anti-Fraud_ y 
se evalua si cumplen con las condiciones para que la transaccion sea aprobada. Una vez que ha sido evaluada, 
se envia un nuevo topico a Kafka llamado `topic-transaction-status` con la resolucion de si la transaccion ha sido aprobada o rechazada. 
El topico es consumido por el web api de _Transactions_ y se actualiza de acuerdo a lo decidido en el sweb api _Antifraud_.

La base de datos en Redis fue utilizada para guardar y consultar el balance de las transacciones diarias de la cuenta origen.

## Ejecucion del proyecto

1. Ejecutar el docker-compose
1. Ejecutar la solucion con la configuracion para que los proyectos de Web API con el sufijo _.Presentation_ sean ejecutados al mismo tiempo
1. Ejecutar la siguiente peticion de ejemplo para comenzar con el flujo 

``
curl --request POST \
  --url https://localhost:7137/ \
  --header 'accept: */*' \
  --header 'content-type: application/json' \
  --data '{
  "sourceAccountId": "5A9D579B-62C8-4608-8466-C5F343BF121C",
  "targetAccountId": "C841A560-EDA7-4E90-A36A-154B5C72AC31",
  "transferTypeId": 1,
  "value": 200
}'
``

## Componentes de la solucion

1. EntityFramework (usando Code-First)
1. Confluent.Kafka
1. Stack Exchange Redis
1. Swashbuckle (Swagger)