using AutoMapper;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Messages.Request;
using TicketsMS.Application.Messages.Response;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Domain.Exceptions;
using TicketsMS.Infrastructure.Repository;

namespace TicketsMS.Application.Services
{
    public class TicketService : ITicketService, IGenerateTicket
    {
        private readonly IRepository<Tickets> _ticketRepository;
        private readonly IEventBusProducer _eventBusProducer;
        private readonly ICustomTicketQueriesRepo _customQueriesRepo;
        private readonly IMapper _mapper;
        private const string TICKET_PREFIX = "TKT";
        private const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int SIZE_CHAR_CODE = 6;

        public TicketService(IRepository<Tickets> ticketRepository, IEventBusProducer eventBusProducer,ICustomTicketQueriesRepo customTicketQueriesRepo, IMapper mapper)
        {
            _customQueriesRepo = customTicketQueriesRepo;
            _ticketRepository = ticketRepository;
            _eventBusProducer = eventBusProducer;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TicketsDetailsDTO>> GetTicketsByUser(int idUser)
        {

            var ticketsUser = await _customQueriesRepo.GetTicketsByUser(idUser);

            var idsTorneos = ticketsUser.Select(x => x.IdTournament ?? 0).ToList();
            var tournaments = await _eventBusProducer.SendRequest<List<int>, IEnumerable<GetTournamentBulkResponse>>(idsTorneos, Queues.Queues.GET_BULK_TOURNAMENTS);
            //FALTAN TICKETS DE PARTIDOS/VIEWERS

            IEnumerable<TicketsDetailsDTO> response =(from t in ticketsUser 
                                                      join tr in tournaments on  t.IdTournament equals tr.Id into tournamentGroup
                                                      from tournament in tournamentGroup.DefaultIfEmpty()
                                                      select new TicketsDetailsDTO
                                                      {
                                                          Id = t.Id,
                                                          Code =t.Code,
                                                          Type = t.Type,
                                                          IdTournament= tournament.Id,
                                                          Name = tournament.Name
                                                      });

            return response;
        }

        public Task<Tickets> GenerateTicket(TicketType type, int idEvent, bool isFree, decimal price)
        {
            Tickets ticket;

            if (type == TicketType.PARTICIPANT)
            {
                ticket = new Tickets
                {
                    IdTournament = idEvent,
                    Code = GenerateTicketCode(),
                    Type = TicketType.PARTICIPANT,
                    Status = TicketStatus.GENERATED,
                };
            }
            else
            {
                ticket = new Tickets
                {
                    IdMatch = idEvent,
                    Code = GenerateTicketCode(),
                    Type = TicketType.VIEWER,
                    Status = TicketStatus.GENERATED
                };

            }

            if (!isFree && price > 0)
            {
                ticket.Price = price;
            }

            return Task.FromResult(ticket);
        }

        public Task<Tickets> GenerateTicketParticipant(int idTournament, bool isFree, decimal price)
        {

            var ticket = new Tickets
            {
                IdTournament = idTournament,
                Code = GenerateTicketCode(),
                Type = TicketType.PARTICIPANT,
                Status = TicketStatus.GENERATED,
            };

            if (!isFree && price > 0)
            {
                ticket.Price = price;
            }

            return Task.FromResult(ticket);
        }

        /// <summary>
        ///     This method generates a ticket for viewer
        /// </summary>
        /// <param name="idMatch"></param>
        /// <param name="isFree"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        /// <exception cref="BusinessRuleException"></exception>
        public Tickets GenerateTicketViewer(int idMatch, bool isFree, decimal price)
        {
            if (!isFree && price == 0) throw new BusinessRuleException("Price must be higher than 0");
            var ticket = new Tickets
            {
                IdMatch = idMatch,
                Code = GenerateTicketCode(),
                Type = TicketType.VIEWER,
                Status = TicketStatus.GENERATED
            };

            return ticket;
        }

        public string GenerateTicketCode()
        {
            Span<char> builderCode = stackalloc char[SIZE_CHAR_CODE];
            Span<byte> randomBytes = stackalloc byte[SIZE_CHAR_CODE];

            string timestamp = DateTime.UtcNow.AddHours(-5).ToString("yyyyMMddHHmmss");

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < SIZE_CHAR_CODE; i++)
            {
                builderCode[i] = CHARACTERS[randomBytes[i] % CHARACTERS.Length];
            }

            var code = $"{TICKET_PREFIX}-{timestamp}-{builderCode}";

            return code;
        }


