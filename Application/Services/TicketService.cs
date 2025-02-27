using AutoMapper;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Application.Interfaces;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Domain.Exceptions;
using TicketsMS.Infrastructure.Repository;

namespace TicketsMS.Application.Services
{
    public class TicketService: ITicketService
    {
        private readonly IRepository<Tickets> _ticketRepository;
        private readonly IMapper _mapper;
        private const string TICKET_PREFIX = "TKT";
        private const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int SIZE_CHAR_CODE = 6;
        public TicketService(IRepository<Tickets> ticketRepository, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
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

            if (!isFree && price> 0)
            {
                ticket.Price = price;
            }

            return Task.FromResult(ticket);
        }

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

            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for(int i=0; i< SIZE_CHAR_CODE; i++)
            {
                builderCode[i] = CHARACTERS[randomBytes[i] % CHARACTERS.Length];
            }

            var code = $"{TICKET_PREFIX}-{timestamp}-{builderCode}";

            return code;
        }

        public async Task<TicketResponseDTO> CreateTicketAsync(Tickets ticketRequest)
        {
            /*if (ticketRequest == true && ticketRequest.Price <= 0)
            {
                throw new BusinessRuleException("Price must be equal to zero if it's free");
            }*/ 
            if(ticketRequest.IdMatch == null && ticketRequest.IdTournament == null)
            {
                throw new BusinessRuleException("Both ticket and tournament id can't be null at the same time");
            }

            //var ticket = _mapper.Map<Tickets>(ticketRequest);
            await _ticketRepository.AddAsync(ticketRequest);
            var ticketResponse = new TicketResponseDTO();
            //var ticketResponse = _mapper.Map<TicketResponseDTO>(ticketRequest);

            return ticketResponse;
        }
    }
}
