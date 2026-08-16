using Microsoft.EntityFrameworkCore;
using Yildiz.CRM.Domains.Entities;

namespace Yildiz.CRM.Applications.Interfaces;

public interface ICrmDbContext
{
    DbSet<Tenant> Tenants { get; set; }
    DbSet<Customer> Customers { get; set; }
    DbSet<Policy> Policies { get; set; }
}