        /// <summary>
        /// VAlidates that a provided ticket is valid and it belongs to the owner
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="codeTicket"></param>
        /// <returns></returns>
        /// <exception cref="BusinessRuleException"></exception>
        public async Task ValidateTicket(int idUser, string codeTicket)
        {
            var ticket = await _customQueriesRepo.GetTicketByCode(codeTicket);
            if (ticket == null) throw new BusinessRuleException("Ticket is not valid");

            if (ticket.TicketSales.IdUser != idUser) throw new BusinessRuleException("User does not own ticket");

            if (
                ticket.Status.Equals(TicketStatus.CANCELED) || 
                ticket.Status.Equals(TicketStatus.GENERATED) || 
                ticket.Status.Equals(TicketStatus.USED)
              ) 
                throw new BusinessRuleException("Ticket is already used or invalid");

            //ticket TYPE
            switch (ticket.Type)
            {
                case TicketType.PARTICIPANT:
                //OR    
                case TicketType.VIEWER:
                    await UpdateTicketStatus(ticket.Id, TicketStatus.ACTIVE);
                    break;
                default:
                    throw new BusinessRuleException($"Ticket type is not valid");
            }
            //UNA VEZ Acabe el evento torneo o partido, cambio el estado a USED para que no pueda volver a ser usado
        }

        /// <summary>
        /// Update a ticket status for a provided id
        /// </summary>
        /// <param name="idTicket"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public async Task UpdateTicketStatus(int idTicket, TicketStatus newStatus)
        {
            await _customQueriesRepo.UpdateStatus(idTicket, newStatus);
        }

        public async Task<bool> UseTicket(UseTicketRequest request)
        {
            var ticket = await _customQueriesRepo.GetTicketByCode(request.Code);
            if (ticket == null) throw new BusinessRuleException("Ticket is not valid or does not exist");

            if (ticket.TicketSales.IdUser != request.IdUser) throw new BusinessRuleException("Ticket does not belong user");

            if(!ticket.Type.Equals(request.Type)) throw new BusinessRuleException("Ticket type provided is not valid");
            //GENERATED IS WHEN A TICKET IS CREATED
            //CANCELED WHEN A TICKET IS NOT VALID FOR A USER
            if (ticket.Status.Equals(TicketStatus.GENERATED)|| 
                ticket.Status.Equals(TicketStatus.CANCELED) || 
                ticket.Status.Equals(TicketStatus.USED)
               )
                throw new BusinessRuleException("Ticket is invalid or it has already been used");

            if (ticket.Type.Equals(TicketType.VIEWER) && ticket.IdMatch  != request.IdMatch)
            {
                throw new BusinessRuleException("Ticket is not valid for event");
            } else if (ticket.Type.Equals(TicketType.PARTICIPANT))
            {
                var requestValidation = new ValidateMatchTournament
                {
                    IdMatch = request.IdMatch,
                    IdTournament = (int)ticket.IdTournament
                };
                //request if match to use ticket belongs to tournament
                var isValidMatchTournmanet = await _eventBusProducer.SendRequest<ValidateMatchTournament, bool>(requestValidation, Queues.Queues.MATCH_BELONGS_TOURNAMENT);
            
                if (!isValidMatchTournmanet) throw new BusinessRuleException("Ticket is not valid for match from tournament");
            
            }

            return true;
        }

        /// <summary>
        /// Create a ticket in db, it could synchronous o asynchronous creation
        /// </summary>
        /// <param name="ticketRequest"></param>
        /// <returns></returns>
        /// <exception cref="BusinessRuleException"></exception>
        public async Task<TicketResponseDTO> CreateTicketAsync(Tickets ticketRequest)
        {
            /*if (ticketRequest == true && ticketRequest.Price <= 0)
            {
                throw new BusinessRuleException("Price must be equal to zero if it's free");
            }*/
            if (ticketRequest.IdMatch == null && ticketRequest.IdTournament == null)
            {
                throw new BusinessRuleException("Both ticket and tournament id can't be null at the same time");
            }

            //var ticket = _mapper.Map<Tickets>(ticketRequest);
            await _ticketRepository.AddAsync(ticketRequest);
            var ticketResponse = new TicketResponseDTO();
            //var ticketResponse = _mapper.Map<TicketResponseDTO>(ticketRequest);

            return ticketResponse;
        }

        public async Task<IEnumerable<TicketResponseDTO>> GetTicketsByStatus(TicketStatus status)
        {
            var tickets = await _customQueriesRepo.GetTicketsByStatus(status);
            var response = _mapper.Map<IEnumerable<TicketResponseDTO>>(tickets);

            return response;
        }
        public async Task<IEnumerable<TicketParticipantResponseDTO>> GetTicketsByStatusAndIdTournament(TicketStatus status, int idTournament)
        {
            var tickets = await _customQueriesRepo.GetTicketsByStatusAndIdTournament(status, idTournament);
            var response = _mapper.Map<IEnumerable<TicketParticipantResponseDTO>>(tickets);

            return response;
        }
    }
}
