﻿using ExpenseTracker.Domain.Account.Events;
using ExpenseTracker.Domain.Account;
using Marten.Events.Aggregation;
using JasperFx.Events;

namespace ExpenseService.Infrastructure.Account;

public class AccountProjection : SingleStreamProjection<AccountReadModel, Guid>
{
    public AccountProjection()
    {
        ProjectEvent<IEvent<AccountCreatedEvent>>((view, @event) =>
        {
            var data = @event.Data;
            view.Id = data.Id;
            view.Name = data.Name;
            view.Number = data.Number;
            view.BankName = data.BankName;
            view.BankPhone = data.BankPhone;
            view.BankAddress = data.BankAddress;
            view.Enabled = true;
            view.ExpectedVersion = @event.Version;
        });
        ProjectEvent<IEvent<AccountUpdatedEvent>>((view, @event) =>
        {
            var data = @event.Data;
            if (!string.IsNullOrEmpty(data.Name))
                view.Name = data.Name;
            view.Number = data.Number;
            view.BankName = data.BankName;
            view.BankPhone = data.BankPhone;
            view.BankAddress = data.BankAddress;
            if (data.Enabled.HasValue)
                view.Enabled = data.Enabled.Value;
            view.ExpectedVersion = @event.Version;
        });
        ProjectEvent<IEvent<AccountDeletedEvent>>((view, @event) =>
        {
            var data = @event.Data;
            view.DeletedAt = data.DeletedAt;
            view.Enabled = false;
            view.ExpectedVersion = @event.Version;
        });
        ProjectEvent<IEvent<AccountWithdrawalEvent>>((view, @event) =>
        {
            var data = @event.Data;
            view.Balance -= data.Amount;
            view.ExpectedVersion = @event.Version;
        });
        ProjectEvent<IEvent<AccountDepositEvent>>((view, @event) =>
        {
            var data = @event.Data;
            view.Balance += data.Amount;
            view.ExpectedVersion = @event.Version;
        });
    }

}
