using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder
            .HasOne(b => b.CreatorUser)
            .WithMany()
            .HasForeignKey(b => b.CreatorUserId);

        builder
            .HasOne(b => b.LastModifierUser)
            .WithMany()
            .HasForeignKey(b => b.LastModifierUserId);
    }
}