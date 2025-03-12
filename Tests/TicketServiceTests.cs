
using AutoMapper;
using TicketsMS.Application.Services;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Domain.Exceptions;
using TicketsMS.Infrastructure.Repository;
using TicketsMS.Application.DTOs.Response;
using Moq;
using Xunit;
using TicketsMS.Application.Interfaces;
namespace TicketsMS.Tests
{

    public class TicketServiceTests
    {
        private readonly Mock<IRepository<Tickets>> _mockTicketRepository;
        private readonly Mock<ICustomTicketQueriesRepo> _mockCustomTicketQueriesRepo;
        private readonly Mock<IEventBusProducer> _mockEventBusProducer;
        private readonly Mock<IMapper> _mockMapper;
        private readonly TicketService _ticketService;

        public TicketServiceTests()
        {
            _mockTicketRepository = new Mock<IRepository<Tickets>>();
            _mockEventBusProducer = new Mock<IEventBusProducer>();
            _mockCustomTicketQueriesRepo = new Mock<ICustomTicketQueriesRepo>();
            _mockMapper = new Mock<IMapper>();

            _ticketService = new TicketService(
                _mockTicketRepository.Object,
                _mockEventBusProducer.Object,
                _mockCustomTicketQueriesRepo.Object,
                _mockMapper.Object
             );

        }

        [Fact]
        public async Task GenerateTicket_ShouldCreateParticipantTicket_WhenTypeIsParticipant()
        {
            // arrange
            int eventId = 1;
            bool isFree = true;
            decimal price = 0;

            // act
            var result = await _ticketService.GenerateTicket(TicketType.PARTICIPANT, eventId, isFree, price);

            // assert
            Assert.NotNull(result);
            Assert.Equal(TicketType.PARTICIPANT, result.Type);
            Assert.Equal(TicketStatus.GENERATED, result.Status);
            Assert.Equal(eventId, result.IdTournament);
            Assert.StartsWith("TKT-", result.Code);
        }

        [Fact]
        public async Task GenerateTicketParticipant_ShouldCreateTicket_WithCorrectProperties()
        {
            // arrange
            int tournamentId = 10;
            bool isFree = false;
            decimal price = 100;

            // act
            var result = await _ticketService.GenerateTicketParticipant(tournamentId, isFree, price);

            // assert
            Assert.NotNull(result);
            Assert.Equal(TicketType.PARTICIPANT, result.Type);
            Assert.Equal(TicketStatus.GENERATED, result.Status);
            Assert.Equal(tournamentId, result.IdTournament);
            Assert.Equal(price, result.Price);
        }

        [Fact]
        public void GenerateTicketViewer_ShouldThrowException_IfPriceIsZeroAndNotFree()
        {
            // arrange
            int matchId = 5;
            bool isFree = false;
            decimal price = 0;

            // act & assert
            var exception = Assert.Throws<BusinessRuleException>(() =>
                _ticketService.GenerateTicketViewer(matchId, isFree, price));

            Assert.Equal("Price must be higher than 0", exception.Message);
        }

        [Fact]
        public void GenerateTicketViewer_ShouldCreateViewerTicket_WithValidData()
        {
            // arrange
            int matchId = 7;
            bool isFree = true;
            decimal price = 0;

            // act
            var result = _ticketService.GenerateTicketViewer(matchId, isFree, price);

            // assert
            Assert.NotNull(result);
            Assert.Equal(TicketType.VIEWER, result.Type);
            Assert.Equal(TicketStatus.GENERATED, result.Status);
            Assert.Equal(matchId, result.IdMatch);
        }

        [Fact]
        public void GenerateTicketCode_ShouldReturnValidCode()
        {
            // act
            var code = _ticketService.GenerateTicketCode();

            // assert
            Assert.NotNull(code);
            Assert.Matches(@"^TKT-\d{14}-[A-Z0-9]{6}$", code);
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldThrowException_WhenBothIdsAreNull()
        {
            // arrange
            var invalidTicket = new Tickets { IdMatch = null, IdTournament = null };

            // act & assert
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _ticketService.CreateTicketAsync(invalidTicket));

            Assert.Equal("Both ticket and tournament id can't be null at the same time", exception.Message);
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldCallRepository_AddAsync()
        {
            // arrange
            var ticket = new Tickets { IdTournament = 1 };

            _mockTicketRepository.Setup(repo => repo.AddAsync(ticket)).ReturnsAsync(It.IsAny<Tickets>());

            // act
            var result = await _ticketService.CreateTicketAsync(ticket);

            // assert
            _mockTicketRepository.Verify(repo => repo.AddAsync(ticket), Times.Once);
            Assert.NotNull(result);
        }
    }
}
