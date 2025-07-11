﻿using ExpenseService.Application.Common;
using ExpenseService.Application.Models.Accounts;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Contracts.Account;

namespace ExpenseService.Application.Account.UpdateAccount;

public record UpdateAccountCommand
{
    public UpdateAccountCommand(Guid id, long expectedVersion, string? name, string? number, string? bankName, string? bankPhone, string? bankAddress, bool? enabled)
    {
        Id = id;
        ExpectedVersion = expectedVersion;
        Name = name;
        Number = number;
        BankName = bankName;
        BankPhone = bankPhone;
        BankAddress = bankAddress;
        Enabled = enabled;
    }

    public Guid Id { get; set; }

    public long ExpectedVersion { get; set; }

    public string? Name { get; set; }

    public string? Number { get; set; }

    public string? BankName { get; set; }

    public string? BankPhone { get; set; }

    public string? BankAddress { get; set; }

    public bool? Enabled { get; set; }
}

public class UpdateAccountCommandHandler
{
    public static async Task<AccountCommandResult> Handle(UpdateAccountCommand request, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var accountAggregate = await unitOfWork.Accounts.LoadAsync(request.Id, cancellationToken) ?? throw new NotFoundException("Account not found");
        accountAggregate.Update(request.Name,
             request.Number,
             request.BankName,
             request.BankPhone,
             request.BankAddress,
             request.Enabled);
        await unitOfWork.Accounts.SaveAsync(accountAggregate, request.ExpectedVersion, cancellationToken);
        if (accountAggregate.UpdatedAt.HasValue)
            await unitOfWork.PublishAsync(new AccountUpdatedIntegrationEvent
            {
                Id = accountAggregate.Id,
                BankName = accountAggregate.BankName,
                BankPhone = accountAggregate.BankPhone,
                BankAddress = accountAggregate.BankAddress,
                UpdatedAt = accountAggregate.UpdatedAt.Value,
                ExpectedVersion = accountAggregate.Version,
                Name = accountAggregate.Name,
                Number = accountAggregate.Number,
                Enabled = accountAggregate.Enabled
            });
        await unitOfWork.CommitAsync(cancellationToken);

        return new AccountCommandResult
        {
            AccountId = accountAggregate.Id,
            NewVersion = accountAggregate.Version
        };
    }
}
