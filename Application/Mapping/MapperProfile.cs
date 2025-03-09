using AutoMapper;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Domain.Entities;

namespace TicketsMS.Application.Mapping
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<TicketRequestDTO, Tickets>().ReverseMap();
            CreateMap<TicketResponseDTO, Tickets>().ReverseMap();
            CreateMap<TicketParticipantResponseDTO, Tickets>().ReverseMap();
        }
    }
}
