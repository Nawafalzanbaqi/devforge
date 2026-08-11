using System;
using System.Threading.Tasks;
using DevForge.ControlPlane.Domain.Entities;

namespace DevForge.ControlPlane.Application.Common.Interfaces;

public interface IBillingService
{
    Task<Invoice> GenerateInvoiceAsync(Guid billingAccountId, DateTimeOffset start, DateTimeOffset end);
}
