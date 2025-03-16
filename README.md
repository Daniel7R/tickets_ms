# Tickets ms
Este es un ms a cargo de la gestion de la lógica de Tickets y TicketSales(venta de tickets) de eventos del proyecto E-Sports.


## Requisitos Previos
- .NET 8 SDK
- PostgreSQL
- RabbitMQ Server

## Estructura del Proyecto

- **API/Controllers/**: Contiene los controladores de la API.
- **Domain/Entities/**: Contiene los modelos o entidades de datos.
- **Application/Services/**: Contiene los servicios de la aplicación.
- **Infrastructure/Data/**: Contiene el contexto de la base de datos.
- **Infrastructure/EventBus/**: Contiene la config del event bus/rabbitmq.
- **Program.cs**: Punto de entrada del proyecto.

# Instrucciones de Ejecución
Para ejecutar el proyecto UsersAuthorization, sigue estos pasos:

- Asegúrate de tener una base de datos PostgreSQL en funcionamiento.
- Configura las variables de entorno necesarias o modifica los archivos appsettings.json, según sea necesario.
- Navega al directorio del proyecto UsersAuthorization.
- Ejecuta el siguiente comando para aplicar las migraciones de la base de datos: `dotnet ef database update`
- Ejecuta el siguiente comando para iniciar el proyecto: `dotnet run`

Esto iniciará el proyecto y estará listo para poder ser usado.

Generar Documentación con Swagger
Swagger automáticamente genera la documentación de la API. Para ver la documentación generada, inicia la aplicación y navega a http://localhost:<puerto>/swagger.


## Enpoints Controllers
- [GET] `api/v1/tickets`: este metodo es el encargado de obtener los tickets de los torneos pendientes(tickets con estado generated) para los participantes del torneo, esta operacion recibe como parametro de Query (`QueryParam`) un parametro llamado `idTournament` el cual es obligatorio, en caso
***URI***: `/api/v1/tickets?idTournament=2`

***Respuesta:*** 
```
{
    "result": [
        {
            "id": 11,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 12,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 13,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 14,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 15,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 16,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 17,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 18,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 19,
            "idTournament": 2,
            "type": "PARTICIPANT"
        },
        {
            "id": 20,
            "idTournament": 2,
            "type": "PARTICIPANT"
        }
    ],
    "message": ""
}
```

- [POST] `api/v1/tickets/use`: este metodo se encarga de hacer diferentes validaciones cuando un user desea hacer uso del ticket, se hacen validaciones de que el ticket(codigo de ticket) sea valido, el ticket corresponda al usuario, ademas  de corresponder al torneo o partido al que se desea unir(*este endpoint requiere un api-key que requiere para su consumo, limitando asi el acceso*, header-name: **x-api-key**)

***URI***: `/api/v1/tickets/use`

***Headers:***
    **x-api-key**: example-value

***Body**:
```
{
  "code": "string",
  "idUser": 0,
  "idMatch": 2,
  "type": "VIEWER"
}
```

## RabbitMQ(EventBus)
En el proyecto se hace uso de RabbitMQ como Message Broker para procesamiento de Eventos ya sean asincronos, como sincronos con el patron de integracion Request/Reply. Estas colas se usan con el fin de hacer procesamiento asincrono de las tareas, y otras, con el fin de evitar exponer endpoints hacia los usuarios.

### Colas Procesamiento Asincronico (Async Queues)
- `tournament.participant.tickets`: esta cola se encarga de recibir y procesar de manera asincrona la creacion de tickets para partcipantes de manera sincrona, cuyo evento de creacion es emitido cuando se crea un torneo dentro del sistema(Microservicio de Torneos)

- `ticket.participant.sale`: esta cola se encarga de generar la venta exitosa que se proceso en el microservicio de Pagos/Transacciones, una vez se confirma la compra de manera exitosa, se asocia dicha transaccion como una venta de ticket para un participante dentro del torneo, todo esto de manera asincrona.

- `ticket.viewer.sale`: esta cola se encarga de generar el ticket que se realiza para un espectador desde el Microservicio de transacciones una vez confirmada la compra, se crea el ticket, de espectador para el evento(partido), y a su vez se asocia la venta del ticket, para despues poder ser usado por el usuario en el evento.

### Colas Procesamiento Sincrono(Request/Reply Queues)
- `ticket.info`: esta cola se encarga de retornar informacion basica del ticket de acuerdo a un id de tipo `int` proporcionado, y retorna las siguientes propiedades, `idTicket`,`status`,`type` y `idTournament`, todo esto con el fin de realizar diferentes validaciones, como si el ticket existe, el estado del ticket sea valido, el tipo de ticket tambien sea valido, y el id del torneo para validaciones adicionales
- `ticket.user.tournament`: se encarga de validar que la venta de un ticket de tipo `PARTICIPANT`de acuerdo a un `idUser` y `idTournament` proporcionadosm retornando si la venta del ticket fue exitosa de acuerdo a los datos proporcionados