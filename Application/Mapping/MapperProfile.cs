using AutoMapper;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Domain.Entities;

namespace TicketsMS.Application.Mapping
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<TicketRequestDTO, Tickets>().ReverseMap();
        }
    }
}
