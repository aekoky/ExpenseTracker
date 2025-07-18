using AuditService.Application.Account.GetAccountById;
using AuditService.Domain.Account;
using AuditService.IntegrationTests.Setup;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Contracts.Account;

namespace AuditService.IntegrationTests.Handlers;

public class GetAccountByIdTests() : IntegrationTest
{
    [Fact]
    public async Task GetAccountById_ShouldReturnAccountWhenExists()
    {
        var accountCreatedEvent = new AccountCreatedIntegrationEvent
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ExpectedVersion = 1,
        };
        DocumentSession.Events.StartStream(accountCreatedEvent.Id, accountCreatedEvent);
        await DocumentSession.SaveChangesAsync();
        await GenerateProjectionsAsync();

        var account = await MessageBus.InvokeAsync<AccountReadModel>(new GetAccountByIdQuery(accountCreatedEvent.Id));

        Assert.NotNull(account);
        Assert.IsType<AccountReadModel>(account);
        Assert.Equal(accountCreatedEvent.Id, account.Id);
        Assert.Equal(accountCreatedEvent.ExpectedVersion, account.ExpectedVersion);
        Assert.Equal(accountCreatedEvent.Balance, account.Balance);
        Assert.Equal(accountCreatedEvent.CreatedAt, account.CreatedAt);
    }

    [Fact]
    public async Task GetAccountById_ShouldThrowWhenNotFound()
    {
        var accountId = Guid.NewGuid();
        await Assert.ThrowsAsync<NotFoundException>(async () =>
         await MessageBus.InvokeAsync<AccountReadModel>(new GetAccountByIdQuery(Guid.NewGuid())));
    }
}